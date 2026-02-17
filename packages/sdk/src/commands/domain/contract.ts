import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const DomainReloadResultSchema = v.object({
  triggered: v.boolean(),
})

export const domainReloadCommand = defineCommand({
  method: 'domainReload',
  wire: 'domain.reload',
  params: () => ({}),
  result: DomainReloadResultSchema,
})

export type DomainReloadResult = InferResult<typeof domainReloadCommand>
