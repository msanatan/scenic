import { randomUUID } from 'node:crypto'
import { PipeConnection } from './connection.ts'
import { pipePath } from './hash.ts'
import { findUnityProject } from './project.ts'
import type { ClientOptions, CommandResponse, UniBridgeClient } from './types.ts'
import { runExecute } from './commands/execute/client.ts'
import { runSceneActive } from './commands/scene/client.ts'
import { runStatus } from './commands/status/client.ts'
import type { CommandRuntime, ExecuteGuard } from './commands/runtime.ts'

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

function createRuntime(
  sendCommand: (command: string, params: Record<string, unknown>) => Promise<CommandResponse>,
  ensureExecuteEnabled: () => void,
): CommandRuntime & ExecuteGuard {
  return {
    async send(command: string, params: Record<string, unknown>): Promise<unknown> {
      return unwrap(await sendCommand(command, params))
    },
    ensureExecuteEnabled,
  }
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

  function ensureExecuteEnabled(): void {
    if (!callerExecuteEnabled) {
      throw new UniBridgeError('Execute is disabled by client or plugin configuration.')
    }

    const metadata = connection.serverMetadata()
    const serverExecuteEnabled = metadata?.capabilities?.executeEnabled ?? true
    if (!serverExecuteEnabled) {
      throw new UniBridgeError('Execute is disabled by client or plugin configuration.')
    }
  }

  const runtime = createRuntime(sendCommand, ensureExecuteEnabled)

  return {
    projectPath,

    async execute(code: string): Promise<unknown> {
      return runExecute(runtime, code)
    },

    async status() {
      return runStatus(runtime)
    },

    async sceneActive() {
      return runSceneActive(runtime)
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

  const runtime = createRuntime(sendCommand, () => {
    // no-op in test client
  })

  return {
    projectPath: '/tmp/test-project',

    async execute(code: string): Promise<unknown> {
      return runExecute(runtime, code)
    },

    async status() {
      return runStatus(runtime)
    },

    async sceneActive() {
      return runSceneActive(runtime)
    },

    close(): void {
      // no-op for test double
    },
  }
}
