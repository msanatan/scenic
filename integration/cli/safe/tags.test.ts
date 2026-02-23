import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'

describe('CLI: tags', () => {
  const createdTags: string[] = []

  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  after(async () => {
    for (const tag of createdTags) {
      await runCli('execute', `var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"); if (assets != null && assets.Length > 0 && assets[0] != null) { var so = new UnityEditor.SerializedObject(assets[0]); var tags = so.FindProperty("tags"); if (tags != null && tags.isArray) { for (var i = tags.arraySize - 1; i >= 0; i--) { if (tags.GetArrayElementAtIndex(i).stringValue == "${tag}") { tags.DeleteArrayElementAtIndex(i); } } so.ApplyModifiedPropertiesWithoutUndo(); UnityEditor.AssetDatabase.SaveAssets(); } }`)
    }
  })

  it('returns tags including built-in markers', async () => {
    const payload = (await runCli('tags', 'get', '--limit', '10', '--offset', '0')) as {
      success: boolean
      result?: {
        tags: Array<{
          name: string
          isBuiltIn: boolean
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
    assert.ok(Array.isArray(payload.result?.tags))
    assert.ok((payload.result?.tags.length ?? 0) <= 10)
    assert.ok((payload.result?.total ?? 0) >= (payload.result?.tags.length ?? 0))
    assert.ok((payload.result?.total ?? 0) > 0)

    const untagged = payload.result?.tags.find((tag) => tag.name === 'Untagged')
    if (untagged != null) {
      assert.equal(untagged.isBuiltIn, true)
    }
  })

  it('adds a tag idempotently', async () => {
    const name = `ScenicTag_${Date.now()}`
    createdTags.push(name)

    const addPayload = (await runCli('tags', 'add', name)) as {
      success: boolean
      result?: {
        tag: {
          name: string
          isBuiltIn: boolean
        }
        added: boolean
        total: number
      }
      error?: string
    }

    assert.equal(addPayload.success, true)
    assert.equal(addPayload.result?.tag.name, name)
    assert.equal(addPayload.result?.tag.isBuiltIn, false)
    assert.equal(addPayload.result?.added, true)

    const addAgainPayload = (await runCli('tags', 'add', name)) as {
      success: boolean
      result?: {
        added: boolean
      }
      error?: string
    }

    assert.equal(addAgainPayload.success, true)
    assert.equal(addAgainPayload.result?.added, false)

    const getPayload = (await runCli('tags', 'get')) as {
      success: boolean
      result?: {
        tags: Array<{ name: string }>
      }
    }
    assert.equal(getPayload.success, true)
    assert.ok(getPayload.result?.tags.some((tag) => tag.name === name))
  })

  it('removes a tag idempotently', async () => {
    const name = `ScenicTag_${Date.now()}`
    createdTags.push(name)

    const addPayload = (await runCli('tags', 'add', name)) as {
      success: boolean
      result?: { added: boolean }
    }
    assert.equal(addPayload.success, true)
    assert.equal(addPayload.result?.added, true)

    const removePayload = (await runCli('tags', 'remove', name)) as {
      success: boolean
      result?: {
        tag: {
          name: string
          isBuiltIn: boolean
        }
        removed: boolean
      }
      error?: string
    }
    assert.equal(removePayload.success, true)
    assert.equal(removePayload.result?.tag.name, name)
    assert.equal(removePayload.result?.tag.isBuiltIn, false)
    assert.equal(removePayload.result?.removed, true)

    const removeAgainPayload = (await runCli('tags', 'remove', name)) as {
      success: boolean
      result?: { removed: boolean }
    }
    assert.equal(removeAgainPayload.success, true)
    assert.equal(removeAgainPayload.result?.removed, false)
  })

})
