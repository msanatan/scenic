import type { Command } from 'commander'
import { createClient } from '@unibridge/sdk'
import { resolveCommandProject } from '../preflight.ts'

interface ExecuteCommandOptions {
  project?: string
  json?: boolean
}

interface ExecuteDeps {
  execute: (code: string) => Promise<unknown>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleExecute(
  code: string,
  opts: ExecuteCommandOptions,
  deps: ExecuteDeps,
): Promise<void> {
  try {
    const result = await deps.execute(code)
    if (opts.json) {
      deps.console.log(JSON.stringify({ success: true, result }))
    } else {
      deps.console.log(String(result))
    }
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error)
    if (opts.json) {
      deps.console.log(JSON.stringify({ success: false, error: message }))
    } else {
      deps.console.error(`Error: ${message}`)
    }
    deps.exit?.(1)
  }
}

export function registerExecute(program: Command): void {
  program
    .command('execute <code>')
    .description('Execute C# code in the Unity Editor')
    .option('-p, --project <path>', 'Path to Unity project')
    .option('--json', 'Output result as JSON')
    .action(async (code: string, opts: ExecuteCommandOptions, command: Command) => {
      let client: ReturnType<typeof createClient> | undefined
      try {
        const globalOpts = command.optsWithGlobals() as { execute?: boolean }
        const envExecute = process.env.UNIBRIDGE_ENABLE_EXECUTE !== '0'
        const callerEnableExecute = globalOpts.execute !== false && envExecute

        const projectPath = resolveCommandProject(
          {
            project: opts.project,
            execute: callerEnableExecute,
          },
          {
            requirePlugin: true,
            requiresExecute: true,
          },
        )

        client = createClient({
          projectPath,
          enableExecute: callerEnableExecute,
        })

        await handleExecute(code, opts, {
          execute: (requestCode) => client!.execute(requestCode),
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
