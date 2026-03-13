import { readFileSync } from 'node:fs'
import type { Command } from 'commander'
import type {
  ComponentListItem,
  ComponentsAddInput,
  ComponentsAddResult,
  ComponentsGetQuery,
  ComponentsGetResult,
  ComponentsRemoveInput,
  ComponentsRemoveResult,
  ComponentsUpdateInput,
  ComponentsUpdateResult,
  ComponentsListQuery,
  ComponentsListResult,
} from '@scenicai/sdk/commands/component'
import { runWithOutput } from './output.ts'
import { parseIntWithMinimum, parseOptionalInt } from './parse.ts'
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

interface ComponentsAddOptions {
  path?: string
  instanceId?: string
  type?: string
  values?: string
  valuesFile?: string
  strict?: boolean
}

interface ComponentsAddDeps {
  add: (input: ComponentsAddInput) => Promise<ComponentsAddResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface ComponentsGetOptions {
  path?: string
  instanceId?: string
  componentInstanceId?: string
  index?: string
  type?: string
}

interface ComponentsGetDeps {
  get: (query: ComponentsGetQuery) => Promise<ComponentsGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface ComponentsRemoveOptions {
  path?: string
  instanceId?: string
  componentInstanceId?: string
  index?: string
  type?: string
}

interface ComponentsRemoveDeps {
  remove: (input: ComponentsRemoveInput) => Promise<ComponentsRemoveResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface ComponentsUpdateOptions {
  path?: string
  instanceId?: string
  componentInstanceId?: string
  index?: string
  type?: string
  values?: string
  valuesFile?: string
  strict?: boolean
}

interface ComponentsUpdateDeps {
  update: (input: ComponentsUpdateInput) => Promise<ComponentsUpdateResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
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

function parseInitialValues(opts: ComponentsAddOptions): Record<string, unknown> | undefined {
  if (opts.values != null && opts.valuesFile != null) {
    throw new Error('Use either --values or --values-file, not both.')
  }

  if (opts.values == null && opts.valuesFile == null) {
    return undefined
  }

  let text: string
  if (opts.valuesFile != null) {
    text = readFileSync(opts.valuesFile, 'utf-8')
  } else if (opts.values != null) {
    text = opts.values
  } else {
    throw new Error('Provide --values or --values-file.')
  }

  let parsed: unknown
  try {
    parsed = JSON.parse(text)
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error)
    throw new Error(`Invalid JSON for initial values: ${message}`)
  }

  if (parsed == null || Array.isArray(parsed) || typeof parsed !== 'object') {
    throw new Error('Initial values must be a JSON object.')
  }

  return parsed as Record<string, unknown>
}

export async function handleComponentsList(
  opts: ComponentsListOptions,
  jsonOutput: boolean,
  deps: ComponentsListDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
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
      return deps.list(query)
    },
    (result, output) => {
      output.log(`Components: ${result.components.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const component of result.components) {
        output.log(`#${component.index} ${component.type} [id=${component.instanceId}, enabled=${formatEnabled(component)}]`)
      }
    },
  )
}

export async function handleComponentsAdd(
  opts: ComponentsAddOptions,
  jsonOutput: boolean,
  deps: ComponentsAddDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      const instanceId = parseInstanceId(opts.instanceId)
      const path = opts.path
      if ((path == null || path.length === 0) && instanceId == null) {
        throw new Error('Provide target via --path or --instance-id.')
      }
      if (path != null && instanceId != null) {
        throw new Error('Use either --path or --instance-id, not both.')
      }
      if (opts.type == null || opts.type.trim().length === 0) {
        throw new Error('Provide --type.')
      }
      const input: ComponentsAddInput = {
        path,
        instanceId,
        type: opts.type,
        initialValues: parseInitialValues(opts),
        strict: opts.strict === true,
      }
      return deps.add(input)
    },
    (result, output) => {
      output.log(`Added: ${result.type}`)
      output.log(`Id:    ${result.instanceId}`)
      output.log(`Applied fields: ${result.appliedFields.length}`)
      output.log(`Ignored fields: ${result.ignoredFields.length}`)
    },
  )
}

export async function handleComponentsGet(
  opts: ComponentsGetOptions,
  jsonOutput: boolean,
  deps: ComponentsGetDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      const instanceId = parseInstanceId(opts.instanceId)
      const path = opts.path
      if ((path == null || path.length === 0) && instanceId == null) {
        throw new Error('Provide target via --path or --instance-id.')
      }
      if (path != null && instanceId != null) {
        throw new Error('Use either --path or --instance-id, not both.')
      }
      const componentInstanceId = parseOptionalInt(opts.componentInstanceId, '--component-instance-id')
      const index = parseOptionalInt(opts.index, '--index')
      const type = opts.type == null || opts.type.trim().length === 0 ? undefined : opts.type.trim()
      const selectorCount = Number(componentInstanceId != null) + Number(index != null) + Number(type != null)
      if (selectorCount !== 1) {
        throw new Error('Provide exactly one selector: --component-instance-id, --index, or --type.')
      }
      if (index != null && index < 0) {
        throw new Error('--index must be a non-negative integer.')
      }
      const query: ComponentsGetQuery = {
        path,
        instanceId,
        componentInstanceId,
        index,
        type,
      }
      return deps.get(query)
    },
    (result, output) => {
      output.log(`Component: ${result.component.type}`)
      output.log(`Id:        ${result.component.instanceId}`)
      output.log(`Index:     ${result.component.index}`)
      output.log(`Enabled:   ${result.component.enabled == null ? 'n/a' : result.component.enabled ? 'yes' : 'no'}`)
      output.log('Serialized:')
      output.log(JSON.stringify(result.component.serialized, null, 2))
    },
  )
}

export async function handleComponentsRemove(
  opts: ComponentsRemoveOptions,
  jsonOutput: boolean,
  deps: ComponentsRemoveDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      const instanceId = parseInstanceId(opts.instanceId)
      const path = opts.path
      if ((path == null || path.length === 0) && instanceId == null) {
        throw new Error('Provide target via --path or --instance-id.')
      }
      if (path != null && instanceId != null) {
        throw new Error('Use either --path or --instance-id, not both.')
      }
      const componentInstanceId = parseOptionalInt(opts.componentInstanceId, '--component-instance-id')
      const index = parseOptionalInt(opts.index, '--index')
      const type = opts.type == null || opts.type.trim().length === 0 ? undefined : opts.type.trim()
      const selectorCount = Number(componentInstanceId != null) + Number(index != null) + Number(type != null)
      if (selectorCount !== 1) {
        throw new Error('Provide exactly one selector: --component-instance-id, --index, or --type.')
      }
      if (index != null && index < 0) {
        throw new Error('--index must be a non-negative integer.')
      }
      const input: ComponentsRemoveInput = {
        path,
        instanceId,
        componentInstanceId,
        index,
        type,
      }
      return deps.remove(input)
    },
    (result, output) => {
      output.log(`Removed: ${result.type}`)
      output.log(`Id:      ${result.instanceId}`)
      output.log(`Index:   ${result.index}`)
    },
  )
}

export async function handleComponentsUpdate(
  opts: ComponentsUpdateOptions,
  jsonOutput: boolean,
  deps: ComponentsUpdateDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      const instanceId = parseInstanceId(opts.instanceId)
      const path = opts.path
      if ((path == null || path.length === 0) && instanceId == null) {
        throw new Error('Provide target via --path or --instance-id.')
      }
      if (path != null && instanceId != null) {
        throw new Error('Use either --path or --instance-id, not both.')
      }
      const componentInstanceId = parseOptionalInt(opts.componentInstanceId, '--component-instance-id')
      const index = parseOptionalInt(opts.index, '--index')
      const type = opts.type == null || opts.type.trim().length === 0 ? undefined : opts.type.trim()
      const selectorCount = Number(componentInstanceId != null) + Number(index != null) + Number(type != null)
      if (selectorCount !== 1) {
        throw new Error('Provide exactly one selector: --component-instance-id, --index, or --type.')
      }
      if (index != null && index < 0) {
        throw new Error('--index must be a non-negative integer.')
      }
      const values = parseInitialValues({
        values: opts.values,
        valuesFile: opts.valuesFile,
      })
      if (values == null) {
        throw new Error('Provide --values or --values-file.')
      }
      const input: ComponentsUpdateInput = {
        path,
        instanceId,
        componentInstanceId,
        index,
        type,
        values,
        strict: opts.strict === true,
      }
      return deps.update(input)
    },
    (result, output) => {
      output.log(`Updated: ${result.type}`)
      output.log(`Id:      ${result.instanceId}`)
      output.log(`Index:   ${result.index}`)
      output.log(`Applied fields: ${result.appliedFields.length}`)
      output.log(`Ignored fields: ${result.ignoredFields.length}`)
    },
  )
}

export function registerComponents(program: Command): void {
  const components = program
    .command('components')
    .description('Manage components on a Unity GameObject')

  components
    .command('add')
    .description('Add a component to a target GameObject')
    .option('--path <path>', 'Target GameObject path, e.g. /Player')
    .option('--instance-id <id>', 'Target GameObject instance ID (session-local)')
    .requiredOption('--type <componentType>', 'Component type, e.g. UnityEngine.Rigidbody')
    .option('--values <json>', 'Inline JSON object for initial values')
    .option('--values-file <path>', 'Path to JSON file containing initial values')
    .option('--strict', 'Fail if any initial value field is unknown')
    .action(async (opts: ComponentsAddOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleComponentsAdd(opts, ctx.jsonOutput, {
            add: (input) => client.componentsAdd(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  components
    .command('get')
    .description('Get and serialize a component on a target GameObject')
    .option('--path <path>', 'Target GameObject path, e.g. /Player')
    .option('--instance-id <id>', 'Target GameObject instance ID (session-local)')
    .option('--component-instance-id <id>', 'Component instance ID')
    .option('--index <number>', 'Component index on the GameObject')
    .option('--type <text>', 'Component type selector (substring, must match exactly one)')
    .action(async (opts: ComponentsGetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleComponentsGet(opts, ctx.jsonOutput, {
            get: (query) => client.componentsGet(query),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  components
    .command('remove')
    .description('Remove a component from a target GameObject')
    .option('--path <path>', 'Target GameObject path, e.g. /Player')
    .option('--instance-id <id>', 'Target GameObject instance ID (session-local)')
    .option('--component-instance-id <id>', 'Component instance ID')
    .option('--index <number>', 'Component index on the GameObject')
    .option('--type <text>', 'Component type selector (substring, must match exactly one)')
    .action(async (opts: ComponentsRemoveOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleComponentsRemove(opts, ctx.jsonOutput, {
            remove: (input) => client.componentsRemove(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  components
    .command('update')
    .description('Update fields on a component')
    .option('--path <path>', 'Target GameObject path, e.g. /Player')
    .option('--instance-id <id>', 'Target GameObject instance ID (session-local)')
    .option('--component-instance-id <id>', 'Component instance ID')
    .option('--index <number>', 'Component index on the GameObject')
    .option('--type <text>', 'Component type selector (substring, must match exactly one)')
    .option('--values <json>', 'Inline JSON object of component values to update')
    .option('--values-file <path>', 'Path to JSON file of component values to update')
    .option('--strict', 'Fail if any value field is unknown')
    .action(async (opts: ComponentsUpdateOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleComponentsUpdate(opts, ctx.jsonOutput, {
            update: (input) => client.componentsUpdate(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

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
