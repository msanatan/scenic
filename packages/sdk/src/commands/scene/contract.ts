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

export type SceneInfo = v.InferOutput<typeof SceneInfoSchema>
export type SceneActiveResult = InferResult<typeof sceneActiveCommand>
