import { randomUUID } from 'node:crypto'
import { PipeConnection } from './connection.ts'
import { pipePath } from './hash.ts'
import { findUnityProject } from './project.ts'
import type { ClientOptions, ExecuteOptions, UniBridgeClient } from './types.ts'

export class UniBridgeError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'UniBridgeError'
  }
}

interface TestConnection {
  send: (req: { id: string; command: string; params: { code: string } }) => Promise<{
    id: string
    success: boolean
    result?: unknown
    error?: string
  }>
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

  return {
    projectPath,
    async execute(code: string, executeOptions: ExecuteOptions = {}): Promise<unknown> {
      if (!callerExecuteEnabled) {
        throw new UniBridgeError('Execute is disabled by client or plugin configuration.')
      }

      await connection.connect(pipePath(projectPath))

      const metadata = connection.serverMetadata()
      const serverExecuteEnabled = metadata?.capabilities?.executeEnabled ?? true
      if (!serverExecuteEnabled) {
        throw new UniBridgeError('Execute is disabled by client or plugin configuration.')
      }

      const response = await connection.send(
        {
          id: randomUUID(),
          command: 'execute',
          params: { code },
        },
        executeOptions,
      )

      if (!response.success) {
        throw new UniBridgeError(response.error ?? 'Command execution failed')
      }

      return response.result
    },
    close(): void {
      connection.disconnect()
    },
  }
}

export function createClientForTests(connection: TestConnection): UniBridgeClient {
  return {
    projectPath: '/tmp/test-project',
    async execute(code: string): Promise<unknown> {
      const response = await connection.send({
        id: randomUUID(),
        command: 'execute',
        params: { code },
      })

      if (!response.success) {
        throw new UniBridgeError(response.error ?? 'Command execution failed')
      }

      return response.result
    },
    close(): void {
      // no-op for test double
    },
  }
}
