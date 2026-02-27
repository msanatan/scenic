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

export const MaterialAssignInputSchema = v.object({
  path: v.optional(v.string()),
  instanceId: v.optional(v.number()),
  assetPath: v.string(),
  rendererIndex: v.optional(v.number()),
  slot: v.optional(v.number()),
})

export const MaterialAssignResultSchema = v.object({
  targetPath: v.string(),
  targetInstanceId: v.number(),
  rendererType: v.string(),
  rendererIndex: v.number(),
  rendererInstanceId: v.number(),
  slot: v.number(),
  material: MaterialSummarySchema,
})

export const materialAssignCommand = defineCommand({
  method: 'materialAssign',
  wire: 'material.assign',
  params: (input: MaterialAssignInput) => ({
    path: input.path,
    instanceId: input.instanceId,
    assetPath: input.assetPath,
    rendererIndex: input.rendererIndex,
    slot: input.slot,
  }),
  result: MaterialAssignResultSchema,
  guard: 'execute',
})

export type MaterialAssignInput = v.InferOutput<typeof MaterialAssignInputSchema>
export type MaterialAssignResult = InferResult<typeof materialAssignCommand>
// --- end ---
