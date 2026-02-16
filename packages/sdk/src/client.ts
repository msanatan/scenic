import { randomUUID } from 'node:crypto'
import { PipeConnection } from './connection.ts'
import { pipePath } from './hash.ts'
import { findUnityProject } from './project.ts'
import type { ClientOptions, CommandResponse, StatusResult, UniBridgeClient } from './types.ts'

export class UniBridgeError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'UniBridgeError'
  }
}

function unwrap(response: CommandResponse): unknown {
  if (!response.success) {
    throw new UniBridgeError(response.error ?? 'Command failed')
  }
  return response.result
}

export function createClient(options: ClientOptions = {}): UniBridgeClient {
  const projectPath = options.projectPath ?? findUnityProject()
  const connection = new PipeConnection({
    projectPath,
    connectTimeout: options.connectTimeout,
    commandTimeout: options.commandTimeout,
    reconnectTimeout: options.reconnectTimeout,
  })
  const callerExecuteEnabled = options.enableExecute ?? true

  async function sendCommand(
    command: string,
    params: Record<string, unknown>,
  ): Promise<CommandResponse> {
    await connection.connect(pipePath(projectPath))
    return connection.send({ id: randomUUID(), command, params })
  }

  return {
    projectPath,

    async execute(code: string): Promise<unknown> {
      if (!callerExecuteEnabled) {
        throw new UniBridgeError('Execute is disabled by client or plugin configuration.')
      }

      const metadata = connection.serverMetadata()
      const serverExecuteEnabled = metadata?.capabilities?.executeEnabled ?? true
      if (!serverExecuteEnabled) {
        throw new UniBridgeError('Execute is disabled by client or plugin configuration.')
      }

      return unwrap(await sendCommand('execute', { code }))
    },

    async status(): Promise<StatusResult> {
      const result = unwrap(await sendCommand('status', {}))
      return JSON.parse(result as string) as StatusResult
    },

    close(): void {
      connection.disconnect()
    },
  }
}

interface TestConnection {
  send: (req: { id: string; command: string; params: Record<string, unknown> }) => Promise<CommandResponse>
}

export function createClientForTests(connection: TestConnection): UniBridgeClient {
  async function sendCommand(
    command: string,
    params: Record<string, unknown>,
  ): Promise<CommandResponse> {
    return connection.send({ id: randomUUID(), command, params })
  }

  return {
    projectPath: '/tmp/test-project',

    async execute(code: string): Promise<unknown> {
      return unwrap(await sendCommand('execute', { code }))
    },

    async status(): Promise<StatusResult> {
      const result = unwrap(await sendCommand('status', {}))
      return JSON.parse(result as string) as StatusResult
    },

    close(): void {
      // no-op for test double
    },
  }
}
