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

export type PackageItem = v.InferOutput<typeof PackageItemSchema>
export type PackagesGetResult = InferResult<typeof packagesGetCommand>
