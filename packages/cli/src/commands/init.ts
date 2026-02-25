import type { Command } from 'commander'
import {
  init as sdkInit,
  readExecuteEnabled,
  writeExecuteEnabled,
  type InitOptions,
  type InitResult,
} from '@scenicai/sdk'
import { getGlobalOptions } from '../options.ts'
import { runWithOutput } from './output.ts'

interface InitCommandOptions {
  local?: string
  git?: string
  enableExecute?: boolean
  disableExecute?: boolean
}

interface InitHandlerDeps {
  init: (options?: InitOptions) => Promise<InitResult>
  console: Pick<Console, 'log' | 'error'>
}

type InitOutput = InitResult & {
  executeEnabled: boolean
}

export async function handleInit(
  opts: InitCommandOptions,
  jsonOutput: boolean,
  deps: InitHandlerDeps = { init: sdkInit, console },
): Promise<void> {
  if (opts.enableExecute && opts.disableExecute) {
    throw new Error('Pass only one of --enable-execute or --disable-execute.')
  }

  const source = opts.local
    ? { type: 'local' as const, path: opts.local }
    : opts.git
      ? { type: 'git' as const, url: opts.git }
      : undefined
  const enableExecute =
    opts.enableExecute ? true
    : opts.disableExecute ? false
    : undefined

  await runWithOutput(
    jsonOutput,
    deps,
    async () => {
      const result = await deps.init({ source, enableExecute })
      const executeEnabled = enableExecute ?? readExecuteEnabled(result.projectPath)
      writeExecuteEnabled(result.projectPath, executeEnabled)
      return { ...result, executeEnabled } as InitOutput
    },
    (result, output) => {
      output.log(`Found Unity ${result.unityVersion} at ${result.projectPath}`)
      output.log(`Installed com.msanatan.scenic@${result.pluginVersion}`)
      if (result.executeEnabled) {
        output.log('Execute: enabled')
      } else {
        output.log('Execute: disabled (pass --enable-execute to enable)')
      }
      output.log('Open Unity to load the plugin.')
    },
  )
}

export function registerInit(program: Command): void {
  program
    .command('init')
    .alias('update')
    .description('Install or update the scenic plugin into a Unity project (alias: update)')
    .option('--local <path>', 'Install from a local path')
    .option('--git <url>', 'Install from a custom git URL')
    .option('--enable-execute', 'Enable the execute command for this project')
    .option('--disable-execute', 'Disable the execute command for this project')
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
