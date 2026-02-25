import type { CommandMethods } from './commands/define.ts'
import type { allCommands } from './commands/registry.ts'

export interface InitOptions {
  projectPath?: string
  source?: GitSource | LocalSource
  enableExecute?: boolean
}

export interface GitSource {
  type: 'git'
  url: string
}

export interface LocalSource {
  type: 'local'
  path: string
}

export interface InitResult {
  projectPath: string
  unityVersion: string
  pluginVersion: string
  pluginSource: 'git' | 'local'
  executeEnabled: boolean
}

export interface CommandRequest {
  id: string
  command: string
  params: Record<string, unknown>
}

export interface CommandResponse {
  id: string
  success: boolean
  result?: unknown
  error?: string
}

export interface ServerMetadata {
  pid: number
  unityVersion: string
  pluginVersion: string
  protocolVersion: number
  projectPath?: string
}

export interface TimeoutOptions {
  connectTimeout?: number
  commandTimeout?: number
}

export interface ClientOptions extends TimeoutOptions {
  projectPath?: string
  enableExecute?: boolean
}

export type ScenicClient = CommandMethods<typeof allCommands> & {
  readonly projectPath: string
  close(): void
}
