import type { Command } from 'commander'
import { createClient } from '@unibridge/sdk'
import type { StatusResult } from '@unibridge/sdk'
import { resolveCommandProject } from '../preflight.ts'
import { getGlobalOptions } from '../options.ts'

interface StatusDeps {
  status: () => Promise<StatusResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleStatus(
  jsonOutput: boolean,
  deps: StatusDeps,
): Promise<void> {
  try {
    const result = await deps.status()
    if (jsonOutput) {
      deps.console.log(JSON.stringify({ success: true, result }))
    } else {
      deps.console.log(`Project: ${result.projectPath}`)
      deps.console.log(`Unity:   ${result.unityVersion}`)
      deps.console.log(`Plugin:  ${result.pluginVersion}`)
      deps.console.log(`Scene:   ${result.activeScene}`)
      deps.console.log(`Mode:    ${result.playMode}`)
    }
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error)
    if (jsonOutput) {
      deps.console.log(JSON.stringify({ success: false, error: message }))
    } else {
      deps.console.error(`Error: ${message}`)
    }
    deps.exit?.(1)
  }
}

export function registerStatus(program: Command): void {
  program
    .command('status')
    .description('Show the current state of the Unity Editor')
    .action(async (_opts: Record<string, never>, command: Command) => {
      let client: ReturnType<typeof createClient> | undefined
      try {
        const globalOpts = getGlobalOptions(command)
        const projectPath = resolveCommandProject(
          { project: globalOpts.project },
          { requirePlugin: true },
        )

        client = createClient({ projectPath })

        await handleStatus(globalOpts.json === true, {
          status: () => client!.status(),
          console,
          exit: (exitCode) => {
            process.exitCode = exitCode
          },
        })
      } finally {
        client?.close()
      }
    })
}
