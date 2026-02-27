import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const MaterialCreateInputSchema = v.object({
  assetPath: v.string(),
  shader: v.optional(v.string()),
})

const MaterialSummarySchema = v.object({
  assetPath: v.string(),
  name: v.string(),
  shader: v.string(),
  instanceId: v.number(),
})

export const MaterialCreateResultSchema = v.object({
  material: MaterialSummarySchema,
})

export const materialCreateCommand = defineCommand({
  method: 'materialCreate',
  wire: 'material.create',
  params: (input: MaterialCreateInput) => ({
    assetPath: input.assetPath,
    shader: input.shader,
  }),
  result: MaterialCreateResultSchema,
  guard: 'execute',
})

export type MaterialCreateInput = v.InferOutput<typeof MaterialCreateInputSchema>
export type MaterialCreateResult = InferResult<typeof materialCreateCommand>

export const MaterialGetInputSchema = v.object({
  assetPath: v.string(),
})

export const MaterialGetResultSchema = v.object({
  material: MaterialSummarySchema,
})

export const materialGetCommand = defineCommand({
  method: 'materialGet',
  wire: 'material.get',
  params: (input: MaterialGetInput) => ({
    assetPath: input.assetPath,
  }),
  result: MaterialGetResultSchema,
})

export type MaterialGetInput = v.InferOutput<typeof MaterialGetInputSchema>
export type MaterialGetResult = InferResult<typeof materialGetCommand>
export type MaterialSummary = v.InferOutput<typeof MaterialSummarySchema>
// --- end ---
