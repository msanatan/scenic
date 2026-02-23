import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const ScriptableObjectCreateInputSchema = v.object({
  assetPath: v.string(),
  type: v.string(),
  initialValues: v.optional(v.record(v.string(), v.unknown())),
  strict: v.optional(v.boolean()),
})

export const ScriptableObjectGetInputSchema = v.object({
  assetPath: v.string(),
})

export const ScriptableObjectUpdateInputSchema = v.object({
  assetPath: v.string(),
  values: v.record(v.string(), v.unknown()),
  strict: v.optional(v.boolean()),
})

const ScriptableObjectSummarySchema = v.object({
  assetPath: v.string(),
  name: v.string(),
  type: v.string(),
  instanceId: v.number(),
})

export const ScriptableObjectCreateResultSchema = v.object({
  asset: ScriptableObjectSummarySchema,
  appliedFields: v.array(v.string()),
  ignoredFields: v.array(v.string()),
})

export const ScriptableObjectGetResultSchema = v.object({
  asset: ScriptableObjectSummarySchema,
  serialized: v.record(v.string(), v.unknown()),
})

export const ScriptableObjectUpdateResultSchema = v.object({
  asset: ScriptableObjectSummarySchema,
  appliedFields: v.array(v.string()),
  ignoredFields: v.array(v.string()),
})

export const scriptableObjectCreateCommand = defineCommand({
  method: 'scriptableObjectCreate',
  wire: 'scriptableobject.create',
  params: (input: ScriptableObjectCreateInput) => ({
    assetPath: input.assetPath,
    type: input.type,
    initialValues: input.initialValues,
    strict: input.strict,
  }),
  result: ScriptableObjectCreateResultSchema,
})

export const scriptableObjectGetCommand = defineCommand({
  method: 'scriptableObjectGet',
  wire: 'scriptableobject.get',
  params: (input: ScriptableObjectGetInput) => ({
    assetPath: input.assetPath,
  }),
  result: ScriptableObjectGetResultSchema,
})

export const scriptableObjectUpdateCommand = defineCommand({
  method: 'scriptableObjectUpdate',
  wire: 'scriptableobject.update',
  params: (input: ScriptableObjectUpdateInput) => ({
    assetPath: input.assetPath,
    values: input.values,
    strict: input.strict,
  }),
  result: ScriptableObjectUpdateResultSchema,
})

export type ScriptableObjectCreateInput = v.InferOutput<typeof ScriptableObjectCreateInputSchema>
export type ScriptableObjectGetInput = v.InferOutput<typeof ScriptableObjectGetInputSchema>
export type ScriptableObjectUpdateInput = v.InferOutput<typeof ScriptableObjectUpdateInputSchema>
export type ScriptableObjectSummary = v.InferOutput<typeof ScriptableObjectSummarySchema>
export type ScriptableObjectCreateResult = InferResult<typeof scriptableObjectCreateCommand>
export type ScriptableObjectGetResult = InferResult<typeof scriptableObjectGetCommand>
export type ScriptableObjectUpdateResult = InferResult<typeof scriptableObjectUpdateCommand>
