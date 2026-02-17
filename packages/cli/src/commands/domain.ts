import type { Command } from 'commander'
import type { DomainReloadResult } from '@unibridge/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface DomainReloadDeps {
  reload: () => Promise<DomainReloadResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function handleDomainReload(
  jsonOutput: boolean,
  deps: DomainReloadDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.reload(),
    (_result, output) => {
      output.log('Domain reload triggered.')
    },
  )
}

export function registerDomain(program: Command): void {
  const domain = program
    .command('domain')
    .description('Unity domain operations')

  domain
    .command('reload')
    .description('Trigger an asset refresh and domain reload')
    .action(async (_opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleDomainReload(ctx.jsonOutput, {
            reload: () => client.domainReload(),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
