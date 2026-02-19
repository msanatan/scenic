import type { Command } from 'commander'
import type { ComponentListItem, ComponentsListQuery, ComponentsListResult } from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface ComponentsListOptions {
  path?: string
  instanceId?: string
  type?: string
  limit?: string
  offset?: string
}

interface ComponentsListDeps {
  list: (query: ComponentsListQuery) => Promise<ComponentsListResult>
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

function parseInstanceId(value: string | undefined): number | undefined {
  if (value == null) {
    return undefined
  }
  const parsed = Number.parseInt(value, 10)
  if (!Number.isInteger(parsed)) {
    throw new Error('--instance-id must be an integer.')
  }
  return parsed
}

function formatEnabled(item: ComponentListItem): string {
  if (item.enabled == null) {
    return 'n/a'
  }
  return item.enabled ? 'yes' : 'no'
}

export async function handleComponentsList(
  opts: ComponentsListOptions,
  jsonOutput: boolean,
  deps: ComponentsListDeps,
): Promise<void> {
  const instanceId = parseInstanceId(opts.instanceId)
  const path = opts.path

  if ((path == null || path.length === 0) && instanceId == null) {
    throw new Error('Provide target via --path or --instance-id.')
  }
  if (path != null && instanceId != null) {
    throw new Error('Use either --path or --instance-id, not both.')
  }

  const query: ComponentsListQuery = {
    path,
    instanceId,
    type: opts.type,
    limit: parseIntWithMinimum(opts.limit, '--limit', 50, 1),
    offset: parseIntWithMinimum(opts.offset, '--offset', 0, 0),
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.list(query),
    (result, output) => {
      output.log(`Components: ${result.components.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const component of result.components) {
        output.log(`#${component.index} ${component.type} [id=${component.instanceId}, enabled=${formatEnabled(component)}]`)
      }
    },
  )
}

export function registerComponents(program: Command): void {
  const components = program
    .command('components')
    .description('List components on a Unity GameObject')

  components
    .command('list')
    .description('List components on a target GameObject with pagination')
    .option('--path <path>', 'Target GameObject path, e.g. /Player')
    .option('--instance-id <id>', 'Target GameObject instance ID (session-local)')
    .option('--type <text>', 'Filter by component type name (substring)')
    .option('--limit <number>', 'Number of components to return (default: 50)')
    .option('--offset <number>', 'Offset into component list (default: 0)')
    .action(async (opts: ComponentsListOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleComponentsList(opts, ctx.jsonOutput, {
            list: (query) => client.componentsList(query),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
