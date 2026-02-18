import type { Command } from 'commander'
import type { SceneActiveResult, SceneCreateResult, SceneListQuery, SceneListResult, SceneOpenResult } from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface SceneActiveDeps {
  active: () => Promise<SceneActiveResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleSceneActive(
  jsonOutput: boolean,
  deps: SceneActiveDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.active(),
    (result, output) => {
      output.log(`Name:    ${result.scene.name}`)
      output.log(`Path:    ${result.scene.path}`)
      output.log(`Dirty:   ${result.scene.isDirty ? 'yes' : 'no'}`)
    },
  )
}

interface SceneOpenDeps {
  open: (path: string) => Promise<SceneOpenResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleSceneOpen(
  path: string,
  jsonOutput: boolean,
  deps: SceneOpenDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.open(path),
    (result, output) => {
      output.log(`Name:    ${result.scene.name}`)
      output.log(`Path:    ${result.scene.path}`)
      output.log(`Dirty:   ${result.scene.isDirty ? 'yes' : 'no'}`)
    },
  )
}

interface SceneListCommandOptions {
  filter?: string
  limit?: string
  offset?: string
}

interface SceneListDeps {
  list: (query?: SceneListQuery) => Promise<SceneListResult>
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

export async function handleSceneList(
  opts: SceneListCommandOptions,
  jsonOutput: boolean,
  deps: SceneListDeps,
): Promise<void> {
  const query: SceneListQuery = {
    filter: opts.filter,
    limit: parseIntWithMinimum(opts.limit, '--limit', 50, 1),
    offset: parseIntWithMinimum(opts.offset, '--offset', 0, 0),
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.list(query),
    (result, output) => {
      output.log(`Scenes: ${result.scenes.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const scene of result.scenes) {
        output.log(`${scene.path}`)
      }
    },
  )
}

interface SceneCreateDeps {
  create: (path: string) => Promise<SceneCreateResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleSceneCreate(
  path: string,
  jsonOutput: boolean,
  deps: SceneCreateDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.create(path),
    (result, output) => {
      output.log(`Name:    ${result.scene.name}`)
      output.log(`Path:    ${result.scene.path}`)
      output.log(`Dirty:   ${result.scene.isDirty ? 'yes' : 'no'}`)
    },
  )
}

export function registerScene(program: Command): void {
  const scene = program
    .command('scene')
    .description('Read and modify Unity scenes')

  scene
    .command('active')
    .description('Show the active scene')
    .action(async (_opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleSceneActive(ctx.jsonOutput, {
            active: () => client.sceneActive(),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  scene
    .command('list')
    .description('List scenes in the project with pagination')
    .option('--filter <text>', 'Filter by scene path substring')
    .option('--limit <number>', 'Number of scenes to return (default: 50)')
    .option('--offset <number>', 'Offset into filtered scenes (default: 0)')
    .action(async (opts: SceneListCommandOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleSceneList(opts, ctx.jsonOutput, {
            list: (query) => client.sceneList(query),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  scene
    .command('create <path>')
    .description('Create a new scene at a project-relative path')
    .action(async (path: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleSceneCreate(path, ctx.jsonOutput, {
            create: (scenePath) => client.sceneCreate(scenePath),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  scene
    .command('open <path>')
    .description('Open a scene by project-relative path')
    .action(async (path: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleSceneOpen(path, ctx.jsonOutput, {
            open: (scenePath) => client.sceneOpen(scenePath),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
