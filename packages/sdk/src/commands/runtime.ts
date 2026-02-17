export interface CommandRuntime {
  send(command: string, params: Record<string, unknown>): Promise<unknown>
}

export interface ExecuteGuard {
  ensureExecuteEnabled(): void
}
