import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const ComponentsListQuerySchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  type: v.optional(v.string()),
  limit: v.optional(v.number()),
  offset: v.optional(v.number()),
})

export const ComponentsAddInputSchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  type: v.string(),
  initialValues: v.optional(v.record(v.string(), v.unknown())),
  strict: v.optional(v.boolean()),
})

const ComponentListItemSchema = v.object({
  instanceId: v.number(),
  type: v.string(),
  index: v.number(),
  enabled: v.optional(v.nullable(v.boolean())),
})

export const ComponentsListResultSchema = v.object({
  components: v.array(ComponentListItemSchema),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export const ComponentsAddResultSchema = v.object({
  instanceId: v.number(),
  type: v.string(),
  appliedFields: v.array(v.string()),
  ignoredFields: v.array(v.string()),
})

export const componentsListCommand = defineCommand({
  method: 'componentsList',
  wire: 'components.list',
  params: (query: ComponentsListQuery) => ({
    path: query.path,
    instanceId: query.instanceId,
    type: query.type,
    limit: query.limit,
    offset: query.offset,
  }),
  result: ComponentsListResultSchema,
})

export const componentsAddCommand = defineCommand({
  method: 'componentsAdd',
  wire: 'components.add',
  params: (input: ComponentsAddInput) => ({
    path: input.path,
    instanceId: input.instanceId,
    type: input.type,
    initialValues: input.initialValues,
    strict: input.strict,
  }),
  result: ComponentsAddResultSchema,
})

export type ComponentsListQuery = v.InferOutput<typeof ComponentsListQuerySchema>
export type ComponentsAddInput = v.InferOutput<typeof ComponentsAddInputSchema>
export type ComponentListItem = v.InferOutput<typeof ComponentListItemSchema>
export type ComponentsListResult = InferResult<typeof componentsListCommand>
export type ComponentsAddResult = InferResult<typeof componentsAddCommand>
