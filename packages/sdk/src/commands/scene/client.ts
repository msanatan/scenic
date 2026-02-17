import type { CommandRuntime } from '../runtime.ts'
import type { SceneActiveResult } from './contract.ts'

export async function runSceneActive(runtime: CommandRuntime): Promise<SceneActiveResult> {
  const result = await runtime.send('scene.active', {})
  return result as SceneActiveResult
}
