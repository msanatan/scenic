import type { Command } from 'commander'
import type { CreateTransform, GameObjectCreateInput, GameObjectCreateResult, GameObjectDimension, PrimitiveTypeName, TransformSpace } from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface GameObjectCreateOptions {
  parent?: string
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

export async function handleGameObjectCreate(
  name: string,
  opts: GameObjectCreateOptions,
  jsonOutput: boolean,
  deps: GameObjectCreateDeps,
): Promise<void> {
  const input: GameObjectCreateInput = {
    name,
    parent: opts.parent,
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
      output.log(`Active:  ${result.isActive ? 'yes' : 'no'}`)
      output.log(`Sibling: ${result.siblingIndex}`)
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
}
