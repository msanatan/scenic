import type { Command } from 'commander'
import type { SettingsResult } from '@scenicai/sdk'
import { runWithOutput } from './output.ts'
import { withUnityClient } from './with-unity-client.ts'

interface SettingsGetDeps {
  get: () => Promise<SettingsResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

interface SettingsUpdateDeps {
  update: (input: { executeEnabled: boolean }) => Promise<SettingsResult>
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

function parseBooleanOption(value: string): boolean {
  if (value === 'true') {
    return true
  }
  if (value === 'false') {
    return false
  }
  throw new Error('Expected --execute-enabled to be true or false.')
}

async function handleSettingsGet(jsonOutput: boolean, deps: SettingsGetDeps): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.get(),
    (result, output) => {
      output.log(`Execute: ${result.executeEnabled ? 'enabled' : 'disabled'}`)
    },
  )
}

async function handleSettingsUpdate(
  executeEnabled: boolean,
  jsonOutput: boolean,
  deps: SettingsUpdateDeps,
): Promise<void> {
  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.update({ executeEnabled }),
    (result, output) => {
      output.log(`Execute: ${result.executeEnabled ? 'enabled' : 'disabled'}`)
    },
  )
}

export function registerSettings(program: Command): void {
  const settings = program
    .command('settings')
    .description('Get or update plugin settings')

  settings
    .command('get')
    .description('Read current plugin settings from Unity runtime')
    .action(async (_opts: Record<string, never>, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleSettingsGet(ctx.jsonOutput, {
            get: () => client.settingsGet(),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })

  settings
    .command('update')
    .description('Update plugin settings in runtime and persisted config')
    .requiredOption(
      '--execute-enabled <true|false>',
      'Set executeEnabled in plugin settings',
      parseBooleanOption,
    )
    .action(async (opts: { executeEnabled: boolean }, command: Command) => {
      await withUnityClient(
        command,
        { requirePlugin: true },
        async (client, ctx) => {
          await handleSettingsUpdate(opts.executeEnabled, ctx.jsonOutput, {
            update: (input) => client.settingsUpdate(input),
            console,
            exit: (exitCode) => {
              process.exitCode = exitCode
            },
          })
        },
      )
    })
}
