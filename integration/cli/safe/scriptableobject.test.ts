import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { getCliEntrypoint, runCli } from '../../helpers/cli-runner.ts'

const typeName = 'Scenic.Editor.Commands.ScriptableObjects.ScenicSampleScriptableObject'

describe('CLI: scriptableobject', () => {
  const createdAssetPaths: string[] = []

  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  after(async () => {
    for (const assetPath of createdAssetPaths) {
      await runCli('execute', `UnityEditor.AssetDatabase.DeleteAsset("${assetPath}"); UnityEditor.AssetDatabase.SaveAssets();`)
    }
    await runCli('execute', 'UnityEditor.AssetDatabase.DeleteAsset("Assets/__TempTests__"); UnityEditor.AssetDatabase.SaveAssets();')
  })

  it('creates, gets, and updates a ScriptableObject asset', async () => {
    await runCli(
      'execute',
      'if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/__TempTests__")) UnityEditor.AssetDatabase.CreateFolder("Assets", "__TempTests__");',
    )

    const assetPath = `Assets/__TempTests__/CliScriptableObject_${Date.now()}.asset`
    createdAssetPaths.push(assetPath)

    const createPayload = (await runCli(
      'scriptableobject',
      'create',
      assetPath,
      '--type',
      typeName,
      '--values',
      '{"number":8.25,"label":"cli-create","enabledFlag":false}',
      '--strict',
    )) as {
      success: boolean
      result?: {
        asset: {
          assetPath: string
          type: string
        }
        appliedFields: string[]
      }
    }

    assert.equal(createPayload.success, true)
    assert.equal(createPayload.result?.asset.assetPath, assetPath)
    assert.equal(createPayload.result?.asset.type, typeName)
    assert.ok(createPayload.result?.appliedFields.includes('number'))

    const getPayload = (await runCli('scriptableobject', 'get', assetPath)) as {
      success: boolean
      result?: {
        asset: {
          assetPath: string
          type: string
        }
        serialized: Record<string, unknown>
      }
    }

    assert.equal(getPayload.success, true)
    assert.equal(getPayload.result?.asset.assetPath, assetPath)
    assert.equal(getPayload.result?.asset.type, typeName)
    assert.equal(typeof getPayload.result?.serialized, 'object')

    const updatePayload = (await runCli(
      'scriptableobject',
      'update',
      assetPath,
      '--values',
      '{"number":15.75,"label":"cli-update","enabledFlag":true}',
      '--strict',
    )) as {
      success: boolean
      result?: {
        asset: {
          assetPath: string
        }
        appliedFields: string[]
      }
    }

    assert.equal(updatePayload.success, true)
    assert.equal(updatePayload.result?.asset.assetPath, assetPath)
    assert.ok(updatePayload.result?.appliedFields.includes('number'))
    assert.ok(updatePayload.result?.appliedFields.includes('label'))
    assert.ok(updatePayload.result?.appliedFields.includes('enabledFlag'))

    const valuesPayload = (await runCli(
      'execute',
      `var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Scenic.Editor.Commands.ScriptableObjects.ScenicSampleScriptableObject>("${assetPath}"); asset.number + "|" + asset.label + "|" + asset.enabledFlag`,
    )) as {
      success: boolean
      result?: unknown
    }
    assert.equal(valuesPayload.success, true)
    assert.equal(valuesPayload.result, '15.75|cli-update|True')
  })
})
