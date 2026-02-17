import type { Command } from 'commander'
import { createClient } from '@unibridge/sdk'
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
    const envExecute = process.env.UNIBRIDGE_ENABLE_EXECUTE !== '0'
    const executeEnabled = globalOpts.execute !== false && envExecute

    const projectPath = resolveCommandProject(
      {
        project: globalOpts.project,
        execute: executeEnabled,
      },
      requirements,
    )

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
