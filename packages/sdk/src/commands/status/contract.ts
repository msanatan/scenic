import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const StatusResultSchema = v.object({
  projectPath: v.string(),
  unityVersion: v.string(),
  pluginVersion: v.string(),
  activeScene: v.string(),
  playMode: v.string(),
})

export const statusCommand = defineCommand({
  method: 'status',
  wire: 'status',
  params: () => ({}),
  result: StatusResultSchema,
})

export type StatusResult = InferResult<typeof statusCommand>
