import { readFileSync } from 'node:fs'
import type { Command } from 'commander'
import type {
  AssetFindInput,
  AssetFindResult,
  AssetGetInput,
  AssetGetResult,
  AssetMoveInput,
  AssetMoveResult,
  AssetCopyInput,
  AssetCopyResult,
  AssetDeleteInput,
  AssetDeleteResult,
  AssetImportInput,
  AssetImportResult,
  AssetImportSettingsGetInput,
  AssetImportSettingsGetResult,
  AssetImportSettingsSetInput,
  AssetImportSettingsSetResult,
  AssetLabelsGetInput,
  AssetLabelsGetResult,
  AssetLabelsAddInput,
  AssetLabelsAddResult,
  AssetLabelsRemoveInput,
  AssetLabelsRemoveResult,
} from '@scenicai/sdk/commands/asset'
import { runWithOutput } from './output.ts'
import { normalizeLabels, parseOptionalInt } from './parse.ts'
import { withUnityClient } from './with-unity-client.ts'

interface AssetFindOptions {
  type?: string
  label?: string[]
  limit?: string
  offset?: string
}

interface AssetFindDeps {
  assetFind: (input: AssetFindInput) => Promise<AssetFindResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetFind(
  query: string | undefined,
  opts: AssetFindOptions,
  jsonOutput: boolean,
  deps: AssetFindDeps,
): Promise<void> {
  const limit = parseOptionalInt(opts.limit, 'limit')
  const offset = parseOptionalInt(opts.offset, 'offset')

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetFind({
      query: query?.trim(),
      type: opts.type?.trim(),
      labels: opts.label ? normalizeLabels(opts.label) : undefined,
      limit,
      offset,
    }),
    (result, output) => {
      output.log(`Assets: ${result.assets.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const asset of result.assets) {
        output.log(`- ${asset.assetPath} [${asset.type}]`)
      }
    },
  )
}


interface AssetGetDeps {
  assetGet: (input: AssetGetInput) => Promise<AssetGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetGet(
  assetPath: string,
  jsonOutput: boolean,
  deps: AssetGetDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetGet({ assetPath: assetPath.trim() }),
    (result, output) => {
      output.log(`AssetPath:     ${result.assetPath}`)
      output.log(`GUID:          ${result.guid}`)
      output.log(`Type:          ${result.type}`)
      output.log(`Name:          ${result.name}`)
      output.log(`Labels:        ${result.labels.join(', ')}`)
      output.log(`Dependencies:  ${result.dependencies.length}`)
    },
  )
}


interface AssetMoveDeps {
  assetMove: (input: AssetMoveInput) => Promise<AssetMoveResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetMove(
  assetPath: string,
  newPath: string,
  jsonOutput: boolean,
  deps: AssetMoveDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetMove({ assetPath: assetPath.trim(), newPath: newPath.trim() }),
    (result, output) => {
      output.log(`Moved: ${result.oldPath} -> ${result.newPath}`)
      output.log(`GUID:  ${result.guid}`)
    },
  )
}


interface AssetCopyDeps {
  assetCopy: (input: AssetCopyInput) => Promise<AssetCopyResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetCopy(
  assetPath: string,
  newPath: string,
  jsonOutput: boolean,
  deps: AssetCopyDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetCopy({ assetPath: assetPath.trim(), newPath: newPath.trim() }),
    (result, output) => {
      output.log(`Copied: ${result.sourcePath} -> ${result.newPath}`)
      output.log(`GUID:   ${result.guid}`)
    },
  )
}


interface AssetDeleteDeps {
  assetDelete: (input: AssetDeleteInput) => Promise<AssetDeleteResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetDelete(
  assetPath: string,
  jsonOutput: boolean,
  deps: AssetDeleteDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetDelete({ assetPath: assetPath.trim() }),
    (result, output) => {
      output.log(`AssetPath: ${result.assetPath}`)
      output.log(`Deleted:   ${result.deleted ? 'yes' : 'no'}`)
    },
  )
}


interface AssetImportOptions {
  options?: string
}

interface AssetImportDeps {
  assetImport: (input: AssetImportInput) => Promise<AssetImportResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetImport(
  assetPath: string,
  opts: AssetImportOptions,
  jsonOutput: boolean,
  deps: AssetImportDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetImport({
      assetPath: assetPath.trim(),
      options: opts.options,
    }),
    (result, output) => {
      output.log(`AssetPath:    ${result.assetPath}`)
      output.log(`ImporterType: ${result.importerType}`)
    },
  )
}


interface AssetImportSettingsGetOptions {
  property?: string[]
}

interface AssetImportSettingsGetDeps {
  assetImportSettingsGet: (input: AssetImportSettingsGetInput) => Promise<AssetImportSettingsGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetImportSettingsGet(
  assetPath: string,
  opts: AssetImportSettingsGetOptions,
  jsonOutput: boolean,
  deps: AssetImportSettingsGetDeps,
): Promise<void> {
  const properties = opts.property?.map((p) => p.trim()).filter((p) => p.length > 0)

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetImportSettingsGet({
      assetPath: assetPath.trim(),
      properties: properties != null && properties.length > 0 ? properties : undefined,
    }),
    (result, output) => {
      output.log(`AssetPath:    ${result.assetPath}`)
      output.log(`ImporterType: ${result.importerType}`)
      for (const [key, value] of Object.entries(result.properties)) {
        output.log(`${key}=${JSON.stringify(value)}`)
      }
    },
  )
}


interface AssetImportSettingsSetOptions {
  values?: string
  valuesFile?: string
}

interface AssetImportSettingsSetDeps {
  assetImportSettingsSet: (input: AssetImportSettingsSetInput) => Promise<AssetImportSettingsSetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function parseJsonObject(value: string, label: string): Record<string, unknown> {
  let parsed: unknown
  try {
    parsed = JSON.parse(value)
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error)
    throw new Error(`Invalid JSON for ${label}: ${message}`)
  }

  if (parsed == null || Array.isArray(parsed) || typeof parsed !== 'object') {
    throw new Error(`${label} must be a JSON object.`)
  }

  return parsed as Record<string, unknown>
}

function parseImportSettingsValues(
  values: string | undefined,
  valuesFile: string | undefined,
): Record<string, unknown> {
  if (values != null && valuesFile != null) {
    throw new Error('Use either --values or --values-file, not both.')
  }

  if (values == null && valuesFile == null) {
    throw new Error('Provide --values or --values-file.')
  }

  const text = valuesFile != null ? readFileSync(valuesFile, 'utf-8') : values ?? ''
  return parseJsonObject(text, 'values')
}

export async function handleAssetImportSettingsSet(
  assetPath: string,
  opts: AssetImportSettingsSetOptions,
  jsonOutput: boolean,
  deps: AssetImportSettingsSetDeps,
): Promise<void> {
  const properties = parseImportSettingsValues(opts.values, opts.valuesFile)

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetImportSettingsSet({
      assetPath: assetPath.trim(),
      properties,
    }),
    (result, output) => {
      output.log(`AssetPath:    ${result.assetPath}`)
      output.log(`ImporterType: ${result.importerType}`)
      output.log(`Applied:      ${result.appliedProperties.length}`)
    },
  )
}


interface AssetLabelsGetDeps {
  assetLabelsGet: (input: AssetLabelsGetInput) => Promise<AssetLabelsGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetLabelsGet(
  assetPath: string,
  jsonOutput: boolean,
  deps: AssetLabelsGetDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetLabelsGet({ assetPath: assetPath.trim() }),
    (result, output) => {
      output.log(`AssetPath: ${result.assetPath}`)
      for (const label of result.labels) {
        output.log(`- ${label}`)
      }
    },
  )
}


interface AssetLabelsAddDeps {
  assetLabelsAdd: (input: AssetLabelsAddInput) => Promise<AssetLabelsAddResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetLabelsAdd(
  assetPath: string,
  labels: string[],
  jsonOutput: boolean,
  deps: AssetLabelsAddDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetLabelsAdd({ assetPath: assetPath.trim(), labels: normalizeLabels(labels) }),
    (result, output) => {
      output.log(`AssetPath: ${result.assetPath}`)
      output.log(`Added:     ${result.added.length}`)
      for (const label of result.labels) {
        output.log(`- ${label}`)
      }
    },
  )
}


interface AssetLabelsRemoveDeps {
  assetLabelsRemove: (input: AssetLabelsRemoveInput) => Promise<AssetLabelsRemoveResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleAssetLabelsRemove(
  assetPath: string,
  labels: string[],
  jsonOutput: boolean,
  deps: AssetLabelsRemoveDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.assetLabelsRemove({ assetPath: assetPath.trim(), labels: normalizeLabels(labels) }),
    (result, output) => {
      output.log(`AssetPath: ${result.assetPath}`)
      output.log(`Removed:   ${result.removed.length}`)
      for (const label of result.labels) {
        output.log(`- ${label}`)
      }
    },
  )
}

export function registerAsset(program: Command): void {
  const group = program
    .command('asset')
    .description('Find, inspect, and manage Unity asset files')

  group
    .command('find [query]')
    .description('Search for assets by query, type, or label')
    .option('--type <type>', 'Filter by asset type')
    .option('--label <label...>', 'Filter by label')
    .option('--limit <number>', 'Maximum number of results')
    .option('--offset <number>', 'Offset into results')
    .action(async (query: string | undefined, opts: AssetFindOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetFind(query, opts, ctx.jsonOutput, {
            assetFind: (input) => client.assetFind(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  group
    .command('get <assetPath>')
    .description('Get detailed information about an asset')
    .action(async (assetPath: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetGet(assetPath, ctx.jsonOutput, {
            assetGet: (input) => client.assetGet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  group
    .command('move <assetPath> <newPath>')
    .description('Move or rename an asset to a new path')
    .action(async (assetPath: string, newPath: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetMove(assetPath, newPath, ctx.jsonOutput, {
            assetMove: (input) => client.assetMove(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  group
    .command('copy <assetPath> <newPath>')
    .description('Copy an asset to a new path')
    .action(async (assetPath: string, newPath: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetCopy(assetPath, newPath, ctx.jsonOutput, {
            assetCopy: (input) => client.assetCopy(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  group
    .command('delete <assetPath>')
    .description('Delete an asset')
    .action(async (assetPath: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetDelete(assetPath, ctx.jsonOutput, {
            assetDelete: (input) => client.assetDelete(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  group
    .command('import <assetPath>')
    .description('Import or reimport an asset')
    .option('--options <importOptions>', 'Import options as a string')
    .action(async (assetPath: string, opts: AssetImportOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetImport(assetPath, opts, ctx.jsonOutput, {
            assetImport: (input) => client.assetImport(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  const importSettings = group
    .command('import-settings')
    .description('Get and set asset import settings')

  importSettings
    .command('get <assetPath>')
    .description('Get import settings for an asset')
    .option('--property <name...>', 'Property names to read')
    .action(async (assetPath: string, opts: AssetImportSettingsGetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetImportSettingsGet(assetPath, opts, ctx.jsonOutput, {
            assetImportSettingsGet: (input) => client.assetImportSettingsGet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  importSettings
    .command('set <assetPath>')
    .description('Set import settings for an asset')
    .option('--values <json>', 'Property values as JSON object')
    .option('--values-file <path>', 'Path to JSON file containing property values')
    .action(async (assetPath: string, opts: AssetImportSettingsSetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetImportSettingsSet(assetPath, opts, ctx.jsonOutput, {
            assetImportSettingsSet: (input) => client.assetImportSettingsSet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  const labels = group
    .command('labels')
    .description('Get, add, and remove asset labels')

  labels
    .command('get <assetPath>')
    .description('Get labels for an asset')
    .action(async (assetPath: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetLabelsGet(assetPath, ctx.jsonOutput, {
            assetLabelsGet: (input) => client.assetLabelsGet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  labels
    .command('add <assetPath> <labels...>')
    .description('Add labels to an asset')
    .action(async (assetPath: string, labelsArg: string[], _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetLabelsAdd(assetPath, labelsArg, ctx.jsonOutput, {
            assetLabelsAdd: (input) => client.assetLabelsAdd(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  labels
    .command('remove <assetPath> <labels...>')
    .description('Remove labels from an asset')
    .action(async (assetPath: string, labelsArg: string[], _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleAssetLabelsRemove(assetPath, labelsArg, ctx.jsonOutput, {
            assetLabelsRemove: (input) => client.assetLabelsRemove(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
