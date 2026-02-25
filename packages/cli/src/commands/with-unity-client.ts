import type { Command } from 'commander'
import { createClient, readExecuteEnabled } from '@scenicai/sdk'
import { getGlobalOptions } from '../options.ts'
import { resolveCommandProject } from '../preflight.ts'

interface ClientCommandRequirements {
  requirePlugin: boolean
  requiresExecute?: boolean
}

interface GlobalCommandContext {
  jsonOutput: boolean
}

export async function withUnityClient(
  command: Command,
  requirements: ClientCommandRequirements,
  run: (client: ReturnType<typeof createClient>, ctx: GlobalCommandContext) => Promise<void>,
): Promise<void> {
  let client: ReturnType<typeof createClient> | undefined

  try {
    const globalOpts = getGlobalOptions(command)

    const projectPath = resolveCommandProject(
      {
        project: globalOpts.project,
        execute: globalOpts.execute,
      },
      requirements,
    )
    const projectExecuteEnabled = readExecuteEnabled(projectPath)
    const executeEnabled = globalOpts.execute !== false && projectExecuteEnabled
    if (requirements.requiresExecute && !executeEnabled) {
      throw new Error('Execute is disabled for this project. Run `scenic init --enable-execute` to enable it.')
    }

    client = createClient({
      projectPath,
      enableExecute: executeEnabled,
      connectTimeout: 10_000,
    })

    await run(client, {
      jsonOutput: globalOpts.json === true,
    })
  } finally {
    client?.close()
  }
}
