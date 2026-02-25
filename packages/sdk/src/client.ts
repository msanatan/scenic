import { randomUUID } from 'node:crypto'
import { PipeConnection } from './connection.ts'
import { readExecuteEnabled } from './config.ts'
import { pipePath } from './hash.ts'
import { findUnityProject } from './project.ts'
import type { ClientOptions, CommandResponse, ScenicClient } from './types.ts'
import { buildClientMethods } from './commands/define.ts'
import type { CommandRuntime, ExecuteGuard } from './commands/runtime.ts'
import { allCommands } from './commands/registry.ts'

export class ScenicError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'ScenicError'
  }
}

function unwrap(response: CommandResponse): unknown {
  if (!response.success) {
    throw new ScenicError(response.error ?? 'Command failed')
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

export function createClient(options: ClientOptions = {}): ScenicClient {
  const projectPath = options.projectPath ?? findUnityProject()
  const connection = new PipeConnection({
    projectPath,
    connectTimeout: options.connectTimeout,
    commandTimeout: options.commandTimeout,
  })
  const callerExecuteEnabled = options.enableExecute ?? false

  async function sendCommand(
    command: string,
    params: Record<string, unknown>,
  ): Promise<CommandResponse> {
    await connection.connect(pipePath(projectPath))
    return connection.send({ id: randomUUID(), command, params })
  }

  function ensureExecuteEnabled(): void {
    if (!callerExecuteEnabled) {
      throw new ScenicError('Execute is disabled by client or plugin configuration.')
    }

    if (!readExecuteEnabled(projectPath)) {
      throw new ScenicError('Execute is disabled by client or plugin configuration.')
    }
  }

  const runtime = createRuntime(sendCommand, ensureExecuteEnabled)

  return {
    projectPath,
    ...buildClientMethods(runtime, allCommands),
    close(): void {
      connection.disconnect()
    },
  }
}
