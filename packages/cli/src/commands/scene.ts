import type { Command } from 'commander'
import type { SceneActiveResult, SceneCreateResult, SceneOpenResult } from '@unibridge/sdk'
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
