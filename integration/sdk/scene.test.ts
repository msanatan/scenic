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
})
