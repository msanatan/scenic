import type { ExecuteResult } from './contract.ts'
import type { CommandRuntime, ExecuteGuard } from '../runtime.ts'

export async function runExecute(
  runtime: CommandRuntime & ExecuteGuard,
  code: string,
): Promise<ExecuteResult> {
  runtime.ensureExecuteEnabled()
  return runtime.send('execute', { code })
}
