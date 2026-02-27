import type { Command } from 'commander'
import type {
  MaterialCreateInput,
  MaterialCreateResult,
  MaterialGetInput,
  MaterialGetResult,
  MaterialAssignInput,
  MaterialAssignResult,
} from '@scenicai/sdk'
import { runWithOutput } from './output.ts'
import { parseOptionalInt } from './parse.ts'
import { withUnityClient } from './with-unity-client.ts'

interface MaterialCreateDeps {
  materialCreate: (input: MaterialCreateInput) => Promise<MaterialCreateResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface MaterialCreateOptions {
  shader?: string
}

export async function handleMaterialCreate(
  assetPath: string,
  opts: MaterialCreateOptions,
  jsonOutput: boolean,
  deps: MaterialCreateDeps,
): Promise<void> {
  if (assetPath.trim().length === 0) {
    throw new Error('Asset path is required.')
  }

  const shader = opts.shader?.trim()
  const input: MaterialCreateInput = {
    assetPath: assetPath.trim(),
    shader: shader != null && shader.length > 0 ? shader : undefined,
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.materialCreate(input),
    (result, output) => {
      output.log(`Created: ${result.material.assetPath}`)
      output.log(`Name:    ${result.material.name}`)
      output.log(`Shader:  ${result.material.shader}`)
      output.log(`Id:      ${result.material.instanceId}`)
    },
  )
}

interface MaterialGetDeps {
  materialGet: (input: MaterialGetInput) => Promise<MaterialGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface MaterialAssignOptions {
  path?: string
  instanceId?: string
  assetPath?: string
  rendererIndex?: string
  slot?: string
}

export async function handleMaterialGet(
  assetPath: string,
  jsonOutput: boolean,
  deps: MaterialGetDeps,
): Promise<void> {
  if (assetPath.trim().length === 0) {
    throw new Error('Asset path is required.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.materialGet({ assetPath: assetPath.trim() }),
    (result, output) => {
      output.log(`Asset:  ${result.material.assetPath}`)
      output.log(`Name:   ${result.material.name}`)
      output.log(`Shader: ${result.material.shader}`)
      output.log(`Id:     ${result.material.instanceId}`)
    },
  )
}

interface MaterialAssignDeps {
  materialAssign: (input: MaterialAssignInput) => Promise<MaterialAssignResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleMaterialAssign(
  opts: MaterialAssignOptions,
  jsonOutput: boolean,
  deps: MaterialAssignDeps,
): Promise<void> {
  const instanceId = parseOptionalInt(opts.instanceId, '--instance-id')
  const path = opts.path
  if ((path == null || path.trim().length === 0) && instanceId == null) {
    throw new Error('Provide target via --path or --instance-id.')
  }
  if (path != null && path.trim().length > 0 && instanceId != null) {
    throw new Error('Use either --path or --instance-id, not both.')
  }
  const materialAssetPath = opts.assetPath?.trim()
  if (materialAssetPath == null || materialAssetPath.length === 0) {
    throw new Error('Provide --asset-path.')
  }

  const rendererIndex = parseOptionalInt(opts.rendererIndex, '--renderer-index')
  const slot = parseOptionalInt(opts.slot, '--slot')
  if (rendererIndex != null && rendererIndex < 0) {
    throw new Error('--renderer-index must be a non-negative integer.')
  }
  if (slot != null && slot < 0) {
    throw new Error('--slot must be a non-negative integer.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.materialAssign({
      path: path?.trim(),
      instanceId,
      assetPath: materialAssetPath,
      rendererIndex,
      slot,
    }),
    (result, output) => {
      output.log(`Target:   ${result.targetPath}`)
      output.log(`Renderer: ${result.rendererType} [index=${result.rendererIndex}, id=${result.rendererInstanceId}]`)
      output.log(`Slot:     ${result.slot}`)
      output.log(`Material: ${result.material.assetPath}`)
    },
  )
}

export function registerMaterial(program: Command): void {
  const group = program
    .command('material')
    .description('Create, inspect, and assign Unity material assets')

  group
    .command('create <assetPath>')
    .description('Create a material asset at a project-relative path')
    .option('--shader <name>', 'Shader name to assign (defaults to a common pipeline shader)')
    .action(async (assetPath: string, opts: MaterialCreateOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleMaterialCreate(assetPath, opts, ctx.jsonOutput, {
            materialCreate: (input) => client.materialCreate(input),
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
    .description('Get a material asset summary')
    .action(async (assetPath: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleMaterialGet(assetPath, ctx.jsonOutput, {
            materialGet: (input) => client.materialGet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  group
    .command('assign')
    .description('Assign a material asset to a Renderer on a target GameObject')
    .requiredOption('--asset-path <assetPath>', 'Material asset path (for example, Assets/Materials/Foo.mat)')
    .option('--path <path>', 'Target GameObject path (for example, /Root/Child)')
    .option('--instance-id <id>', 'Target GameObject instance ID')
    .option('--renderer-index <index>', 'Renderer component index on target (default: 0)')
    .option('--slot <index>', 'Material slot index on renderer (default: 0)')
    .action(async (opts: MaterialAssignOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleMaterialAssign(opts, ctx.jsonOutput, {
            materialAssign: (input) => client.materialAssign(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

}
