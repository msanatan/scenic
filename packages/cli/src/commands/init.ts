import type { Command } from 'commander'
import {
  createClient,
  init as sdkInit,
  readExecuteEnabled,
  writeExecuteEnabled,
  type InitOptions,
  type InitResult,
  type SettingsResult,
} from '@scenicai/sdk'
import { getGlobalOptions } from '../options.ts'
import { runWithOutput } from './output.ts'

interface InitCommandOptions {
  local?: string
  git?: string
  enableExecute?: boolean
  disableExecute?: boolean
}

interface LiveSettingsClient {
  settingsUpdate: (input: { executeEnabled: boolean }) => Promise<SettingsResult>
  close: () => void
}

interface InitHandlerDeps {
  init: (options?: InitOptions) => Promise<InitResult>
  createClient: (options: { projectPath: string; connectTimeout: number; enableExecute: boolean }) => LiveSettingsClient
  readExecuteEnabled: (projectPath: string) => boolean
  writeExecuteEnabled: (projectPath: string, enabled: boolean) => void
  console: Pick<Console, 'log' | 'error'>
}

type ApplyMode =
  | 'applied-live-and-persisted'
  | 'persisted-for-next-unity-startup'

type InitOutput = InitResult & {
  executeEnabled: boolean
  applyMode?: ApplyMode
  warning?: string
}

function isUnityUnavailableError(error: unknown): boolean {
  const message = error instanceof Error ? error.message : String(error)
  return /Connect timeout/i.test(message)
    || /ENOENT/i.test(message)
    || /socket hang up/i.test(message)
}

function isUnknownSettingsCommandError(error: unknown): boolean {
  const message = error instanceof Error ? error.message : String(error)
  return /Unknown command:\s*settings\.update/i.test(message)
}

async function applySettingWithFallback(
  projectPath: string,
  executeEnabled: boolean,
  deps: InitHandlerDeps,
): Promise<{ executeEnabled: boolean; applyMode: ApplyMode; warning?: string }> {
  const client = deps.createClient({
    projectPath,
    connectTimeout: 2_500,
    enableExecute: true,
  })

  try {
    const result = await client.settingsUpdate({ executeEnabled })
    return {
      executeEnabled: result.executeEnabled,
      applyMode: 'applied-live-and-persisted',
    }
  } catch (error) {
    if (isUnknownSettingsCommandError(error)) {
      deps.writeExecuteEnabled(projectPath, executeEnabled)
      return {
        executeEnabled,
        applyMode: 'persisted-for-next-unity-startup',
        warning: 'Connected plugin does not support settings.update; runtime apply is unavailable until Unity reloads.',
      }
    }

    if (isUnityUnavailableError(error)) {
      deps.writeExecuteEnabled(projectPath, executeEnabled)
      return {
        executeEnabled,
        applyMode: 'persisted-for-next-unity-startup',
      }
    }

    throw error
  } finally {
    client.close()
  }
}

export async function handleInit(
  opts: InitCommandOptions,
  jsonOutput: boolean,
  deps: InitHandlerDeps = {
    init: sdkInit,
    createClient: (options) => createClient(options),
    readExecuteEnabled,
    writeExecuteEnabled,
    console,
  },
): Promise<void> {
  if (opts.enableExecute && opts.disableExecute) {
    throw new Error('Pass only one of --enable-execute or --disable-execute.')
  }

  const source = opts.local
    ? { type: 'local' as const, path: opts.local }
    : opts.git
      ? { type: 'git' as const, url: opts.git }
      : undefined
  const requestedExecute =
    opts.enableExecute ? true
    : opts.disableExecute ? false
    : undefined

  await runWithOutput(
    jsonOutput,
    deps,
    async () => {
      const result = await deps.init({ source })

      if (typeof requestedExecute !== 'boolean') {
        return {
          ...result,
          executeEnabled: deps.readExecuteEnabled(result.projectPath),
        } as InitOutput
      }

      const applyResult = await applySettingWithFallback(result.projectPath, requestedExecute, deps)
      return {
        ...result,
        executeEnabled: applyResult.executeEnabled,
        applyMode: applyResult.applyMode,
        warning: applyResult.warning,
      } as InitOutput
    },
    (result, output) => {
      output.log(`Found Unity ${result.unityVersion} at ${result.projectPath}`)
      output.log(`Installed com.msanatan.scenic@${result.pluginVersion}`)
      output.log(`Execute: ${result.executeEnabled ? 'enabled' : 'disabled (pass --enable-execute to enable)'}`)

      if (result.applyMode === 'applied-live-and-persisted') {
        output.log('Settings: applied live + persisted')
      }
      if (result.applyMode === 'persisted-for-next-unity-startup') {
        output.log('Settings: persisted for next Unity startup')
        output.log('Open Unity to load or refresh the plugin.')
      }
      if (result.warning) {
        output.log(`Warning: ${result.warning}`)
      }
      if (!result.applyMode) {
        output.log('Open Unity to load the plugin.')
      }
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
          createClient: (options) => createClient(options),
          readExecuteEnabled,
          writeExecuteEnabled,
          console,
        },
      )
    })
}
