import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const PackageItemSchema = v.object({
  name: v.string(),
  displayName: v.string(),
  version: v.string(),
  source: v.string(),
  isDirectDependency: v.boolean(),
})

export const PackagesGetResultSchema = v.object({
  packages: v.array(PackageItemSchema),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export interface PackagesGetQuery {
  limit?: number
  offset?: number
  includeIndirect?: boolean
  search?: string
}

export const packagesGetCommand = defineCommand({
  method: 'packagesGet',
  wire: 'packages.get',
  params: (query?: PackagesGetQuery) => ({
    limit: query?.limit,
    offset: query?.offset,
    includeIndirect: query?.includeIndirect,
    search: query?.search,
  }),
  result: PackagesGetResultSchema,
})

export const PackagesAddInputSchema = v.object({
  name: v.string(),
  version: v.optional(v.string()),
})

export const PackagesAddResultSchema = v.object({
  package: PackageItemSchema,
  added: v.boolean(),
  total: v.number(),
})

export const packagesAddCommand = defineCommand({
  method: 'packagesAdd',
  wire: 'packages.add',
  params: (input: PackagesAddInput) => ({
    name: input.name,
    version: input.version,
  }),
  result: PackagesAddResultSchema,
})

export const PackagesRemoveInputSchema = v.object({
  name: v.string(),
})

export const PackagesRemoveResultSchema = v.object({
  package: PackageItemSchema,
  removed: v.boolean(),
  total: v.number(),
})

export const packagesRemoveCommand = defineCommand({
  method: 'packagesRemove',
  wire: 'packages.remove',
  params: (input: PackagesRemoveInput) => ({
    name: input.name,
  }),
  result: PackagesRemoveResultSchema,
})

export type PackageItem = v.InferOutput<typeof PackageItemSchema>
export type PackagesGetResult = InferResult<typeof packagesGetCommand>
export type PackagesAddInput = v.InferOutput<typeof PackagesAddInputSchema>
export type PackagesAddResult = InferResult<typeof packagesAddCommand>
export type PackagesRemoveInput = v.InferOutput<typeof PackagesRemoveInputSchema>
export type PackagesRemoveResult = InferResult<typeof packagesRemoveCommand>
