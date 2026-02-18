import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const TestModeSchema = v.picklist(['edit', 'play'])
export const TestStatusSchema = v.picklist(['passed', 'failed', 'skipped', 'inconclusive'])

export const TestListQuerySchema = v.object({
  mode: v.optional(TestModeSchema),
  filter: v.optional(v.string()),
  limit: v.optional(v.number()),
  offset: v.optional(v.number()),
})

export const TestRunQuerySchema = v.object({
  mode: v.optional(TestModeSchema),
  filter: v.optional(v.string()),
  limit: v.optional(v.number()),
  offset: v.optional(v.number()),
})

const TestListItemSchema = v.object({
  name: v.string(),
  fullName: v.string(),
  mode: TestModeSchema,
  assembly: v.string(),
})

const TestRunItemSchema = v.object({
  name: v.string(),
  fullName: v.string(),
  mode: TestModeSchema,
  status: TestStatusSchema,
  durationMs: v.number(),
  message: v.optional(v.nullable(v.string())),
  stackTrace: v.optional(v.nullable(v.string())),
})

export const TestListResultSchema = v.object({
  tests: v.array(TestListItemSchema),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export const TestRunResultSchema = v.object({
  tests: v.array(TestRunItemSchema),
  passed: v.number(),
  failed: v.number(),
  skipped: v.number(),
  inconclusive: v.number(),
  durationMs: v.number(),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export const testListCommand = defineCommand({
  method: 'testList',
  wire: 'test.list',
  params: (query?: TestListQuery) => ({
    mode: query?.mode,
    filter: query?.filter,
    limit: query?.limit,
    offset: query?.offset,
  }),
  result: TestListResultSchema,
})

export const testRunCommand = defineCommand({
  method: 'testRun',
  wire: 'test.run',
  params: (query?: TestRunQuery) => ({
    mode: query?.mode,
    filter: query?.filter,
    limit: query?.limit,
    offset: query?.offset,
  }),
  result: TestRunResultSchema,
})

export type TestMode = v.InferOutput<typeof TestModeSchema>
export type TestStatus = v.InferOutput<typeof TestStatusSchema>
export type TestListQuery = v.InferOutput<typeof TestListQuerySchema>
export type TestRunQuery = v.InferOutput<typeof TestRunQuerySchema>
export type TestListItem = v.InferOutput<typeof TestListItemSchema>
export type TestRunItem = v.InferOutput<typeof TestRunItemSchema>
export type TestListResult = InferResult<typeof testListCommand>
export type TestRunResult = InferResult<typeof testRunCommand>
