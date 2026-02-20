import type { Command } from 'commander'
import type { TagItem, TagsAddInput, TagsAddResult, TagsGetResult, TagsRemoveInput, TagsRemoveResult } from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface TagsGetDeps {
  get: () => Promise<TagsGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface TagsAddDeps {
  add: (input: TagsAddInput) => Promise<TagsAddResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface TagsRemoveDeps {
  remove: (input: TagsRemoveInput) => Promise<TagsRemoveResult>
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

export async function handleTagsAdd(
  name: string,
  jsonOutput: boolean,
  deps: TagsAddDeps,
): Promise<void> {
  if (name.trim().length === 0) {
    throw new Error('Tag name is required.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.add({ name: name.trim() }),
    (result, output) => {
      output.log(`Tag:   ${formatTag(result.tag)}`)
      output.log(`Added: ${result.added ? 'yes' : 'no'}`)
      output.log(`Total: ${result.total}`)
    },
  )
}

export async function handleTagsRemove(
  name: string,
  jsonOutput: boolean,
  deps: TagsRemoveDeps,
): Promise<void> {
  if (name.trim().length === 0) {
    throw new Error('Tag name is required.')
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.remove({ name: name.trim() }),
    (result, output) => {
      output.log(`Tag:     ${formatTag(result.tag)}`)
      output.log(`Removed: ${result.removed ? 'yes' : 'no'}`)
      output.log(`Total:   ${result.total}`)
    },
  )
}

export function registerTags(program: Command): void {
  const tags = program
    .command('tags')
    .description('Inspect and mutate Unity project tags')

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

  tags
    .command('add <name>')
    .description('Add a project tag (idempotent)')
    .action(async (name: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleTagsAdd(name, ctx.jsonOutput, {
            add: (input) => client.tagsAdd(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  tags
    .command('remove <name>')
    .description('Remove a project tag (idempotent; built-in tags are protected)')
    .action(async (name: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleTagsRemove(name, ctx.jsonOutput, {
            remove: (input) => client.tagsRemove(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
