import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const LogsSeveritySchema = v.picklist(['info', 'warn', 'error'])

export const LogsQuerySchema = v.object({
  severity: v.optional(LogsSeveritySchema),
  limit: v.optional(v.number()),
  offset: v.optional(v.number()),
})

const LogEntrySchema = v.object({
  timestamp: v.string(),
  severity: LogsSeveritySchema,
  message: v.string(),
  stackTrace: v.optional(v.string()),
})

export const LogsResultSchema = v.object({
  logs: v.array(LogEntrySchema),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export const logsCommand = defineCommand({
  method: 'logs',
  wire: 'logs',
  params: (query?: LogsQuery) => ({
    severity: query?.severity,
    limit: query?.limit,
    offset: query?.offset,
  }),
  result: LogsResultSchema,
})

export type LogsSeverity = v.InferOutput<typeof LogsSeveritySchema>
export type LogsQuery = v.InferOutput<typeof LogsQuerySchema>
export type LogEntry = v.InferOutput<typeof LogEntrySchema>
export type LogsResult = InferResult<typeof logsCommand>
