import type { Command } from 'commander'
import type { TagItem, TagsGetResult } from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface TagsGetDeps {
  get: () => Promise<TagsGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function formatTag(tag: TagItem): string {
  return `${tag.name}${tag.isBuiltIn ? ' (built-in)' : ''}`
}

export async function handleTagsGet(
  jsonOutput: boolean,
  deps: TagsGetDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.get(),
    (result, output) => {
      output.log(`Tags: ${result.total}`)
      for (const tag of result.tags) {
        output.log(formatTag(tag))
      }
    },
  )
}

export function registerTags(program: Command): void {
  const tags = program
    .command('tags')
    .description('Inspect Unity project tags')

  tags
    .command('get')
    .description('List all tags, including built-in tags')
    .action(async (_opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleTagsGet(ctx.jsonOutput, {
            get: () => client.tagsGet(),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
