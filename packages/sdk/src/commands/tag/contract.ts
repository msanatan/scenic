import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const TagItemSchema = v.object({
  name: v.string(),
  isBuiltIn: v.boolean(),
})

export const TagsGetResultSchema = v.object({
  tags: v.array(TagItemSchema),
  total: v.number(),
})

export const tagsGetCommand = defineCommand({
  method: 'tagsGet',
  wire: 'tags.get',
  params: () => ({}),
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

export type TagItem = v.InferOutput<typeof TagItemSchema>
export type TagsGetResult = InferResult<typeof tagsGetCommand>
export type TagsAddInput = v.InferOutput<typeof TagsAddInputSchema>
export type TagsAddResult = InferResult<typeof tagsAddCommand>
