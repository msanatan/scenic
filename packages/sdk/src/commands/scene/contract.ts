import * as v from 'valibot'

const SceneInfoSchema = v.object({
  name: v.string(),
  path: v.string(),
  isDirty: v.boolean(),
})

export const SceneActiveResultSchema = v.object({
  scene: SceneInfoSchema,
})

export type SceneInfo = v.InferOutput<typeof SceneInfoSchema>
export type SceneActiveResult = v.InferOutput<typeof SceneActiveResultSchema>
