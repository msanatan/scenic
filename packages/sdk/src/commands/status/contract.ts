import * as v from 'valibot'

export const StatusResultSchema = v.object({
  projectPath: v.string(),
  unityVersion: v.string(),
  pluginVersion: v.string(),
  activeScene: v.string(),
  playMode: v.string(),
})

export type StatusResult = v.InferOutput<typeof StatusResultSchema>
