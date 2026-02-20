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

export type TagItem = v.InferOutput<typeof TagItemSchema>
export type TagsGetResult = InferResult<typeof tagsGetCommand>
