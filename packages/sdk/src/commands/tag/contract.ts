import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const TagItemSchema = v.object({
  name: v.string(),
  isBuiltIn: v.boolean(),
})

export const TagsGetResultSchema = v.object({
  tags: v.array(TagItemSchema),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export interface TagsGetQuery {
  limit?: number
  offset?: number
}

export const tagsGetCommand = defineCommand({
  method: 'tagsGet',
  wire: 'tags.get',
  params: (query?: TagsGetQuery) => ({
    limit: query?.limit,
    offset: query?.offset,
  }),
  result: TagsGetResultSchema,
})

export const TagsAddInputSchema = v.object({
  name: v.string(),
})

export const TagsAddResultSchema = v.object({
  tag: TagItemSchema,
  added: v.boolean(),
  total: v.number(),
})

export const tagsAddCommand = defineCommand({
  method: 'tagsAdd',
  wire: 'tags.add',
  params: (input: TagsAddInput) => ({
    name: input.name,
  }),
  result: TagsAddResultSchema,
})

export const TagsRemoveInputSchema = v.object({
  name: v.string(),
})

export const TagsRemoveResultSchema = v.object({
  tag: TagItemSchema,
  removed: v.boolean(),
  total: v.number(),
})

export const tagsRemoveCommand = defineCommand({
  method: 'tagsRemove',
  wire: 'tags.remove',
  params: (input: TagsRemoveInput) => ({
    name: input.name,
  }),
  result: TagsRemoveResultSchema,
})

export type TagItem = v.InferOutput<typeof TagItemSchema>
export type TagsGetResult = InferResult<typeof tagsGetCommand>
export type TagsAddInput = v.InferOutput<typeof TagsAddInputSchema>
export type TagsAddResult = InferResult<typeof tagsAddCommand>
export type TagsRemoveInput = v.InferOutput<typeof TagsRemoveInputSchema>
export type TagsRemoveResult = InferResult<typeof tagsRemoveCommand>
