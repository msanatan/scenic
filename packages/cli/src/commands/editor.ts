import type { Command } from 'commander'
import type { EditorStateResult } from '@scenicai/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface EditorActionDeps {
  run: () => Promise<EditorStateResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

async function handleEditorAction(
  jsonOutput: boolean,
  deps: EditorActionDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.run(),
    (result, output) => {
      output.log(`Play mode: ${result.playMode}`)
    },
  )
}

export function registerEditor(program: Command): void {
  const editor = program
    .command('editor')
    .description('Control Unity editor play mode state')

  editor
    .command('play')
    .description('Start play mode')
    .action(async (_opts: unknown, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleEditorAction(ctx.jsonOutput, {
            run: () => client.editorPlay(),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  editor
    .command('pause')
    .description('Pause play mode')
    .action(async (_opts: unknown, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleEditorAction(ctx.jsonOutput, {
            run: () => client.editorPause(),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  editor
    .command('stop')
    .description('Stop play mode')
    .action(async (_opts: unknown, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleEditorAction(ctx.jsonOutput, {
            run: () => client.editorStop(),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
