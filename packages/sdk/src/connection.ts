import net from 'node:net'
import { randomUUID } from 'node:crypto'
import { existsSync, readFileSync } from 'node:fs'
import path from 'node:path'
import { stateDir } from './hash.ts'
import type {
  CommandRequest,
  CommandResponse,
  ExecuteOptions,
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

  async send(request: CommandRequest, options: ExecuteOptions = {}): Promise<CommandResponse> {
    if (!this.pipePathValue) {
      throw new Error('Pipe path is not set. Call connect() before send().')
    }

    await this.ensureConnected(this.reconnectTimeout)

    const timeout = options.timeout ?? this.commandTimeout
    return new Promise<CommandResponse>((resolve, reject) => {
      const timer = setTimeout(() => {
        this.pending.delete(request.id)
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
      }
    })

    socket.on('error', () => {
      this.connected = false
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
      if (!pending) {
        continue
      }

      clearTimeout(pending.timer)
      this.pending.delete(parsed.id)
      pending.resolve(parsed)
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
}

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms))
}
