import type { Command } from 'commander'
import { init as sdkInit, type InitOptions, type InitResult } from '@unibridge/sdk'
import { getGlobalOptions } from '../options.ts'
import { runWithOutput } from './output.ts'

interface InitCommandOptions {
  local?: string
  git?: string
}

interface InitHandlerDeps {
  init: (options?: InitOptions) => Promise<InitResult>
  console: Pick<Console, 'log' | 'error'>
}

export async function handleInit(
  opts: InitCommandOptions,
  jsonOutput: boolean,
  deps: InitHandlerDeps = { init: sdkInit, console },
): Promise<void> {
  const source = opts.local
    ? { type: 'local' as const, path: opts.local }
    : opts.git
      ? { type: 'git' as const, url: opts.git }
      : undefined

  await runWithOutput(
    jsonOutput,
    deps,
    () => deps.init({ source }),
    (result, output) => {
      output.log(`Found Unity ${result.unityVersion} at ${result.projectPath}`)
      output.log(`Installed com.msanatan.unibridge@${result.pluginVersion}`)
      output.log('Open Unity to load the plugin.')
    },
  )
}

export function registerInit(program: Command): void {
  program
    .command('init')
    .description('Install the unibridge plugin into a Unity project')
    .option('--local <path>', 'Install from a local path')
    .option('--git <url>', 'Install from a custom git URL')
    .action(async (opts: InitCommandOptions, command: Command) => {
      const globalOpts = getGlobalOptions(command)
      await handleInit(
        opts,
        globalOpts.json === true,
        {
          init: (initOpts) => sdkInit({ ...initOpts, projectPath: globalOpts.project }),
          console,
        },
      )
    })
}
