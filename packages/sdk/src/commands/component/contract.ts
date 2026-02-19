import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const ComponentsListQuerySchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  type: v.optional(v.string()),
  limit: v.optional(v.number()),
  offset: v.optional(v.number()),
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

export type ComponentsListQuery = v.InferOutput<typeof ComponentsListQuerySchema>
export type ComponentListItem = v.InferOutput<typeof ComponentListItemSchema>
export type ComponentsListResult = InferResult<typeof componentsListCommand>
