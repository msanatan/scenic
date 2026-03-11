import type { Command } from 'commander'
import type {
  TagItem,
  TagsAddInput,
  TagsAddResult,
  TagsGetQuery,
  TagsGetResult,
  TagsRemoveInput,
  TagsRemoveResult,
} from '@scenicai/sdk/commands/tag'
import { runWithOutput } from './output.ts'
import { parseIntWithMinimum, parseName } from './parse.ts'
import { withUnityClient } from './with-unity-client.ts'

interface TagsGetOptions {
  limit?: string
  offset?: string
}

interface TagsGetDeps {
  get: (query?: TagsGetQuery) => Promise<TagsGetResult>
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
  opts: TagsGetOptions,
  jsonOutput: boolean,
  deps: TagsGetDeps,
): Promise<void> {
  const query: TagsGetQuery = {
    limit: parseIntWithMinimum(opts.limit, '--limit', 50, 1),
    offset: parseIntWithMinimum(opts.offset, '--offset', 0, 0),
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.get(query),
    (result, output) => {
      output.log(`Tags: ${result.tags.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
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
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.add({ name: parseName(name, 'Tag name') }),
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
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.remove({ name: parseName(name, 'Tag name') }),
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
    .description('List tags with pagination, including built-in tags')
    .option('--limit <number>', 'Number of tags to return (default: 50)')
    .option('--offset <number>', 'Offset into tags (default: 0)')
    .action(async (opts: TagsGetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleTagsGet(opts, ctx.jsonOutput, {
            get: (query) => client.tagsGet(query),
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
