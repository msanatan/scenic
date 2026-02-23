import type { Command } from 'commander'
import type { StatusResult } from '@scenicai/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface StatusDeps {
  status: () => Promise<StatusResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleStatus(
  jsonOutput: boolean,
  deps: StatusDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.status(),
    (result, output) => {
      output.log(`Project: ${result.projectPath}`)
      output.log(`Unity:   ${result.unityVersion}`)
      output.log(`Plugin:  ${result.pluginVersion}`)
      output.log(`Scene:   ${result.activeScene}`)
      output.log(`Mode:    ${result.playMode}`)
    },
  )
}

export function registerStatus(program: Command): void {
  program
    .command('status')
    .description('Show the current state of the Unity Editor')
    .action(async (_opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleStatus(ctx.jsonOutput, {
            status: () => client.status(),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
