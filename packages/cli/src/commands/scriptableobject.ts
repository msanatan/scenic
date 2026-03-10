import { readFileSync } from 'node:fs'
import type { Command } from 'commander'
import type {
  ScriptableObjectCreateInput,
  ScriptableObjectCreateResult,
  ScriptableObjectGetInput,
  ScriptableObjectGetResult,
  ScriptableObjectUpdateInput,
  ScriptableObjectUpdateResult,
} from '@scenicai/sdk/commands/scriptableobject'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface ScriptableObjectCreateOptions {
  type?: string
  values?: string
  valuesFile?: string
  strict?: boolean
}

interface ScriptableObjectUpdateOptions {
  values?: string
  valuesFile?: string
  strict?: boolean
}

interface ScriptableObjectCreateDeps {
  create: (input: ScriptableObjectCreateInput) => Promise<ScriptableObjectCreateResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface ScriptableObjectGetDeps {
  get: (input: ScriptableObjectGetInput) => Promise<ScriptableObjectGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface ScriptableObjectUpdateDeps {
  update: (input: ScriptableObjectUpdateInput) => Promise<ScriptableObjectUpdateResult>
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

function parseValues(
  values: string | undefined,
  valuesFile: string | undefined,
  label: string,
  required: boolean,
): Record<string, unknown> | undefined {
  if (values != null && valuesFile != null) {
    throw new Error('Use either --values or --values-file, not both.')
  }

  if (values == null && valuesFile == null) {
    if (required) {
      throw new Error('Provide --values or --values-file.')
    }
    return undefined
  }

  const text = valuesFile != null ? readFileSync(valuesFile, 'utf-8') : values ?? ''
  return parseJsonObject(text, label)
}

export async function handleScriptableObjectCreate(
  assetPath: string,
  opts: ScriptableObjectCreateOptions,
  jsonOutput: boolean,
  deps: ScriptableObjectCreateDeps,
): Promise<void> {
  if (assetPath.trim().length === 0) {
    throw new Error('Asset path is required.')
  }
  if (opts.type == null || opts.type.trim().length === 0) {
    throw new Error('Provide --type.')
  }

  const input: ScriptableObjectCreateInput = {
    assetPath: assetPath.trim(),
    type: opts.type.trim(),
    initialValues: parseValues(opts.values, opts.valuesFile, 'initial values', false),
    strict: opts.strict === true,
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.create(input),
    (result, output) => {
      output.log(`Created: ${result.asset.assetPath}`)
      output.log(`Type:    ${result.asset.type}`)
      output.log(`Id:      ${result.asset.instanceId}`)
      output.log(`Applied: ${result.appliedFields.length}`)
      output.log(`Ignored: ${result.ignoredFields.length}`)
    },
  )
}

export async function handleScriptableObjectGet(
  assetPath: string,
  jsonOutput: boolean,
  deps: ScriptableObjectGetDeps,
): Promise<void> {
  if (assetPath.trim().length === 0) {
    throw new Error('Asset path is required.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.get({ assetPath: assetPath.trim() }),
    (result, output) => {
      output.log(`Asset: ${result.asset.assetPath}`)
      output.log(`Type:  ${result.asset.type}`)
      output.log(`Id:    ${result.asset.instanceId}`)
      output.log(`Fields serialized: ${Object.keys(result.serialized).length}`)
    },
  )
}

export async function handleScriptableObjectUpdate(
  assetPath: string,
  opts: ScriptableObjectUpdateOptions,
  jsonOutput: boolean,
  deps: ScriptableObjectUpdateDeps,
): Promise<void> {
  if (assetPath.trim().length === 0) {
    throw new Error('Asset path is required.')
  }

  const input: ScriptableObjectUpdateInput = {
    assetPath: assetPath.trim(),
    values: parseValues(opts.values, opts.valuesFile, 'values', true) ?? {},
    strict: opts.strict === true,
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.update(input),
    (result, output) => {
      output.log(`Updated: ${result.asset.assetPath}`)
      output.log(`Type:    ${result.asset.type}`)
      output.log(`Id:      ${result.asset.instanceId}`)
      output.log(`Applied: ${result.appliedFields.length}`)
      output.log(`Ignored: ${result.ignoredFields.length}`)
    },
  )
}

export function registerScriptableObject(program: Command): void {
  const scriptableObject = program
    .command('scriptableobject')
    .description('Create, read, and modify ScriptableObject assets')

  scriptableObject
    .command('create <assetPath>')
    .description('Create a ScriptableObject asset')
    .requiredOption('--type <type>', 'Fully-qualified ScriptableObject type name')
    .option('--values <json>', 'Initial values as JSON object')
    .option('--values-file <path>', 'Path to JSON file containing initial values')
    .option('--strict', 'Fail if any provided initial value key does not exist on the type')
    .action(async (assetPath: string, opts: ScriptableObjectCreateOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleScriptableObjectCreate(assetPath, opts, ctx.jsonOutput, {
            create: (input) => client.scriptableObjectCreate(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  scriptableObject
    .command('get <assetPath>')
    .description('Get a ScriptableObject asset and serialized fields')
    .action(async (assetPath: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleScriptableObjectGet(assetPath, ctx.jsonOutput, {
            get: (input) => client.scriptableObjectGet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  scriptableObject
    .command('update <assetPath>')
    .description('Update ScriptableObject fields')
    .option('--values <json>', 'Values patch as JSON object')
    .option('--values-file <path>', 'Path to JSON file containing values patch')
    .option('--strict', 'Fail if any provided field key does not exist on the type')
    .action(async (assetPath: string, opts: ScriptableObjectUpdateOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleScriptableObjectUpdate(assetPath, opts, ctx.jsonOutput, {
            update: (input) => client.scriptableObjectUpdate(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
