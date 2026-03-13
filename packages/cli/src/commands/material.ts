import { readFileSync } from 'node:fs'
import type { Command } from 'commander'
import type {
  MaterialColor,
  MaterialCreateInput,
  MaterialCreateResult,
  MaterialGetInput,
  MaterialGetResult,
  MaterialAssignInput,
  MaterialAssignResult,
  MaterialPropertiesGetInput,
  MaterialPropertiesGetResult,
  MaterialPropertiesSetInput,
  MaterialPropertiesSetResult,
  MaterialPropertySetValue,
  MaterialVector,
} from '@scenicai/sdk/commands/material'
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
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      if (assetPath.trim().length === 0) {
        throw new Error('Asset path is required.')
      }
      const shader = opts.shader?.trim()
      const input: MaterialCreateInput = {
        assetPath: assetPath.trim(),
        shader: shader != null && shader.length > 0 ? shader : undefined,
      }
      return deps.materialCreate(input)
    },
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

interface MaterialPropertiesGetOptions {
  names?: string[]
}

interface MaterialPropertiesSetOptions {
  values?: string
  valuesFile?: string
  strict?: boolean
}

export async function handleMaterialGet(
  assetPath: string,
  jsonOutput: boolean,
  deps: MaterialGetDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      if (assetPath.trim().length === 0) {
        throw new Error('Asset path is required.')
      }
      return deps.materialGet({ assetPath: assetPath.trim() })
    },
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

interface MaterialPropertiesGetDeps {
  materialPropertiesGet: (input: MaterialPropertiesGetInput) => Promise<MaterialPropertiesGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface MaterialPropertiesSetDeps {
  materialPropertiesSet: (input: MaterialPropertiesSetInput) => Promise<MaterialPropertiesSetResult>
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
): Record<string, MaterialPropertySetValue> {
  if (values != null && valuesFile != null) {
    throw new Error('Use either --values or --values-file, not both.')
  }

  if (values == null && valuesFile == null) {
    throw new Error('Provide --values or --values-file.')
  }

  const text = valuesFile != null ? readFileSync(valuesFile, 'utf-8') : values ?? ''
  return parseJsonObject(text, 'values') as Record<string, MaterialPropertySetValue>
}

function formatColor(value: MaterialColor): string {
  return `${value.r},${value.g},${value.b},${value.a}`
}

function formatVector(value: MaterialVector): string {
  return `${value.x},${value.y},${value.z},${value.w}`
}

export async function handleMaterialAssign(
  opts: MaterialAssignOptions,
  jsonOutput: boolean,
  deps: MaterialAssignDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
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
      return deps.materialAssign({
        path: path?.trim(),
        instanceId,
        assetPath: materialAssetPath,
        rendererIndex,
        slot,
      })
    },
    (result, output) => {
      output.log(`Target:   ${result.targetPath}`)
      output.log(`Renderer: ${result.rendererType} [index=${result.rendererIndex}, id=${result.rendererInstanceId}]`)
      output.log(`Slot:     ${result.slot}`)
      output.log(`Material: ${result.material.assetPath}`)
    },
  )
}

export async function handleMaterialPropertiesGet(
  assetPath: string,
  opts: MaterialPropertiesGetOptions,
  jsonOutput: boolean,
  deps: MaterialPropertiesGetDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      if (assetPath.trim().length === 0) {
        throw new Error('Asset path is required.')
      }
      const names = opts.names?.map((name) => name.trim()).filter((name) => name.length > 0)
      return deps.materialPropertiesGet({ assetPath: assetPath.trim(), names })
    },
    (result, output) => {
      output.log(`Material: ${result.material.assetPath}`)
      output.log(`Properties: ${Object.keys(result.properties).length}`)
      for (const [name, property] of Object.entries(result.properties)) {
        if ((property.type === 'float' || property.type === 'range') && typeof property.value === 'number') {
          output.log(`- ${name} [${property.type}] = ${property.value}`)
          continue
        }

        if (property.type === 'int' && typeof property.value === 'number') {
          output.log(`- ${name} [int] = ${property.value}`)
          continue
        }

        if (
          property.type === 'color'
          && property.value != null
          && typeof property.value === 'object'
          && 'r' in property.value
        ) {
          output.log(`- ${name} [color] = ${formatColor(property.value as MaterialColor)}`)
          continue
        }

        if (
          property.type === 'vector'
          && property.value != null
          && typeof property.value === 'object'
          && 'x' in property.value
        ) {
          output.log(`- ${name} [vector] = ${formatVector(property.value as MaterialVector)}`)
          continue
        }

        if (property.type === 'texture') {
          output.log(`- ${name} [texture] = ${property.assetPath ?? '<none>'}`)
          continue
        }

        output.log(`- ${name} [${property.type}]`)
      }
    },
  )
}

export async function handleMaterialPropertiesSet(
  assetPath: string,
  opts: MaterialPropertiesSetOptions,
  jsonOutput: boolean,
  deps: MaterialPropertiesSetDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      if (assetPath.trim().length === 0) {
        throw new Error('Asset path is required.')
      }
      const input: MaterialPropertiesSetInput = {
        assetPath: assetPath.trim(),
        values: parseValues(opts.values, opts.valuesFile),
        strict: opts.strict === true,
      }
      return deps.materialPropertiesSet(input)
    },
    (result, output) => {
      output.log(`Updated: ${result.material.assetPath}`)
      output.log(`Applied: ${result.appliedProperties.length}`)
      output.log(`Ignored: ${result.ignoredProperties.length}`)
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

  const properties = group
    .command('props')
    .description('Get and set material properties')

  properties
    .command('get <assetPath>')
    .description('Get material properties by name or list all supported properties')
    .option('--name <propertyName...>', 'Property names to read (repeat or pass multiple values)')
    .action(async (assetPath: string, opts: MaterialPropertiesGetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleMaterialPropertiesGet(assetPath, opts, ctx.jsonOutput, {
            materialPropertiesGet: (input) => client.materialPropertiesGet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  properties
    .command('set <assetPath>')
    .description('Set material properties from a JSON object')
    .option('--values <json>', 'Property updates as JSON object')
    .option('--values-file <path>', 'Path to JSON file containing property updates')
    .option('--strict', 'Fail if any provided property name does not exist on the shader')
    .action(async (assetPath: string, opts: MaterialPropertiesSetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleMaterialPropertiesSet(assetPath, opts, ctx.jsonOutput, {
            materialPropertiesSet: (input) => client.materialPropertiesSet(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

}
