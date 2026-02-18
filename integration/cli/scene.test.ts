import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../helpers/cli-runner.ts'

describe('CLI: scene', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('returns the active scene', async () => {
    const payload = (await runCli('scene', 'active')) as {
      success: boolean
      result?: {
        scene: {
          name: string
          path: string
          isDirty: boolean
        }
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(typeof payload.result?.scene.name, 'string')
    assert.equal(typeof payload.result?.scene.path, 'string')
    assert.equal(typeof payload.result?.scene.isDirty, 'boolean')
  })

  it('lists scenes with pagination', async () => {
    const payload = (await runCli('scene', 'list', '--limit', '10', '--offset', '0')) as {
      success: boolean
      result?: {
        scenes: Array<{
          name: string
          path: string
        }>
        total: number
        limit: number
        offset: number
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.limit, 10)
    assert.equal(payload.result?.offset, 0)
    assert.equal(typeof payload.result?.total, 'number')
    assert.ok(Array.isArray(payload.result?.scenes))
    if ((payload.result?.scenes.length ?? 0) > 0) {
      assert.equal(typeof payload.result?.scenes[0].name, 'string')
      assert.equal(typeof payload.result?.scenes[0].path, 'string')
      assert.ok((payload.result?.scenes[0].path ?? '').endsWith('.unity'))
    }
  })

  describe('open', () => {
    after(async () => {
      await runCli('scene', 'open', 'Assets/Scenes/SampleScene.unity')
    })

    it('opens a scene by path', async () => {
      const payload = (await runCli('scene', 'open', 'Assets/Scenes/SampleScene.unity')) as {
        success: boolean
        result?: {
          scene: {
            name: string
            path: string
            isDirty: boolean
          }
        }
        error?: string
      }

      assert.equal(payload.success, true)
      assert.equal(typeof payload.result?.scene.name, 'string')
      assert.equal(payload.result?.scene.path, 'Assets/Scenes/SampleScene.unity')
      assert.equal(typeof payload.result?.scene.isDirty, 'boolean')
    })
  })

  describe('create', () => {
    const testScenePath = 'Assets/Scenes/__CliTestCreated__.unity'

    after(async () => {
      await runCli('execute', `UnityEditor.AssetDatabase.DeleteAsset("${testScenePath}")`)
      await runCli('scene', 'open', 'Assets/Scenes/SampleScene.unity')
    })

    it('creates a new scene at the given path', async () => {
      const payload = (await runCli('scene', 'create', testScenePath)) as {
        success: boolean
        result?: {
          scene: {
            name: string
            path: string
            isDirty: boolean
          }
        }
        error?: string
      }

      assert.equal(payload.success, true)
      assert.equal(typeof payload.result?.scene.name, 'string')
      assert.equal(payload.result?.scene.path, testScenePath)
      assert.equal(typeof payload.result?.scene.isDirty, 'boolean')
    })
  })
})
