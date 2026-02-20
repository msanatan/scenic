import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: tags', () => {
  let client: UniBridgeClient
  const createdTags: string[] = []

  before(() => {
    client = createTestClient()
  })

  after(async () => {
    for (const tag of createdTags) {
      await client.execute(`var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"); if (assets != null && assets.Length > 0 && assets[0] != null) { var so = new UnityEditor.SerializedObject(assets[0]); var tags = so.FindProperty("tags"); if (tags != null && tags.isArray) { for (var i = tags.arraySize - 1; i >= 0; i--) { if (tags.GetArrayElementAtIndex(i).stringValue == "${tag}") { tags.DeleteArrayElementAtIndex(i); } } so.ApplyModifiedPropertiesWithoutUndo(); UnityEditor.AssetDatabase.SaveAssets(); } }`)
    }
    client.close()
  })

  it('returns tags including built-in markers', async () => {
    const result = await client.tagsGet()

    assert.equal(typeof result.total, 'number')
    assert.ok(Array.isArray(result.tags))
    assert.equal(result.total, result.tags.length)
    assert.ok(result.total > 0)

    const untagged = result.tags.find((tag) => tag.name === 'Untagged')
    assert.ok(untagged != null)
    assert.equal(untagged?.isBuiltIn, true)
  })

  it('adds a tag idempotently', async () => {
    const name = `UniBridgeTag_${Date.now()}`
    createdTags.push(name)

    const added = await client.tagsAdd({ name })
    assert.equal(added.tag.name, name)
    assert.equal(added.tag.isBuiltIn, false)
    assert.equal(added.added, true)
    assert.ok(added.total > 0)

    const addedAgain = await client.tagsAdd({ name })
    assert.equal(addedAgain.tag.name, name)
    assert.equal(addedAgain.added, false)

    const tags = await client.tagsGet()
    assert.ok(tags.tags.some((tag) => tag.name === name))
  })
})
