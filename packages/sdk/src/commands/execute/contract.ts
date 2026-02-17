import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const executeCommand = defineCommand({
  method: 'execute',
  wire: 'execute',
  params: (code: string) => ({ code }),
  result: v.unknown(),
  guard: 'execute',
})

export type ExecuteResult = InferResult<typeof executeCommand>
