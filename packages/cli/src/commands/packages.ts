import type { Command } from 'commander'
import type {
  PackageItem,
  PackagesAddInput,
  PackagesAddResult,
  PackagesGetQuery,
  PackagesGetResult,
  PackagesRemoveInput,
  PackagesRemoveResult,
} from '@scenicai/sdk/commands/package'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'
import { parseIntWithMinimum } from './parse.ts'

interface PackagesGetOptions {
  limit?: string
  offset?: string
  includeIndirect?: boolean
  search?: string
}

interface PackagesGetDeps {
  get: (query?: PackagesGetQuery) => Promise<PackagesGetResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface PackagesAddOptions {
  version?: string
}

interface PackagesAddDeps {
  add: (input: PackagesAddInput) => Promise<PackagesAddResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface PackagesRemoveDeps {
  remove: (input: PackagesRemoveInput) => Promise<PackagesRemoveResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function formatPackage(pkg: PackageItem): string {
  return `${pkg.name}@${pkg.version} [display=${pkg.displayName}, source=${pkg.source}, direct=${pkg.isDirectDependency ? 'yes' : 'no'}]`
}

export async function handlePackagesGet(
  opts: PackagesGetOptions,
  jsonOutput: boolean,
  deps: PackagesGetDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      const search = opts.search?.trim()
      const query: PackagesGetQuery = {
        limit: parseIntWithMinimum(opts.limit, '--limit', 50, 1),
        offset: parseIntWithMinimum(opts.offset, '--offset', 0, 0),
        includeIndirect: opts.includeIndirect ?? false,
        search: search == null || search.length === 0 ? undefined : search,
      }
      return deps.get(query)
    },
    (result, output) => {
      output.log(`Packages: ${result.packages.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const pkg of result.packages) {
        output.log(formatPackage(pkg))
      }
    },
  )
}

export async function handlePackagesAdd(
  name: string,
  opts: PackagesAddOptions,
  jsonOutput: boolean,
  deps: PackagesAddDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      const trimmedName = name.trim()
      if (trimmedName.length === 0) {
        throw new Error('Package name is required.')
      }
      const version = opts.version?.trim()
      return deps.add({
        name: trimmedName,
        version: version == null || version.length === 0 ? undefined : version,
      })
    },
    (result, output) => {
      output.log(`Package: ${formatPackage(result.package)}`)
      output.log(`Added: ${result.added ? 'yes' : 'no'}`)
      output.log(`Total: ${result.total}`)
    },
  )
}

export async function handlePackagesRemove(
  name: string,
  jsonOutput: boolean,
  deps: PackagesRemoveDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => {
      const trimmedName = name.trim()
      if (trimmedName.length === 0) {
        throw new Error('Package name is required.')
      }
      return deps.remove({ name: trimmedName })
    },
    (result, output) => {
      output.log(`Package: ${formatPackage(result.package)}`)
      output.log(`Removed: ${result.removed ? 'yes' : 'no'}`)
      output.log(`Total: ${result.total}`)
    },
  )
}

export function registerPackages(program: Command): void {
  const packages = program
    .command('packages')
    .description('Inspect Unity Package Manager packages')

  packages
    .command('get')
    .description('List installed packages with pagination and optional filtering')
    .option('--limit <number>', 'Number of packages to return (default: 50)')
    .option('--offset <number>', 'Offset into package list (default: 0)')
    .option('--include-indirect', 'Include indirect/transitive dependencies')
    .option('--search <query>', 'Case-insensitive contains filter on name or display name')
    .action(async (opts: PackagesGetOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handlePackagesGet(opts, ctx.jsonOutput, {
            get: (query) => client.packagesGet(query),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  packages
    .command('add <name>')
    .description('Add a Unity package by name (idempotent); optionally pin a version')
    .option('--version <version>', 'Specific package version to add')
    .action(async (name: string, opts: PackagesAddOptions, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handlePackagesAdd(name, opts, ctx.jsonOutput, {
            add: (input) => client.packagesAdd(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  packages
    .command('remove <name>')
    .description('Remove a direct Unity package dependency by name (idempotent)')
    .action(async (name: string, _opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handlePackagesRemove(name, ctx.jsonOutput, {
            remove: (input) => client.packagesRemove(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
