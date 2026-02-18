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

  it('lists scenes with pagination', async () => {
    const result = await client.sceneList({ limit: 10, offset: 0 })
    assert.equal(result.limit, 10)
    assert.equal(result.offset, 0)
    assert.equal(typeof result.total, 'number')
    assert.ok(Array.isArray(result.scenes))
    if (result.scenes.length > 0) {
      assert.equal(typeof result.scenes[0].name, 'string')
      assert.equal(typeof result.scenes[0].path, 'string')
      assert.ok(result.scenes[0].path.endsWith('.unity'))
    }
  })

  it('returns a flattened scene hierarchy with parent links', async () => {
    const result = await client.sceneHierarchy({ limit: 200, offset: 0 })
    assert.equal(result.limit, 200)
    assert.equal(result.offset, 0)
    assert.equal(typeof result.total, 'number')
    assert.ok(Array.isArray(result.nodes))
    if (result.nodes.length > 0) {
      const first = result.nodes[0]
      assert.equal(typeof first.name, 'string')
      assert.equal(typeof first.path, 'string')
      assert.equal(typeof first.isActive, 'boolean')
      assert.equal(typeof first.depth, 'number')
      assert.equal(typeof first.parentIndex, 'number')
      assert.equal(typeof first.siblingIndex, 'number')
    }
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
