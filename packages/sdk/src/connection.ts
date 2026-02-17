import net from 'node:net'
import { randomUUID } from 'node:crypto'
import { existsSync, readFileSync, unlinkSync } from 'node:fs'
import path from 'node:path'
import { stateDir } from './hash.ts'
import type {
  CommandRequest,
  CommandResponse,
  ServerMetadata,
  TimeoutOptions,
} from './types.ts'

const DEFAULT_CONNECT_TIMEOUT = 5_000
const DEFAULT_COMMAND_TIMEOUT = 30_000
const DEFAULT_RECONNECT_TIMEOUT = 30_000
const EXPECTED_PROTOCOL_VERSION = 1

interface PendingRequest {
  resolve: (value: CommandResponse) => void
  reject: (reason?: unknown) => void
  timer: NodeJS.Timeout
}

interface PipeConnectionOptions extends TimeoutOptions {
  projectPath?: string
}

export class PipeConnection {
  private socket: net.Socket | undefined
  private connected = false
  private intentionalDisconnect = false
  private frameBuffer = Buffer.alloc(0)
  private pending = new Map<string, PendingRequest>()
  private pipePathValue: string | undefined
  private connectInFlight: Promise<void> | undefined
  private readonly connectTimeout: number
  private readonly commandTimeout: number
  private readonly reconnectTimeout: number
  private readonly projectPath: string | undefined
  private recoveryInFlight: Promise<void> | undefined

  constructor(options: PipeConnectionOptions = {}) {
    this.connectTimeout = options.connectTimeout ?? DEFAULT_CONNECT_TIMEOUT
    this.commandTimeout = options.commandTimeout ?? DEFAULT_COMMAND_TIMEOUT
    this.reconnectTimeout = options.reconnectTimeout ?? DEFAULT_RECONNECT_TIMEOUT
    this.projectPath = options.projectPath
  }

  async connect(pipePath: string): Promise<void> {
    this.pipePathValue = pipePath
    await this.ensureConnected(this.connectTimeout)
  }

  async send(request: CommandRequest): Promise<CommandResponse> {
    if (!this.pipePathValue) {
      throw new Error('Pipe path is not set. Call connect() before send().')
    }

    await this.ensureConnected(this.reconnectTimeout)

    const timeout = this.commandTimeout
    return new Promise<CommandResponse>((resolve, reject) => {
      const timer = setTimeout(() => {
        this.pending.delete(request.id)
        const fileResult = this.readResultFromDisk(request.id)
        if (fileResult) {
          resolve(fileResult)
          return
        }

        reject(new Error(`Command timeout (${Math.round(timeout / 1000)}s) — the command may have hung Unity's main thread`))
      }, timeout)

      this.pending.set(request.id, { resolve, reject, timer })

      try {
        this.writeFrame(request)
      } catch (error) {
        clearTimeout(timer)
        this.pending.delete(request.id)
        reject(error)
      }
    })
  }

  disconnect(): void {
    this.intentionalDisconnect = true
    this.connected = false

    if (this.socket) {
      this.socket.destroy()
      this.socket = undefined
    }

    for (const [id, pending] of this.pending) {
      clearTimeout(pending.timer)
      pending.reject(new Error(`Connection closed before response for ${id}`))
    }
    this.pending.clear()
    this.connectInFlight = undefined
  }

  serverMetadata(): ServerMetadata | null {
    if (!this.projectPath) {
      return null
    }

    const metadataPath = path.join(stateDir(this.projectPath), 'server.json')
    if (!existsSync(metadataPath)) {
      return null
    }

    try {
      return JSON.parse(readFileSync(metadataPath, 'utf-8')) as ServerMetadata
    } catch {
      return null
    }
  }

  private async ensureConnected(timeout: number): Promise<void> {
    if (this.connected && this.socket && !this.socket.destroyed) {
      return
    }

    if (!this.pipePathValue) {
      throw new Error('Pipe path is not set. Call connect() before send().')
    }

    if (!this.connectInFlight) {
      this.connectInFlight = this.connectWithRetry(this.pipePathValue, timeout)
        .finally(() => {
          this.connectInFlight = undefined
        })
    }

    await this.connectInFlight
  }

  private async connectWithRetry(pipePath: string, timeout: number): Promise<void> {
    const started = Date.now()
    let delay = 200

    while (Date.now() - started < timeout) {
      try {
        await this.connectOnce(pipePath)
        this.validateProtocolVersion()
        this.requestPendingResults()
        return
      } catch {
        await sleep(delay)
        delay = Math.min(delay * 2, 5_000)
      }
    }

    throw new Error(`Connect timeout (${Math.round(timeout / 1000)}s) — is Unity open with the unibridge plugin loaded?`)
  }

  private connectOnce(pipePath: string): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      const socket = net.connect({ path: pipePath })

      const onError = (error: Error) => {
        socket.removeListener('connect', onConnect)
        reject(error)
      }

      const onConnect = () => {
        socket.removeListener('error', onError)
        this.socket = socket
        this.connected = true
        this.intentionalDisconnect = false
        this.attachSocketHandlers(socket)
        resolve()
      }

      socket.once('error', onError)
      socket.once('connect', onConnect)
    })
  }

  private attachSocketHandlers(socket: net.Socket): void {
    socket.on('data', (chunk: Buffer | string) => {
      if (typeof chunk !== 'string') {
        this.frameBuffer = Buffer.concat([this.frameBuffer, chunk])
      }
      this.parseFrames()
    })

    socket.on('close', () => {
      if (!this.intentionalDisconnect) {
        this.connected = false
        this.tryRecoverPending()
      }
    })

    socket.on('error', () => {
      this.connected = false
      this.tryRecoverPending()
    })
  }

  private parseFrames(): void {
    while (this.frameBuffer.length >= 4) {
      const messageLength = this.frameBuffer.readUInt32BE(0)
      if (this.frameBuffer.length < 4 + messageLength) {
        return
      }

      const rawMessage = this.frameBuffer.subarray(4, 4 + messageLength)
      this.frameBuffer = this.frameBuffer.subarray(4 + messageLength)

      if (rawMessage.length === 0) {
        continue
      }

      let parsed: CommandResponse
      try {
        parsed = JSON.parse(rawMessage.toString()) as CommandResponse
      } catch {
        continue
      }

      const pending = this.pending.get(parsed.id)
      if (pending) {
        clearTimeout(pending.timer)
        this.pending.delete(parsed.id)
        this.deleteResultFile(parsed.id)
        pending.resolve(parsed)
        continue
      }

      this.tryResolveRecoveredPayload(parsed)
    }
  }

  private writeFrame(payload: unknown): void {
    if (!this.socket || this.socket.destroyed) {
      throw new Error('Connection is not active')
    }

    const body = Buffer.from(JSON.stringify(payload), 'utf-8')
    const frame = Buffer.alloc(4 + body.length)
    frame.writeUInt32BE(body.length, 0)
    body.copy(frame, 4)
    this.socket.write(frame)
  }

  private validateProtocolVersion(): void {
    const metadata = this.serverMetadata()
    if (!metadata) {
      return
    }

    if (metadata.protocolVersion !== EXPECTED_PROTOCOL_VERSION) {
      throw new Error(
        `Protocol version mismatch: SDK expects ${EXPECTED_PROTOCOL_VERSION}, Unity plugin is ${metadata.protocolVersion}`,
      )
    }
  }

  private requestPendingResults(): void {
    if (!this.socket || this.socket.destroyed || this.pending.size === 0) {
      return
    }

    this.writeFrame({
      id: `recover-${randomUUID()}`,
      command: 'recoverResults',
      params: { ids: [...this.pending.keys()] },
    })
  }

  private tryRecoverPending(): void {
    if (this.pending.size === 0 || this.recoveryInFlight) {
      return
    }

    this.recoveryInFlight = this.ensureConnected(this.reconnectTimeout)
      .catch(() => {
        // Per-request timeouts and file fallback will handle failure.
      })
      .finally(() => {
        this.recoveryInFlight = undefined
      })
  }

  private tryResolveRecoveredPayload(response: CommandResponse): void {
    let payload: unknown = response.result
    if (typeof payload === 'string') {
      try {
        payload = JSON.parse(payload)
      } catch {
        return
      }
    }

    if (!payload || typeof payload !== 'object' || !('results' in payload)) {
      return
    }

    const maybeResults = (payload as { results?: unknown }).results
    if (!Array.isArray(maybeResults)) {
      return
    }

    for (const item of maybeResults) {
      if (!item || typeof item !== 'object' || !('id' in item) || typeof item.id !== 'string') {
        continue
      }
      const recoveredItem = item as Partial<CommandResponse> & { id: string }

      const pending = this.pending.get(recoveredItem.id)
      if (!pending) {
        continue
      }

      const recovered: CommandResponse = {
        id: recoveredItem.id,
        success: typeof recoveredItem.success === 'boolean' ? recoveredItem.success : false,
        result: recoveredItem.result,
        error: typeof recoveredItem.error === 'string' ? recoveredItem.error : undefined,
      }

      clearTimeout(pending.timer)
      this.pending.delete(recoveredItem.id)
      this.deleteResultFile(recoveredItem.id)
      pending.resolve(recovered)
    }
  }

  private readResultFromDisk(requestId: string): CommandResponse | null {
    if (!this.projectPath) {
      return null
    }

    const resultPath = path.join(stateDir(this.projectPath), 'results', `${requestId}.json`)
    if (!existsSync(resultPath)) {
      return null
    }

    try {
      const parsed = JSON.parse(readFileSync(resultPath, 'utf-8')) as Partial<CommandResponse>
      if (typeof parsed.id !== 'string' || parsed.id !== requestId) {
        return null
      }

      const response: CommandResponse = {
        id: parsed.id,
        success: Boolean(parsed.success),
        result: parsed.result,
        error: typeof parsed.error === 'string' ? parsed.error : undefined,
      }

      this.deleteResultFile(requestId)

      return response
    } catch {
      return null
    }
  }

  private deleteResultFile(requestId: string): void {
    if (!this.projectPath) {
      return
    }

    const resultPath = path.join(stateDir(this.projectPath), 'results', `${requestId}.json`)
    if (!existsSync(resultPath)) {
      return
    }

    try {
      unlinkSync(resultPath)
    } catch {
      // Best-effort cleanup.
    }
  }
}

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms))
}
