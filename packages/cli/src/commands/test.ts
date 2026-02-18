import type { Command } from 'commander'
import type {
  TestListQuery,
  TestListResult,
  TestMode,
  TestRunQuery,
  TestRunResult,
} from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface TestCommandOptions {
  mode?: string
  filter?: string
  limit?: string
  offset?: string
}

interface TestListDeps {
  list: (query?: TestListQuery) => Promise<TestListResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface TestRunDeps {
  run: (query?: TestRunQuery) => Promise<TestRunResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function parseMode(value: string | undefined): TestMode | undefined {
  if (value == null) {
    return undefined
  }
  if (value === 'edit' || value === 'play') {
    return value
  }
  throw new Error('--mode must be one of: edit, play.')
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

function parseQuery(opts: TestCommandOptions): TestListQuery {
  return {
    mode: parseMode(opts.mode),
    filter: opts.filter,
    limit: parseIntWithMinimum(opts.limit, '--limit', 50, 1),
    offset: parseIntWithMinimum(opts.offset, '--offset', 0, 0),
  }
}

export async function handleTestList(
  opts: TestCommandOptions,
  jsonOutput: boolean,
  deps: TestListDeps,
): Promise<void> {
  const query = parseQuery(opts)

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.list(query),
    (result, output) => {
      output.log(`Tests: ${result.tests.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const test of result.tests) {
        output.log(`${test.mode.toUpperCase()} ${test.fullName}`)
      }
    },
  )
}

export async function handleTestRun(
  opts: TestCommandOptions,
  jsonOutput: boolean,
  deps: TestRunDeps,
): Promise<void> {
  const query: TestRunQuery = parseQuery(opts)

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.run(query),
    (result, output) => {
      output.log(`Ran ${result.total} tests`)
      output.log(`Passed: ${result.passed}  Failed: ${result.failed}  Skipped: ${result.skipped}  Inconclusive: ${result.inconclusive}`)
      output.log(`Duration: ${result.durationMs}ms`)
      for (const test of result.tests) {
        output.log(`${test.status.toUpperCase()} ${test.fullName} (${test.durationMs}ms)`)
        if (test.message) {
          output.log(test.message)
        }
      }
    },
  )
}

export function registerTest(program: Command): void {
  const test = program
    .command('test')
    .description('List and run Unity Test Framework tests')

  const applyCommonOptions = (command: Command): Command =>
    command
      .option('--mode <mode>', 'Test mode (edit|play)')
      .option('--filter <text>', 'Filter by test full name substring')
      .option('--limit <number>', 'Number of items to return (default: 50)')
      .option('--offset <number>', 'Offset into filtered items (default: 0)')

  applyCommonOptions(
    test
      .command('list')
      .description('List available tests with pagination')
      .action(async (opts: TestCommandOptions, command: Command) => {
        await withUnityClient(
          command,
          { requirePlugin: true },
          async (client, ctx) => {
            await handleTestList(opts, ctx.jsonOutput, {
              list: (query) => client.testList(query),
              console,
              exit: (exitCode) => {
                process.exitCode = exitCode
              },
            })
          },
        )
      }),
  )

  applyCommonOptions(
    test
      .command('run')
      .description('Run tests and return paginated results')
      .action(async (opts: TestCommandOptions, command: Command) => {
        await withUnityClient(
          command,
          { requirePlugin: true },
          async (client, ctx) => {
            await handleTestRun(opts, ctx.jsonOutput, {
              run: (query) => client.testRun(query),
              console,
              exit: (exitCode) => {
                process.exitCode = exitCode
              },
            })
          },
        )
      }),
  )
}
