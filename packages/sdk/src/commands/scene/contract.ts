import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const SceneInfoSchema = v.object({
  name: v.string(),
  path: v.string(),
  isDirty: v.boolean(),
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
export type SceneActiveResult = InferResult<typeof sceneActiveCommand>
export type SceneOpenResult = InferResult<typeof sceneOpenCommand>
export type SceneCreateResult = InferResult<typeof sceneCreateCommand>
