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

export const ComponentsGetQuerySchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  componentInstanceId: v.optional(v.number()),
  index: v.optional(v.number()),
  type: v.optional(v.string()),
})

export const ComponentsRemoveInputSchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  componentInstanceId: v.optional(v.number()),
  index: v.optional(v.number()),
  type: v.optional(v.string()),
})

export const ComponentsUpdateInputSchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  componentInstanceId: v.optional(v.number()),
  index: v.optional(v.number()),
  type: v.optional(v.string()),
  values: v.record(v.string(), v.unknown()),
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

export const ComponentsGetResultSchema = v.object({
  component: v.object({
    instanceId: v.number(),
    type: v.string(),
    index: v.number(),
    enabled: v.optional(v.nullable(v.boolean())),
    serialized: v.record(v.string(), v.unknown()),
  }),
})

export const ComponentsRemoveResultSchema = v.object({
  removed: v.boolean(),
  instanceId: v.number(),
  type: v.string(),
  index: v.number(),
})

export const ComponentsUpdateResultSchema = v.object({
  instanceId: v.number(),
  type: v.string(),
  index: v.number(),
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

export const componentsGetCommand = defineCommand({
  method: 'componentsGet',
  wire: 'components.get',
  params: (query: ComponentsGetQuery) => ({
    path: query.path,
    instanceId: query.instanceId,
    componentInstanceId: query.componentInstanceId,
    index: query.index,
    type: query.type,
  }),
  result: ComponentsGetResultSchema,
})

export const componentsRemoveCommand = defineCommand({
  method: 'componentsRemove',
  wire: 'components.remove',
  params: (input: ComponentsRemoveInput) => ({
    path: input.path,
    instanceId: input.instanceId,
    componentInstanceId: input.componentInstanceId,
    index: input.index,
    type: input.type,
  }),
  result: ComponentsRemoveResultSchema,
})

export const componentsUpdateCommand = defineCommand({
  method: 'componentsUpdate',
  wire: 'components.update',
  params: (input: ComponentsUpdateInput) => ({
    path: input.path,
    instanceId: input.instanceId,
    componentInstanceId: input.componentInstanceId,
    index: input.index,
    type: input.type,
    values: input.values,
    strict: input.strict,
  }),
  result: ComponentsUpdateResultSchema,
})

export type ComponentsListQuery = v.InferOutput<typeof ComponentsListQuerySchema>
export type ComponentsAddInput = v.InferOutput<typeof ComponentsAddInputSchema>
export type ComponentsGetQuery = v.InferOutput<typeof ComponentsGetQuerySchema>
export type ComponentsRemoveInput = v.InferOutput<typeof ComponentsRemoveInputSchema>
export type ComponentsUpdateInput = v.InferOutput<typeof ComponentsUpdateInputSchema>
export type ComponentListItem = v.InferOutput<typeof ComponentListItemSchema>
export type ComponentsListResult = InferResult<typeof componentsListCommand>
export type ComponentsAddResult = InferResult<typeof componentsAddCommand>
export type ComponentsGetResult = InferResult<typeof componentsGetCommand>
export type ComponentsRemoveResult = InferResult<typeof componentsRemoveCommand>
export type ComponentsUpdateResult = InferResult<typeof componentsUpdateCommand>
