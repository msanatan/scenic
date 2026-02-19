import type { Command } from 'commander'
import type {
  CreateTransform,
  GameObjectCreateInput,
  GameObjectCreateResult,
  GameObjectDestroyInput,
  GameObjectDestroyResult,
  GameObjectDimension,
  GameObjectFindQuery,
  GameObjectFindResult,
  GameObjectGetInput,
  GameObjectGetResult,
  GameObjectReparentInput,
  GameObjectReparentResult,
  GameObjectUpdateInput,
  GameObjectUpdateResult,
  PrimitiveTypeName,
  TransformSpace,
} from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface GameObjectCreateOptions {
  parent?: string
  parentInstanceId?: string
  dimension?: string
  primitive?: string
  space?: string
  position?: string
  rotation?: string
  scale?: string
}

interface GameObjectCreateDeps {
  create: (input: GameObjectCreateInput) => Promise<GameObjectCreateResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface GameObjectDestroyOptions {
  path?: string
  instanceId?: string
}

interface GameObjectDestroyDeps {
  destroy: (input: GameObjectDestroyInput) => Promise<GameObjectDestroyResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface GameObjectUpdateOptions {
  path?: string
  instanceId?: string
  name?: string
  tag?: string
  layer?: string
  isStatic?: string
  space?: string
  position?: string
  rotation?: string
  scale?: string
}

interface GameObjectUpdateDeps {
  update: (input: GameObjectUpdateInput) => Promise<GameObjectUpdateResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface GameObjectReparentOptions {
  path?: string
  instanceId?: string
  parent?: string
  parentInstanceId?: string
  toRoot?: boolean
  worldPositionStays?: boolean
}

interface GameObjectReparentDeps {
  reparent: (input: GameObjectReparentInput) => Promise<GameObjectReparentResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface GameObjectGetOptions {
  path?: string
  instanceId?: string
}

interface GameObjectGetDeps {
  get: (input: GameObjectGetInput) => Promise<GameObjectGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface GameObjectFindOptions {
  scenePath?: string
  includeInactive?: boolean
  limit?: string
  offset?: string
}

interface GameObjectFindDeps {
  find: (query: GameObjectFindQuery) => Promise<GameObjectFindResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function parseDimension(value: string | undefined): GameObjectDimension | undefined {
  if (value == null) {
    return undefined
  }
  if (value === '2d' || value === '3d') {
    return value
  }
  throw new Error('--dimension must be one of: 2d, 3d.')
}

function parsePrimitive(value: string | undefined): PrimitiveTypeName | undefined {
  if (value == null) {
    return undefined
  }
  if (value === 'cube' || value === 'sphere' || value === 'capsule' || value === 'cylinder' || value === 'plane' || value === 'quad') {
    return value
  }
  throw new Error('--primitive must be one of: cube, sphere, capsule, cylinder, plane, quad.')
}

function parseSpace(value: string | undefined): TransformSpace | undefined {
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

function parseTransform(opts: GameObjectCreateOptions): CreateTransform | undefined {
  const transform: CreateTransform = {
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

function parseParentInstanceId(value: string | undefined): number | undefined {
  if (value == null) {
    return undefined
  }
  const parsed = Number.parseInt(value, 10)
  if (!Number.isInteger(parsed)) {
    throw new Error('--parent-instance-id must be an integer.')
  }
  return parsed
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

function parseBoolean(value: string | undefined, label: string): boolean | undefined {
  if (value == null) {
    return undefined
  }
  if (value === 'true') {
    return true
  }
  if (value === 'false') {
    return false
  }
  throw new Error(`${label} must be true or false.`)
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
  if (!Number.isInteger(parsed) || parsed < minimum) {
    if (minimum <= 0) {
      throw new Error(`${label} must be a non-negative integer.`)
    }
    throw new Error(`${label} must be an integer >= ${minimum}.`)
  }
  return parsed
}

export async function handleGameObjectCreate(
  name: string,
  opts: GameObjectCreateOptions,
  jsonOutput: boolean,
  deps: GameObjectCreateDeps,
): Promise<void> {
  if (opts.parent != null && opts.parentInstanceId != null) {
    throw new Error('Use either --parent or --parent-instance-id, not both.')
  }

  const input: GameObjectCreateInput = {
    name,
    parent: opts.parent,
    parentInstanceId: parseParentInstanceId(opts.parentInstanceId),
    dimension: parseDimension(opts.dimension),
    primitive: parsePrimitive(opts.primitive),
    transform: parseTransform(opts),
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.create(input),
    (result, output) => {
      output.log(`Created: ${result.path}`)
      output.log(`Id:      ${result.instanceId}`)
      output.log(`Active:  ${result.isActive ? 'yes' : 'no'}`)
      output.log(`Sibling: ${result.siblingIndex}`)
    },
  )
}

export async function handleGameObjectDestroy(
  opts: GameObjectDestroyOptions,
  jsonOutput: boolean,
  deps: GameObjectDestroyDeps,
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
    () => deps.destroy({ path, instanceId }),
    (result, output) => {
      output.log(`Destroyed: ${result.path}`)
      output.log(`Id:        ${result.instanceId}`)
      output.log(`Name:      ${result.name}`)
    },
  )
}

export async function handleGameObjectUpdate(
  opts: GameObjectUpdateOptions,
  jsonOutput: boolean,
  deps: GameObjectUpdateDeps,
): Promise<void> {
  const instanceId = parseInstanceId(opts.instanceId, '--instance-id')
  const path = opts.path

  if ((path == null || path.length === 0) && instanceId == null) {
    throw new Error('Provide either --path or --instance-id.')
  }
  if (path != null && instanceId != null) {
    throw new Error('Use either --path or --instance-id, not both.')
  }

  const transform = parseTransform(opts)
  const isStatic = parseBoolean(opts.isStatic, '--is-static')
  const hasAnyUpdate = opts.name != null || opts.tag != null || opts.layer != null || isStatic != null || transform != null
  if (!hasAnyUpdate) {
    throw new Error('Provide at least one update field.')
  }

  const input: GameObjectUpdateInput = {
    path,
    instanceId,
    name: opts.name,
    tag: opts.tag,
    layer: opts.layer,
    isStatic,
    transform,
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.update(input),
    (result, output) => {
      output.log(`Updated: ${result.path}`)
      output.log(`Id:      ${result.instanceId}`)
      output.log(`Tag:     ${result.tag}`)
      output.log(`Layer:   ${result.layer}`)
      output.log(`Static:  ${result.isStatic ? 'yes' : 'no'}`)
      output.log(`Pos:     ${result.transform.position.x},${result.transform.position.y},${result.transform.position.z}`)
    },
  )
}

export async function handleGameObjectReparent(
  opts: GameObjectReparentOptions,
  jsonOutput: boolean,
  deps: GameObjectReparentDeps,
): Promise<void> {
  const instanceId = parseInstanceId(opts.instanceId, '--instance-id')
  const path = opts.path

  if ((path == null || path.length === 0) && instanceId == null) {
    throw new Error('Provide target via --path or --instance-id.')
  }
  if (path != null && instanceId != null) {
    throw new Error('Use either --path or --instance-id for target, not both.')
  }

  const parentInstanceId = parseInstanceId(opts.parentInstanceId, '--parent-instance-id')
  const parentPath = opts.parent
  const toRoot = opts.toRoot === true

  if (toRoot && (parentPath != null || parentInstanceId != null)) {
    throw new Error('Use either --to-root or a parent selector, not both.')
  }
  if (!toRoot && (parentPath == null && parentInstanceId == null)) {
    throw new Error('Provide destination via --parent/--parent-instance-id, or set --to-root.')
  }
  if (parentPath != null && parentInstanceId != null) {
    throw new Error('Use either --parent or --parent-instance-id, not both.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.reparent({
      path,
      instanceId,
      parentPath,
      parentInstanceId,
      toRoot,
      worldPositionStays: opts.worldPositionStays === true,
    }),
    (result, output) => {
      output.log(`Reparented: ${result.path}`)
      output.log(`Id:        ${result.instanceId}`)
      output.log(`Parent:    ${result.parentPath ?? '(root)'}`)
      output.log(`Sibling:   ${result.siblingIndex}`)
    },
  )
}

export async function handleGameObjectGet(
  opts: GameObjectGetOptions,
  jsonOutput: boolean,
  deps: GameObjectGetDeps,
): Promise<void> {
  const instanceId = parseInstanceId(opts.instanceId, '--instance-id')
  const path = opts.path

  if ((path == null || path.length === 0) && instanceId == null) {
    throw new Error('Provide target via --path or --instance-id.')
  }
  if (path != null && instanceId != null) {
    throw new Error('Use either --path or --instance-id, not both.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.get({ path, instanceId }),
    (result, output) => {
      output.log(`GameObject: ${result.path}`)
      output.log(`Id:         ${result.instanceId}`)
      output.log(`Parent:     ${result.parentPath ?? '(root)'}`)
      output.log(`Active:     ${result.isActive ? 'yes' : 'no'}`)
      output.log(`Tag:        ${result.tag}`)
      output.log(`Layer:      ${result.layer}`)
      output.log(`Static:     ${result.isStatic ? 'yes' : 'no'}`)
      output.log(`Sibling:    ${result.siblingIndex}`)
    },
  )
}

export async function handleGameObjectFind(
  query: string,
  opts: GameObjectFindOptions,
  jsonOutput: boolean,
  deps: GameObjectFindDeps,
): Promise<void> {
  if (query.trim().length === 0) {
    throw new Error('Provide a non-empty query.')
  }

  const input: GameObjectFindQuery = {
    query,
    scenePath: opts.scenePath,
    includeInactive: opts.includeInactive === true,
    limit: parseIntWithMinimum(opts.limit, '--limit', 50, 1),
    offset: parseIntWithMinimum(opts.offset, '--offset', 0, 0),
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.find(input),
    (result, output) => {
      output.log(`Matches: ${result.gameObjects.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const item of result.gameObjects) {
        output.log(`${item.path} [id=${item.instanceId}, active=${item.isActive ? 'yes' : 'no'}, sibling=${item.siblingIndex}]`)
      }
    },
  )
}

export function registerGameObject(program: Command): void {
  const gameObject = program
    .command('gameobject')
    .description('Create and manage Unity GameObjects')

  gameObject
    .command('create <name>')
    .description('Create a GameObject with optional parent, primitive, and transform')
    .option('--parent <path>', 'Parent GameObject path, e.g. /Environment')
    .option('--parent-instance-id <id>', 'Parent GameObject instance ID (session-local)')
    .option('--dimension <dimension>', 'Object type (2d|3d). 2d auto-adds SpriteRenderer')
    .option('--primitive <primitive>', '3D primitive (cube|sphere|capsule|cylinder|plane|quad)')
    .option('--space <space>', 'Transform space (local|world)')
    .option('--position <x,y,z>', 'Initial position vector')
    .option('--rotation <x,y,z>', 'Initial euler rotation vector')
    .option('--scale <x,y,z>', 'Initial local scale vector')
    .action(async (name: string, opts: GameObjectCreateOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleGameObjectCreate(name, opts, ctx.jsonOutput, {
            create: (input) => client.gameObjectCreate(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  gameObject
    .command('destroy')
    .description('Destroy a GameObject by path or instance ID')
    .option('--path <path>', 'GameObject path, e.g. /Environment/Enemy_01')
    .option('--instance-id <id>', 'GameObject instance ID (session-local)')
    .action(async (opts: GameObjectDestroyOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleGameObjectDestroy(opts, ctx.jsonOutput, {
            destroy: (input) => client.gameObjectDestroy(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  gameObject
    .command('update')
    .description('Update GameObject properties by path or instance ID')
    .option('--path <path>', 'GameObject path, e.g. /Environment/Enemy_01')
    .option('--instance-id <id>', 'GameObject instance ID (session-local)')
    .option('--name <name>', 'Updated GameObject name')
    .option('--tag <tag>', 'Updated Unity tag')
    .option('--layer <layer>', 'Updated Unity layer name or index')
    .option('--is-static <true|false>', 'Set static flag')
    .option('--space <space>', 'Transform space (local|world)')
    .option('--position <x,y,z>', 'Updated position vector')
    .option('--rotation <x,y,z>', 'Updated euler rotation vector')
    .option('--scale <x,y,z>', 'Updated local scale vector')
    .action(async (opts: GameObjectUpdateOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleGameObjectUpdate(opts, ctx.jsonOutput, {
            update: (input) => client.gameObjectUpdate(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  gameObject
    .command('get')
    .description('Get GameObject info by path or instance ID')
    .option('--path <path>', 'Target GameObject path, e.g. /Environment/Enemy_01')
    .option('--instance-id <id>', 'Target GameObject instance ID (session-local)')
    .action(async (opts: GameObjectGetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleGameObjectGet(opts, ctx.jsonOutput, {
            get: (input) => client.gameObjectGet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  gameObject
    .command('reparent')
    .description('Reparent a GameObject to another parent or root')
    .option('--path <path>', 'Target GameObject path, e.g. /Environment/Enemy_01')
    .option('--instance-id <id>', 'Target GameObject instance ID (session-local)')
    .option('--parent <path>', 'Destination parent GameObject path')
    .option('--parent-instance-id <id>', 'Destination parent instance ID')
    .option('--to-root', 'Move target to scene root')
    .option('--world-position-stays', 'Preserve world transform while reparenting')
    .action(async (opts: GameObjectReparentOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleGameObjectReparent(opts, ctx.jsonOutput, {
            reparent: (input) => client.gameObjectReparent(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  gameObject
    .command('find <query>')
    .description('Find GameObjects in the scene hierarchy by name or path')
    .option('--scene-path <path>', 'Restrict search to a loaded scene path')
    .option('--include-inactive', 'Include inactive GameObjects')
    .option('--limit <number>', 'Number of results to return (default: 50)')
    .option('--offset <number>', 'Offset into result list (default: 0)')
    .action(async (query: string, opts: GameObjectFindOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleGameObjectFind(query, opts, ctx.jsonOutput, {
            find: (input) => client.gameObjectFind(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
