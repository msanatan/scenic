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

})

export type MaterialAssignInput = v.InferOutput<typeof MaterialAssignInputSchema>
export type MaterialAssignResult = InferResult<typeof materialAssignCommand>

const MaterialPropertyTypeSchema = v.picklist(['float', 'range', 'int', 'color', 'vector', 'texture'])

const MaterialColorSchema = v.object({
  r: v.number(),
  g: v.number(),
  b: v.number(),
  a: v.number(),
})

const MaterialVectorSchema = v.object({
  x: v.number(),
  y: v.number(),
  z: v.number(),
  w: v.number(),
})

const MaterialPropertyGetValueSchema = v.object({
  type: MaterialPropertyTypeSchema,
  value: v.optional(v.union([v.number(), MaterialColorSchema, MaterialVectorSchema])),
  assetPath: v.optional(v.nullable(v.string())),
  textureType: v.optional(v.nullable(v.string())),
})

const MaterialPropertySetValueSchema = v.object({
  type: MaterialPropertyTypeSchema,
  value: v.optional(v.union([v.number(), MaterialColorSchema, MaterialVectorSchema])),
  assetPath: v.optional(v.nullable(v.string())),
})

export const MaterialPropertiesGetInputSchema = v.object({
  assetPath: v.string(),
  names: v.optional(v.array(v.string())),
})

export const MaterialPropertiesGetResultSchema = v.object({
  material: MaterialSummarySchema,
  properties: v.record(v.string(), MaterialPropertyGetValueSchema),
})

export const materialPropertiesGetCommand = defineCommand({
  method: 'materialPropertiesGet',
  wire: 'material.properties.get',
  params: (input: MaterialPropertiesGetInput) => ({
    assetPath: input.assetPath,
    names: input.names,
  }),
  result: MaterialPropertiesGetResultSchema,
})

export const MaterialPropertiesSetInputSchema = v.object({
  assetPath: v.string(),
  values: v.record(v.string(), MaterialPropertySetValueSchema),
  strict: v.optional(v.boolean()),
})

export const MaterialPropertiesSetResultSchema = v.object({
  material: MaterialSummarySchema,
  appliedProperties: v.array(v.string()),
  ignoredProperties: v.array(v.string()),
})

export const materialPropertiesSetCommand = defineCommand({
  method: 'materialPropertiesSet',
  wire: 'material.properties.set',
  params: (input: MaterialPropertiesSetInput) => ({
    assetPath: input.assetPath,
    values: input.values,
    strict: input.strict,
  }),
  result: MaterialPropertiesSetResultSchema,

})

export type MaterialPropertyType = v.InferOutput<typeof MaterialPropertyTypeSchema>
export type MaterialColor = v.InferOutput<typeof MaterialColorSchema>
export type MaterialVector = v.InferOutput<typeof MaterialVectorSchema>
export type MaterialPropertyGetValue = v.InferOutput<typeof MaterialPropertyGetValueSchema>
export type MaterialPropertySetValue = v.InferOutput<typeof MaterialPropertySetValueSchema>
export type MaterialPropertiesGetInput = v.InferOutput<typeof MaterialPropertiesGetInputSchema>
export type MaterialPropertiesSetInput = v.InferOutput<typeof MaterialPropertiesSetInputSchema>
export type MaterialPropertiesGetResult = InferResult<typeof materialPropertiesGetCommand>
export type MaterialPropertiesSetResult = InferResult<typeof materialPropertiesSetCommand>
// --- end ---
