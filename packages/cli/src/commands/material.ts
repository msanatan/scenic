import type { Command } from 'commander'
import type {
  MaterialCreateInput,
  MaterialCreateResult,
  MaterialGetInput,
  MaterialGetResult,
} from '@scenicai/sdk'
import { runWithOutput } from './output.ts'
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

export function registerMaterial(program: Command): void {
  const group = program
    .command('material')
    .description('Create and inspect Unity material assets')

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

}
