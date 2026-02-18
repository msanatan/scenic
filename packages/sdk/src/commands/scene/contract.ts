import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const SceneInfoSchema = v.object({
  name: v.string(),
  path: v.string(),
  isDirty: v.boolean(),
})

const SceneListItemSchema = v.object({
  name: v.string(),
  path: v.string(),
})

const SceneHierarchyNodeSchema = v.object({
  name: v.string(),
  path: v.string(),
  isActive: v.boolean(),
  depth: v.number(),
  parentIndex: v.number(),
  siblingIndex: v.number(),
})

export const SceneListResultSchema = v.object({
  scenes: v.array(SceneListItemSchema),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export const SceneHierarchyResultSchema = v.object({
  nodes: v.array(SceneHierarchyNodeSchema),
  total: v.number(),
  limit: v.number(),
  offset: v.number(),
})

export const sceneListCommand = defineCommand({
  method: 'sceneList',
  wire: 'scene.list',
  params: (query?: SceneListQuery) => ({
    filter: query?.filter,
    limit: query?.limit,
    offset: query?.offset,
  }),
  result: SceneListResultSchema,
})

export const sceneHierarchyCommand = defineCommand({
  method: 'sceneHierarchy',
  wire: 'scene.hierarchy',
  params: (query?: SceneHierarchyQuery) => ({
    limit: query?.limit,
    offset: query?.offset,
  }),
  result: SceneHierarchyResultSchema,
})

export const SceneActiveResultSchema = v.object({
  scene: SceneInfoSchema,
})

export const sceneActiveCommand = defineCommand({
  method: 'sceneActive',
  wire: 'scene.active',
  params: () => ({}),
  result: SceneActiveResultSchema,
})

export const SceneOpenResultSchema = v.object({
  scene: SceneInfoSchema,
})

export const sceneOpenCommand = defineCommand({
  method: 'sceneOpen',
  wire: 'scene.open',
  params: (path: string) => ({ path }),
  result: SceneOpenResultSchema,
})

export const SceneCreateResultSchema = v.object({
  scene: SceneInfoSchema,
})

export const sceneCreateCommand = defineCommand({
  method: 'sceneCreate',
  wire: 'scene.create',
  params: (path: string) => ({ path }),
  result: SceneCreateResultSchema,
})

export type SceneInfo = v.InferOutput<typeof SceneInfoSchema>
export type SceneListItem = v.InferOutput<typeof SceneListItemSchema>
export type SceneHierarchyNode = v.InferOutput<typeof SceneHierarchyNodeSchema>
export interface SceneListQuery {
  filter?: string
  limit?: number
  offset?: number
}
export interface SceneHierarchyQuery {
  limit?: number
  offset?: number
}
export type SceneListResult = InferResult<typeof sceneListCommand>
export type SceneHierarchyResult = InferResult<typeof sceneHierarchyCommand>
export type SceneActiveResult = InferResult<typeof sceneActiveCommand>
export type SceneOpenResult = InferResult<typeof sceneOpenCommand>
export type SceneCreateResult = InferResult<typeof sceneCreateCommand>
