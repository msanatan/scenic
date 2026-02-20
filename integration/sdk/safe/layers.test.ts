import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: layers', () => {
  let client: UniBridgeClient
  const createdLayers: string[] = []

  before(() => {
    client = createTestClient()
  })

  after(async () => {
    for (const layer of createdLayers) {
      await client.execute(`var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"); if (assets != null && assets.Length > 0 && assets[0] != null) { var so = new UnityEditor.SerializedObject(assets[0]); var layers = so.FindProperty("layers"); if (layers != null && layers.isArray) { for (var i = 31; i >= 8; i--) { var p = layers.GetArrayElementAtIndex(i); if (p.stringValue == "${layer}") { p.stringValue = string.Empty; } } so.ApplyModifiedPropertiesWithoutUndo(); UnityEditor.AssetDatabase.SaveAssets(); } }`)
    }
    client.close()
  })

  it('returns paginated layer slots', async () => {
    const result = await client.layersGet({ limit: 10, offset: 0 })

    assert.equal(result.limit, 10)
    assert.equal(result.offset, 0)
    assert.equal(result.total, 32)
    assert.equal(result.layers.length, 10)

    const first = result.layers[0]
    assert.equal(first.index, 0)
    assert.equal(typeof first.name, 'string')
    assert.equal(typeof first.isBuiltIn, 'boolean')
    assert.equal(typeof first.isUserEditable, 'boolean')
    assert.equal(typeof first.isOccupied, 'boolean')
    assert.equal(first.isBuiltIn, true)
    assert.equal(first.isUserEditable, false)
  })

  it('adds a layer idempotently', async () => {
    const name = `UniBridgeLayer_${Date.now()}`
    createdLayers.push(name)

    const added = await client.layersAdd({ name })
    assert.equal(added.layer.name, name)
    assert.equal(added.layer.isUserEditable, true)
    assert.equal(added.layer.isOccupied, true)
    assert.equal(added.added, true)
    assert.equal(added.total, 32)

    const addedAgain = await client.layersAdd({ name })
    assert.equal(addedAgain.layer.name, name)
    assert.equal(addedAgain.added, false)
  })

  it('removes a layer idempotently', async () => {
    const name = `UniBridgeLayer_${Date.now()}`
    createdLayers.push(name)

    await client.layersAdd({ name })
    const removed = await client.layersRemove({ name })
    assert.equal(removed.layer.name, name)
    assert.equal(removed.layer.isUserEditable, true)
    assert.equal(removed.removed, true)

    const removedAgain = await client.layersRemove({ name })
    assert.equal(removedAgain.layer.name, name)
    assert.equal(removedAgain.removed, false)
  })
})
