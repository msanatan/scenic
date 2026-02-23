import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { ScenicClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient, projectPath } from '../../helpers/sdk-client.ts'

describe('SDK: status', () => {
  let client: ScenicClient

  before(() => {
    client = createTestClient()
  })
  after(() => {
    client.close()
  })

  it('returns project path, versions, scene, and play mode', async () => {
    const status = await client.status()
    assert.equal(status.projectPath, projectPath)
    assert.equal(typeof status.unityVersion, 'string')
    assert.equal(typeof status.pluginVersion, 'string')
    assert.equal(typeof status.activeScene, 'string')
    assert.equal(typeof status.playMode, 'string')
    assert.ok(status.unityVersion.length > 0)
  })
})
