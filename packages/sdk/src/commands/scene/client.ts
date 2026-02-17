import * as v from 'valibot'
import type { CommandRuntime } from '../runtime.ts'
import { SceneActiveResultSchema, type SceneActiveResult } from './contract.ts'

export async function runSceneActive(runtime: CommandRuntime): Promise<SceneActiveResult> {
  const result = await runtime.send('scene.active', {})
  return v.parse(SceneActiveResultSchema, result)
}
