import type { Command } from 'commander'
import type { LogsQuery, LogsResult, LogsSeverity } from '@scenicai/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface LogsCommandOptions {
  severity?: LogsSeverity
  limit?: string
  offset?: string
}

interface LogsDeps {
  logs: (query?: LogsQuery) => Promise<LogsResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function parseIntWithMinimum(
  value: string | undefined,
  label: string,
  defaultValue: number,
  minimum: number,
): number {
  if (value == null) {
    return defaultValue
  }

  const parsed = Number.parseInt(value, 10)
  if (!Number.isFinite(parsed) || parsed < minimum) {
    if (minimum <= 0) {
      throw new Error(`${label} must be a non-negative integer.`)
    }
    throw new Error(`${label} must be an integer >= ${minimum}.`)
  }

  return parsed
}

function parseSeverity(value: string | undefined): LogsSeverity | undefined {
  if (value == null) {
    return undefined
  }
  if (value === 'info' || value === 'warn' || value === 'error') {
    return value
  }
  throw new Error('--severity must be one of: info, warn, error.')
}

export async function handleLogs(
  opts: LogsCommandOptions,
  jsonOutput: boolean,
  deps: LogsDeps,
): Promise<void> {
  const limit = parseIntWithMinimum(opts.limit, '--limit', 50, 1)
  const offset = parseIntWithMinimum(opts.offset, '--offset', 0, 0)
  const query: LogsQuery = {
    severity: parseSeverity(opts.severity),
    limit,
    offset,
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.logs(query),
    (result, output) => {
      output.log(`Logs: ${result.logs.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const entry of result.logs) {
        output.log(`[${entry.timestamp}] ${entry.severity.toUpperCase()} ${entry.message}`)
        if (entry.stackTrace && entry.stackTrace.trim().length > 0) {
          output.log(entry.stackTrace)
        }
      }
    },
  )
}

export function registerLogs(program: Command): void {
  program
    .command('logs')
    .description('List Unity Editor logs with optional severity filter and pagination')
    .option('--severity <severity>', 'Filter by severity (info|warn|error)')
    .option('--limit <number>', 'Number of log entries to return (default: 50)')
    .option('--offset <number>', 'Offset into the filtered log entries (default: 0)')
    .action(async (opts: LogsCommandOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleLogs(opts, ctx.jsonOutput, {
            logs: (query) => client.logs(query),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
