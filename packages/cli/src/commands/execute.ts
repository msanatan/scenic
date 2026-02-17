import type { Command } from 'commander'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface ExecuteDeps {
  execute: (code: string) => Promise<unknown>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleExecute(
  code: string,
  jsonOutput: boolean,
  deps: ExecuteDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.execute(code),
    (result, output) => {
      output.log(String(result))
    },
  )
}

export function registerExecute(program: Command): void {
  program
    .command('execute <code>')
    .description('Execute C# code in the Unity Editor')
    .action(async (code: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        {
          requirePlugin: true,
          requiresExecute: true,
        },
        async (client, ctx) => {
          await handleExecute(code, ctx.jsonOutput, {
            execute: (requestCode) => client.execute(requestCode),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
