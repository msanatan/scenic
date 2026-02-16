export interface InitOptions {
  projectPath?: string
  source?: GitSource | LocalSource
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
  capabilities?: {
    executeEnabled?: boolean
  }
  projectPath?: string
}

export interface ExecuteParams {
  code: string
}

export interface TimeoutOptions {
  connectTimeout?: number
  commandTimeout?: number
  reconnectTimeout?: number
}

export interface ExecuteOptions {
  timeout?: number
}

export interface ClientOptions extends TimeoutOptions {
  projectPath?: string
  enableExecute?: boolean
}

export interface UniBridgeClient {
  readonly projectPath: string
  execute(code: string, options?: ExecuteOptions): Promise<unknown>
  close(): void
}
