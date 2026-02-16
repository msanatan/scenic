import type { Command } from 'commander'
import { init as sdkInit, type InitOptions, type InitResult } from '@unibridge/sdk'
import { getGlobalOptions } from '../options.ts'

interface InitCommandOptions {
  local?: string
  git?: string
}

interface InitHandlerDeps {
  init: (options?: InitOptions) => Promise<InitResult>
  console: Pick<Console, 'log'>
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

  const result = await deps.init({
    source,
  })

  if (jsonOutput) {
    deps.console.log(JSON.stringify({ success: true, result }))
  } else {
    deps.console.log(`Found Unity ${result.unityVersion} at ${result.projectPath}`)
    deps.console.log(`Installed com.msanatan.unibridge@${result.pluginVersion}`)
    deps.console.log('Open Unity to load the plugin.')
  }
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
