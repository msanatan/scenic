import type { Command } from 'commander'
import { init as sdkInit, type InitOptions, type InitResult } from '@unibridge/sdk'

interface InitCommandOptions {
  project?: string
  local?: string
  git?: string
}

interface InitHandlerDeps {
  init: (options?: InitOptions) => Promise<InitResult>
  console: Pick<Console, 'log'>
}

export async function handleInit(
  opts: InitCommandOptions,
  deps: InitHandlerDeps = { init: sdkInit, console },
): Promise<void> {
  const source = opts.local
    ? { type: 'local' as const, path: opts.local }
    : opts.git
      ? { type: 'git' as const, url: opts.git }
      : undefined

  const result = await deps.init({
    projectPath: opts.project,
    source,
  })

  deps.console.log(`Found Unity ${result.unityVersion} at ${result.projectPath}`)
  deps.console.log(`Installed com.msanatan.unibridge@${result.pluginVersion}`)
  deps.console.log('Open Unity to load the plugin.')
}

export function registerInit(program: Command): void {
  program
    .command('init')
    .description('Install the unibridge plugin into a Unity project')
    .option('-p, --project <path>', 'Path to Unity project (default: auto-detect)')
    .option('--local <path>', 'Install from a local path')
    .option('--git <url>', 'Install from a custom git URL')
    .action(async (opts: InitCommandOptions) => {
      await handleInit(opts)
    })
}
