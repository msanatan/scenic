import type { CommandRuntime } from '../runtime.ts'
import type { StatusResult } from './contract.ts'

export async function runStatus(runtime: CommandRuntime): Promise<StatusResult> {
  const result = await runtime.send('status', {})
  return result as StatusResult
}
