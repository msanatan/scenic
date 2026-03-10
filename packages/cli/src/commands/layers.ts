import type { Command } from 'commander'
import type {
  LayersAddInput,
  LayersAddResult,
  LayersGetQuery,
  LayersGetResult,
  LayersRemoveInput,
  LayersRemoveResult,
} from '@scenicai/sdk/commands/layer'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface LayersGetOptions {
  limit?: string
  offset?: string
}

interface LayersGetDeps {
  get: (query?: LayersGetQuery) => Promise<LayersGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface LayersAddDeps {
  add: (input: LayersAddInput) => Promise<LayersAddResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface LayersRemoveDeps {
  remove: (input: LayersRemoveInput) => Promise<LayersRemoveResult>
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

export async function handleLayersGet(
  opts: LayersGetOptions,
  jsonOutput: boolean,
  deps: LayersGetDeps,
): Promise<void> {
  const query: LayersGetQuery = {
    limit: parseIntWithMinimum(opts.limit, '--limit', 50, 1),
    offset: parseIntWithMinimum(opts.offset, '--offset', 0, 0),
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.get(query),
    (result, output) => {
      output.log(`Layers: ${result.layers.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const layer of result.layers) {
        const name = layer.name.length > 0 ? layer.name : '<empty>'
        output.log(`#${layer.index} ${name} [occupied=${layer.isOccupied ? 'yes' : 'no'}, built-in=${layer.isBuiltIn ? 'yes' : 'no'}]`)
      }
    },
  )
}

export async function handleLayersAdd(
  name: string,
  jsonOutput: boolean,
  deps: LayersAddDeps,
): Promise<void> {
  const trimmed = name.trim()
  if (trimmed.length === 0) {
    throw new Error('Layer name is required.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.add({ name: trimmed }),
    (result, output) => {
      const nameValue = result.layer.name.length > 0 ? result.layer.name : '<empty>'
      output.log(`Layer: #${result.layer.index} ${nameValue}`)
      output.log(`Added: ${result.added ? 'yes' : 'no'}`)
      output.log(`Total: ${result.total}`)
    },
  )
}

export async function handleLayersRemove(
  name: string,
  jsonOutput: boolean,
  deps: LayersRemoveDeps,
): Promise<void> {
  const trimmed = name.trim()
  if (trimmed.length === 0) {
    throw new Error('Layer name is required.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.remove({ name: trimmed }),
    (result, output) => {
      const nameValue = result.layer.name.length > 0 ? result.layer.name : '<empty>'
      output.log(`Layer:   #${result.layer.index} ${nameValue}`)
      output.log(`Removed: ${result.removed ? 'yes' : 'no'}`)
      output.log(`Total:   ${result.total}`)
    },
  )
}

export function registerLayers(program: Command): void {
  const layers = program
    .command('layers')
    .description('Inspect and mutate Unity project layers')

  layers
    .command('get')
    .description('List layer slots with pagination')
    .option('--limit <number>', 'Number of layer slots to return (default: 50)')
    .option('--offset <number>', 'Offset into layer slots (default: 0)')
    .action(async (opts: LayersGetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleLayersGet(opts, ctx.jsonOutput, {
            get: (query) => client.layersGet(query),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  layers
    .command('add <name>')
    .description('Add a layer in the first available user slot (8-31), idempotent by name')
    .action(async (name: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleLayersAdd(name, ctx.jsonOutput, {
            add: (input) => client.layersAdd(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  layers
    .command('remove <name>')
    .description('Remove a user layer by name (idempotent; built-in layers are protected)')
    .action(async (name: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleLayersRemove(name, ctx.jsonOutput, {
            remove: (input) => client.layersRemove(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
