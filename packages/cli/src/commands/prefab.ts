import type { Command } from 'commander'
import type {
  PrefabCreateTransform,
  PrefabInstantiateInput,
  PrefabInstantiateResult,
  PrefabSaveInput,
  PrefabSaveResult,
  PrefabTransformSpace,
} from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface PrefabInstantiateOptions {
  parent?: string
  parentInstanceId?: string
  space?: string
  position?: string
  rotation?: string
  scale?: string
}

interface PrefabInstantiateDeps {
  instantiate: (input: PrefabInstantiateInput) => Promise<PrefabInstantiateResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface PrefabSaveOptions {
  path?: string
  instanceId?: string
}

interface PrefabSaveDeps {
  save: (input: PrefabSaveInput) => Promise<PrefabSaveResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function parseSpace(value: string | undefined): PrefabTransformSpace | undefined {
  if (value == null) {
    return undefined
  }
  if (value === 'local' || value === 'world') {
    return value
  }
  throw new Error('--space must be one of: local, world.')
}

function parseVector3(value: string | undefined, label: string): { x: number; y: number; z: number } | undefined {
  if (value == null) {
    return undefined
  }
  const parts = value.split(',').map((part) => Number.parseFloat(part.trim()))
  if (parts.length !== 3 || parts.some((part) => !Number.isFinite(part))) {
    throw new Error(`${label} must be in x,y,z format.`)
  }
  return { x: parts[0], y: parts[1], z: parts[2] }
}

function parseTransform(opts: PrefabInstantiateOptions): PrefabCreateTransform | undefined {
  const transform: PrefabCreateTransform = {
    space: parseSpace(opts.space),
    position: parseVector3(opts.position, '--position'),
    rotation: parseVector3(opts.rotation, '--rotation'),
    scale: parseVector3(opts.scale, '--scale'),
  }

  if (transform.space == null && transform.position == null && transform.rotation == null && transform.scale == null) {
    return undefined
  }

  return transform
}

function parseInstanceId(value: string | undefined, label: string): number | undefined {
  if (value == null) {
    return undefined
  }
  const parsed = Number.parseInt(value, 10)
  if (!Number.isInteger(parsed)) {
    throw new Error(`${label} must be an integer.`)
  }
  return parsed
}

export async function handlePrefabInstantiate(
  prefabPath: string,
  opts: PrefabInstantiateOptions,
  jsonOutput: boolean,
  deps: PrefabInstantiateDeps,
): Promise<void> {
  if (opts.parent != null && opts.parentInstanceId != null) {
    throw new Error('Use either --parent or --parent-instance-id, not both.')
  }

  const input: PrefabInstantiateInput = {
    prefabPath,
    parentPath: opts.parent,
    parentInstanceId: parseInstanceId(opts.parentInstanceId, '--parent-instance-id'),
    transform: parseTransform(opts),
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.instantiate(input),
    (result, output) => {
      output.log(`Instantiated: ${result.path}`)
      output.log(`Id:           ${result.instanceId}`)
      output.log(`Prefab:       ${result.prefabPath}`)
      output.log(`Sibling:      ${result.siblingIndex}`)
    },
  )
}

export async function handlePrefabSave(
  prefabPath: string,
  opts: PrefabSaveOptions,
  jsonOutput: boolean,
  deps: PrefabSaveDeps,
): Promise<void> {
  const instanceId = parseInstanceId(opts.instanceId, '--instance-id')
  const path = opts.path

  if ((path == null || path.length === 0) && instanceId == null) {
    throw new Error('Provide either --path or --instance-id.')
  }
  if (path != null && instanceId != null) {
    throw new Error('Use either --path or --instance-id, not both.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.save({ prefabPath, path, instanceId }),
    (result, output) => {
      output.log(`Saved:        ${result.prefabPath}`)
      output.log(`Source:       ${result.sourcePath}`)
      output.log(`Source Id:    ${result.sourceInstanceId}`)
    },
  )
}

export function registerPrefab(program: Command): void {
  const prefab = program
    .command('prefab')
    .description('Instantiate and save prefabs')

  prefab
    .command('instantiate <prefabPath>')
    .description('Instantiate a prefab asset into the active scene')
    .option('--parent <path>', 'Parent GameObject path (e.g. /Canvas)')
    .option('--parent-instance-id <id>', 'Parent GameObject instance ID')
    .option('--space <space>', 'Transform space: local or world')
    .option('--position <x,y,z>', 'Position override')
    .option('--rotation <x,y,z>', 'Rotation (Euler) override in degrees')
    .option('--scale <x,y,z>', 'Local scale override')
    .action(async (prefabPath: string, opts: PrefabInstantiateOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handlePrefabInstantiate(prefabPath, opts, ctx.jsonOutput, {
            instantiate: (input) => client.prefabInstantiate(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  prefab
    .command('save <prefabPath>')
    .description('Save a scene GameObject hierarchy as a prefab asset')
    .option('--path <path>', 'Scene path to the GameObject to save')
    .option('--instance-id <id>', 'Scene GameObject instance ID to save')
    .action(async (prefabPath: string, opts: PrefabSaveOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handlePrefabSave(prefabPath, opts, ctx.jsonOutput, {
            save: (input) => client.prefabSave(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
