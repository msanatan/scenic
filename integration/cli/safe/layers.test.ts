import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'

describe('CLI: layers', () => {
  const createdLayers: string[] = []

  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  after(async () => {
    for (const layer of createdLayers) {
      await runCli('execute', `var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"); if (assets != null && assets.Length > 0 && assets[0] != null) { var so = new UnityEditor.SerializedObject(assets[0]); var layers = so.FindProperty("layers"); if (layers != null && layers.isArray) { for (var i = 31; i >= 8; i--) { var p = layers.GetArrayElementAtIndex(i); if (p.stringValue == "${layer}") { p.stringValue = string.Empty; } } so.ApplyModifiedPropertiesWithoutUndo(); UnityEditor.AssetDatabase.SaveAssets(); } }`)
    }
  })

  it('returns paginated layer slots', async () => {
    const payload = (await runCli('layers', 'get', '--limit', '10', '--offset', '0')) as {
      success: boolean
      result?: {
        layers: Array<{
          index: number
          name: string
          isBuiltIn: boolean
          isUserEditable: boolean
          isOccupied: boolean
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
    assert.equal(payload.result?.total, 32)
    assert.equal(payload.result?.layers.length, 10)

    const first = payload.result?.layers[0]
    assert.equal(first?.index, 0)
    assert.equal(typeof first?.name, 'string')
    assert.equal(typeof first?.isBuiltIn, 'boolean')
    assert.equal(typeof first?.isUserEditable, 'boolean')
    assert.equal(typeof first?.isOccupied, 'boolean')
    assert.equal(first?.isBuiltIn, true)
    assert.equal(first?.isUserEditable, false)
  })

  it('adds a layer idempotently', async () => {
    const name = `ScenicLayer_${Date.now()}`
    createdLayers.push(name)

    const addPayload = (await runCli('layers', 'add', name)) as {
      success: boolean
      result?: {
        layer: {
          index: number
          name: string
          isBuiltIn: boolean
          isUserEditable: boolean
          isOccupied: boolean
        }
        added: boolean
        total: number
      }
      error?: string
    }

    assert.equal(addPayload.success, true)
    assert.equal(addPayload.result?.layer.name, name)
    assert.equal(addPayload.result?.layer.isUserEditable, true)
    assert.equal(addPayload.result?.layer.isOccupied, true)
    assert.equal(addPayload.result?.added, true)
    assert.equal(addPayload.result?.total, 32)

    const addAgainPayload = (await runCli('layers', 'add', name)) as {
      success: boolean
      result?: {
        added: boolean
        layer: { name: string }
      }
      error?: string
    }

    assert.equal(addAgainPayload.success, true)
    assert.equal(addAgainPayload.result?.layer.name, name)
    assert.equal(addAgainPayload.result?.added, false)
  })

  it('removes a layer idempotently', async () => {
    const name = `ScenicLayer_${Date.now()}`
    createdLayers.push(name)

    const addPayload = (await runCli('layers', 'add', name)) as {
      success: boolean
      result?: { added: boolean }
    }
    assert.equal(addPayload.success, true)
    assert.equal(addPayload.result?.added, true)

    const removePayload = (await runCli('layers', 'remove', name)) as {
      success: boolean
      result?: {
        layer: {
          name: string
          isUserEditable: boolean
        }
        removed: boolean
      }
      error?: string
    }
    assert.equal(removePayload.success, true)
    assert.equal(removePayload.result?.layer.name, name)
    assert.equal(removePayload.result?.layer.isUserEditable, true)
    assert.equal(removePayload.result?.removed, true)

    const removeAgainPayload = (await runCli('layers', 'remove', name)) as {
      success: boolean
      result?: {
        layer: { name: string }
        removed: boolean
      }
    }
    assert.equal(removeAgainPayload.success, true)
    assert.equal(removeAgainPayload.result?.layer.name, name)
    assert.equal(removeAgainPayload.result?.removed, false)
  })
})
