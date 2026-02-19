import * as v from 'valibot'
import { defineCommand, type InferResult } from '../define.ts'

const EditorStateResultSchema = v.object({
  playMode: v.picklist(['edit', 'playing', 'paused']),
})

export const editorPlayCommand = defineCommand({
  method: 'editorPlay',
  wire: 'editor.play',
  params: () => ({}),
  result: EditorStateResultSchema,
})

export const editorPauseCommand = defineCommand({
  method: 'editorPause',
  wire: 'editor.pause',
  params: () => ({}),
  result: EditorStateResultSchema,
})

export const editorStopCommand = defineCommand({
  method: 'editorStop',
  wire: 'editor.stop',
  params: () => ({}),
  result: EditorStateResultSchema,
})

export type EditorStateResult = InferResult<typeof editorPlayCommand>
