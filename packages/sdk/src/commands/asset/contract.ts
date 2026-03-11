import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const AssetFindInputSchema = v.object({
  query: v.optional(v.string()),
  type: v.optional(v.string()),
  labels: v.optional(v.array(v.string())),
  limit: v.optional(v.number()),
  offset: v.optional(v.number()),
})

export const AssetFindResultSchema = v.object({
  assets: v.array(v.object({
    assetPath: v.string(),
    guid: v.string(),
    type: v.string(),
    name: v.string(),
  })),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export const assetFindCommand = defineCommand({
  method: 'assetFind',
  wire: 'asset.find',
  params: (input: AssetFindInput) => ({
    query: input.query,
    type: input.type,
    labels: input.labels,
    limit: input.limit,
    offset: input.offset,
  }),
  result: AssetFindResultSchema,
})

export type AssetFindInput = v.InferOutput<typeof AssetFindInputSchema>
export type AssetFindResult = InferResult<typeof assetFindCommand>

export const AssetGetInputSchema = v.object({
  assetPath: v.string(),
})

export const AssetGetResultSchema = v.object({
  assetPath: v.string(),
  guid: v.string(),
  type: v.string(),
  name: v.string(),
  labels: v.array(v.string()),
  dependencies: v.array(v.string()),
})

export const assetGetCommand = defineCommand({
  method: 'assetGet',
  wire: 'asset.get',
  params: (input: AssetGetInput) => ({
    assetPath: input.assetPath,
  }),
  result: AssetGetResultSchema,
})

export type AssetGetInput = v.InferOutput<typeof AssetGetInputSchema>
export type AssetGetResult = InferResult<typeof assetGetCommand>

export const AssetMoveInputSchema = v.object({
  assetPath: v.string(),
  newPath: v.string(),
})

export const AssetMoveResultSchema = v.object({
  oldPath: v.string(),
  newPath: v.string(),
  guid: v.string(),
})

export const assetMoveCommand = defineCommand({
  method: 'assetMove',
  wire: 'asset.move',
  params: (input: AssetMoveInput) => ({
    assetPath: input.assetPath,
    newPath: input.newPath,
  }),
  result: AssetMoveResultSchema,
  guard: 'execute',
})

export type AssetMoveInput = v.InferOutput<typeof AssetMoveInputSchema>
export type AssetMoveResult = InferResult<typeof assetMoveCommand>

export const AssetCopyInputSchema = v.object({
  assetPath: v.string(),
  newPath: v.string(),
})

export const AssetCopyResultSchema = v.object({
  sourcePath: v.string(),
  newPath: v.string(),
  guid: v.string(),
})

export const assetCopyCommand = defineCommand({
  method: 'assetCopy',
  wire: 'asset.copy',
  params: (input: AssetCopyInput) => ({
    assetPath: input.assetPath,
    newPath: input.newPath,
  }),
  result: AssetCopyResultSchema,
  guard: 'execute',
})

export type AssetCopyInput = v.InferOutput<typeof AssetCopyInputSchema>
export type AssetCopyResult = InferResult<typeof assetCopyCommand>

export const AssetDeleteInputSchema = v.object({
  assetPath: v.string(),
})

export const AssetDeleteResultSchema = v.object({
  assetPath: v.string(),
  deleted: v.boolean(),
})

export const assetDeleteCommand = defineCommand({
  method: 'assetDelete',
  wire: 'asset.delete',
  params: (input: AssetDeleteInput) => ({
    assetPath: input.assetPath,
  }),
  result: AssetDeleteResultSchema,
  guard: 'execute',
})

export type AssetDeleteInput = v.InferOutput<typeof AssetDeleteInputSchema>
export type AssetDeleteResult = InferResult<typeof assetDeleteCommand>

export const AssetImportInputSchema = v.object({
  assetPath: v.string(),
  options: v.optional(v.string()),
})

export const AssetImportResultSchema = v.object({
  assetPath: v.string(),
  importerType: v.string(),
})

export const assetImportCommand = defineCommand({
  method: 'assetImport',
  wire: 'asset.import',
  params: (input: AssetImportInput) => ({
    assetPath: input.assetPath,
    options: input.options,
  }),
  result: AssetImportResultSchema,
  guard: 'execute',
})

export type AssetImportInput = v.InferOutput<typeof AssetImportInputSchema>
export type AssetImportResult = InferResult<typeof assetImportCommand>

export const AssetImportSettingsGetInputSchema = v.object({
  assetPath: v.string(),
  properties: v.optional(v.array(v.string())),
})

export const AssetImportSettingsGetResultSchema = v.object({
  assetPath: v.string(),
  importerType: v.string(),
  properties: v.record(v.string(), v.unknown()),
})

export const assetImportSettingsGetCommand = defineCommand({
  method: 'assetImportSettingsGet',
  wire: 'asset.importSettings.get',
  params: (input: AssetImportSettingsGetInput) => ({
    assetPath: input.assetPath,
    properties: input.properties,
  }),
  result: AssetImportSettingsGetResultSchema,
})

export type AssetImportSettingsGetInput = v.InferOutput<typeof AssetImportSettingsGetInputSchema>
export type AssetImportSettingsGetResult = InferResult<typeof assetImportSettingsGetCommand>

export const AssetImportSettingsSetInputSchema = v.object({
  assetPath: v.string(),
  properties: v.record(v.string(), v.unknown()),
})

export const AssetImportSettingsSetResultSchema = v.object({
  assetPath: v.string(),
  importerType: v.string(),
  appliedProperties: v.array(v.string()),
})

export const assetImportSettingsSetCommand = defineCommand({
  method: 'assetImportSettingsSet',
  wire: 'asset.importSettings.set',
  params: (input: AssetImportSettingsSetInput) => ({
    assetPath: input.assetPath,
    properties: input.properties,
  }),
  result: AssetImportSettingsSetResultSchema,
  guard: 'execute',
})

export type AssetImportSettingsSetInput = v.InferOutput<typeof AssetImportSettingsSetInputSchema>
export type AssetImportSettingsSetResult = InferResult<typeof assetImportSettingsSetCommand>

export const AssetLabelsGetInputSchema = v.object({
  assetPath: v.string(),
})

export const AssetLabelsGetResultSchema = v.object({
  assetPath: v.string(),
  labels: v.array(v.string()),
})

export const assetLabelsGetCommand = defineCommand({
  method: 'assetLabelsGet',
  wire: 'asset.labels.get',
  params: (input: AssetLabelsGetInput) => ({
    assetPath: input.assetPath,
  }),
  result: AssetLabelsGetResultSchema,
})

export type AssetLabelsGetInput = v.InferOutput<typeof AssetLabelsGetInputSchema>
export type AssetLabelsGetResult = InferResult<typeof assetLabelsGetCommand>

export const AssetLabelsAddInputSchema = v.object({
  assetPath: v.string(),
  labels: v.array(v.string()),
})

export const AssetLabelsAddResultSchema = v.object({
  assetPath: v.string(),
  labels: v.array(v.string()),
  added: v.array(v.string()),
})

export const assetLabelsAddCommand = defineCommand({
  method: 'assetLabelsAdd',
  wire: 'asset.labels.add',
  params: (input: AssetLabelsAddInput) => ({
    assetPath: input.assetPath,
    labels: input.labels,
  }),
  result: AssetLabelsAddResultSchema,
  guard: 'execute',
})

export type AssetLabelsAddInput = v.InferOutput<typeof AssetLabelsAddInputSchema>
export type AssetLabelsAddResult = InferResult<typeof assetLabelsAddCommand>

export const AssetLabelsRemoveInputSchema = v.object({
  assetPath: v.string(),
  labels: v.array(v.string()),
})

export const AssetLabelsRemoveResultSchema = v.object({
  assetPath: v.string(),
  labels: v.array(v.string()),
  removed: v.array(v.string()),
})

export const assetLabelsRemoveCommand = defineCommand({
  method: 'assetLabelsRemove',
  wire: 'asset.labels.remove',
  params: (input: AssetLabelsRemoveInput) => ({
    assetPath: input.assetPath,
    labels: input.labels,
  }),
  result: AssetLabelsRemoveResultSchema,
  guard: 'execute',
})

export type AssetLabelsRemoveInput = v.InferOutput<typeof AssetLabelsRemoveInputSchema>
export type AssetLabelsRemoveResult = InferResult<typeof assetLabelsRemoveCommand>
// --- end ---
