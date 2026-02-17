import type { CommandRuntime } from '../runtime.ts'
import type { StatusResult } from './contract.ts'

export async function runStatus(runtime: CommandRuntime): Promise<StatusResult> {
  const result = await runtime.send('status', {})
  if (typeof result !== 'string') {
    throw new Error('Status command returned an invalid payload.')
  }

  return JSON.parse(result) as StatusResult
}
