import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import type { ScenicClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

const typeName = 'Scenic.Editor.Commands.ScriptableObjects.ScenicSampleScriptableObject'

describe('SDK: scriptableobject', () => {
  let client: ScenicClient
  const createdAssetPaths: string[] = []

  before(() => {
    client = createTestClient()
  })

  after(async () => {
    for (const assetPath of createdAssetPaths) {
      await client.execute(`UnityEditor.AssetDatabase.DeleteAsset("${assetPath}"); UnityEditor.AssetDatabase.SaveAssets();`)
    }
    await client.execute('UnityEditor.AssetDatabase.DeleteAsset("Assets/__TempTests__"); UnityEditor.AssetDatabase.SaveAssets();').catch(() => undefined)
    client.close()
  })

  it('creates, gets, and updates a ScriptableObject asset', async () => {
    await client.execute(
      'if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/__TempTests__")) UnityEditor.AssetDatabase.CreateFolder("Assets", "__TempTests__");',
    )

    const assetPath = `Assets/__TempTests__/SdkScriptableObject_${Date.now()}.asset`
    createdAssetPaths.push(assetPath)

    const created = await client.scriptableObjectCreate({
      assetPath,
      type: typeName,
      initialValues: {
        number: 8.25,
        label: 'sdk-create',
        enabledFlag: false,
      },
      strict: true,
    })

    assert.equal(created.asset.assetPath, assetPath)
    assert.equal(created.asset.type, typeName)
    assert.ok(created.appliedFields.includes('number'))
    assert.ok(created.appliedFields.includes('label'))

    const fetched = await client.scriptableObjectGet({ assetPath })
    assert.equal(fetched.asset.assetPath, assetPath)
    assert.equal(fetched.asset.type, typeName)
    assert.equal(typeof fetched.serialized, 'object')
    assert.notEqual(fetched.serialized, null)

    const updated = await client.scriptableObjectUpdate({
      assetPath,
      values: {
        number: 12.5,
        label: 'sdk-update',
        enabledFlag: true,
      },
      strict: true,
    })

    assert.equal(updated.asset.assetPath, assetPath)
    assert.ok(updated.appliedFields.includes('number'))
    assert.ok(updated.appliedFields.includes('label'))
    assert.ok(updated.appliedFields.includes('enabledFlag'))

    const values = await client.execute(
      `var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Scenic.Editor.Commands.ScriptableObjects.ScenicSampleScriptableObject>("${assetPath}"); asset.number + "|" + asset.label + "|" + asset.enabledFlag`,
    )
    assert.equal(values, '12.5|sdk-update|True')
  })
})
