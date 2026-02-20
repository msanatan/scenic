import type { Command } from 'commander'
import type { LayersGetQuery, LayersGetResult } from '@unibridge/sdk'
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

export function registerLayers(program: Command): void {
  const layers = program
    .command('layers')
    .description('Inspect Unity project layers')

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
}
