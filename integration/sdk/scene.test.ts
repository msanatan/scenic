import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../packages/sdk/src/index.ts'
import { createTestClient } from '../helpers/sdk-client.ts'

describe('SDK: scene', () => {
  let client: UniBridgeClient

  before(() => {
    client = createTestClient()
  })
  after(() => {
    client.close()
  })

  it('returns the active scene', async () => {
    const scene = await client.sceneActive()
    assert.equal(typeof scene.scene.name, 'string')
    assert.equal(typeof scene.scene.path, 'string')
    assert.equal(typeof scene.scene.isDirty, 'boolean')
    assert.ok(scene.scene.name.length > 0)
  })

  describe('open', () => {
    let originalScenePath: string

    before(async () => {
      const active = await client.sceneActive()
      originalScenePath = active.scene.path
    })

    after(async () => {
      await client.sceneOpen(originalScenePath)
    })

    it('opens a scene by path', async () => {
      const result = await client.sceneOpen('Assets/Scenes/SampleScene.unity')
      assert.equal(typeof result.scene.name, 'string')
      assert.equal(typeof result.scene.path, 'string')
      assert.equal(typeof result.scene.isDirty, 'boolean')
      assert.equal(result.scene.path, 'Assets/Scenes/SampleScene.unity')
    })
  })

  describe('create', () => {
    const testScenePath = 'Assets/Scenes/__TestCreated__.unity'

    after(async () => {
      await client.execute(
        `UnityEditor.AssetDatabase.DeleteAsset("${testScenePath}")`,
      )
      await client.sceneOpen('Assets/Scenes/SampleScene.unity')
    })

    it('creates a new scene at the given path', async () => {
      const result = await client.sceneCreate(testScenePath)
      assert.equal(typeof result.scene.name, 'string')
      assert.equal(result.scene.path, testScenePath)
      assert.equal(typeof result.scene.isDirty, 'boolean')
    })
  })
})
