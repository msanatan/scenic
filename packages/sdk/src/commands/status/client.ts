import * as v from 'valibot'
import type { CommandRuntime } from '../runtime.ts'
import { StatusResultSchema, type StatusResult } from './contract.ts'

export async function runStatus(runtime: CommandRuntime): Promise<StatusResult> {
  const result = await runtime.send('status', {})
  return v.parse(StatusResultSchema, result)
}
