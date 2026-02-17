import type { CommandRuntime } from '../runtime.ts'
import type { SceneActiveResult } from './contract.ts'

export async function runSceneActive(runtime: CommandRuntime): Promise<SceneActiveResult> {
  const result = await runtime.send('scene.active', {})
  if (typeof result !== 'string') {
    throw new Error('Scene active command returned an invalid payload.')
  }

  return JSON.parse(result) as SceneActiveResult
}
