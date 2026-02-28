import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

export const SettingsResultSchema = v.object({
  executeEnabled: v.boolean(),
})

export const SettingsUpdateInputSchema = v.object({
  executeEnabled: v.boolean(),
})

export type SettingsUpdateInput = v.InferOutput<typeof SettingsUpdateInputSchema>

export const settingsGetCommand = defineCommand({
  method: 'settingsGet',
  wire: 'settings.get',
  params: () => ({}),
  result: SettingsResultSchema,
})

export const settingsUpdateCommand = defineCommand({
  method: 'settingsUpdate',
  wire: 'settings.update',
  params: (input: SettingsUpdateInput) => v.parse(SettingsUpdateInputSchema, input),
  result: SettingsResultSchema,
})

export type SettingsResult = InferResult<typeof settingsGetCommand>
