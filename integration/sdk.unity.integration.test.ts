import assert from 'node:assert/strict'
import { test } from 'node:test'
import path from 'node:path'
import { createClient } from '../packages/sdk/src/client.ts'

const projectPath = path.resolve(import.meta.dirname, '../TestProjects/UniBridge3Dv6.3')

test('SDK integration: can query Unity status and active scene', async () => {
  const client = createClient({
    projectPath,
    connectTimeout: 10_000,
    commandTimeout: 20_000,
    reconnectTimeout: 20_000,
  })

  try {
    const status = await client.status()
    assert.equal(status.projectPath, projectPath)
    assert.equal(typeof status.unityVersion, 'string')
    assert.equal(typeof status.pluginVersion, 'string')
    assert.equal(typeof status.activeScene, 'string')
    assert.equal(typeof status.playMode, 'string')
    assert.ok(status.unityVersion.length > 0)

    const scene = await client.sceneActive()
    assert.equal(typeof scene.scene.name, 'string')
    assert.equal(typeof scene.scene.path, 'string')
    assert.equal(typeof scene.scene.isDirty, 'boolean')
    assert.ok(scene.scene.name.length > 0)
  } finally {
    client.close()
  }
})
