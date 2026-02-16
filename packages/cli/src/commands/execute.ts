import type { Command } from 'commander'
import { createClient } from '@unibridge/sdk'
import { resolveCommandProject } from '../preflight.ts'

interface ExecuteCommandOptions {
  project?: string
  json?: boolean
  timeout?: string
}

interface ExecuteDeps {
  execute: (code: string, options: { timeout: number }) => Promise<unknown>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function parseTimeout(input: string | undefined): number {
  const parsed = Number(input ?? '30000')
  if (!Number.isFinite(parsed) || parsed <= 0) {
    throw new Error('Timeout must be a positive number of milliseconds.')
  }

  return parsed
}

export async function handleExecute(
  code: string,
  opts: ExecuteCommandOptions,
  deps: ExecuteDeps,
): Promise<void> {
  try {
    const result = await deps.execute(code, { timeout: parseTimeout(opts.timeout) })
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
    .option('--timeout <ms>', 'Command timeout in milliseconds', '30000')
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
          execute: (requestCode, executeOptions) => client?.execute(requestCode, executeOptions) ?? Promise.resolve(null),
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
