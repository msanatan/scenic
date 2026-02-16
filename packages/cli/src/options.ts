import type { Command } from 'commander'

export interface GlobalCommandOptions {
  project?: string
  json?: boolean
  execute?: boolean
}

export function getGlobalOptions(command: Command): GlobalCommandOptions {
  return command.optsWithGlobals() as GlobalCommandOptions
}
