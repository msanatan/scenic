import type { Command } from 'commander'
import type { PackageItem, PackagesGetQuery, PackagesGetResult } from '@scenicai/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

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

function formatPackage(pkg: PackageItem): string {
  return `${pkg.name}@${pkg.version} [display=${pkg.displayName}, source=${pkg.source}, direct=${pkg.isDirectDependency ? 'yes' : 'no'}]`
}

export async function handlePackagesGet(
  opts: PackagesGetOptions,
  jsonOutput: boolean,
  deps: PackagesGetDeps,
): Promise<void> {
  const search = opts.search?.trim()
  const query: PackagesGetQuery = {
    limit: parseIntWithMinimum(opts.limit, '--limit', 50, 1),
    offset: parseIntWithMinimum(opts.offset, '--offset', 0, 0),
    includeIndirect: opts.includeIndirect ?? false,
    search: search == null || search.length === 0 ? undefined : search,
  }

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.get(query),
    (result, output) => {
      output.log(`Packages: ${result.packages.length} of ${result.total} (limit ${result.limit}, offset ${result.offset})`)
      for (const pkg of result.packages) {
        output.log(formatPackage(pkg))
      }
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
}
