import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { ScenicClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: material', () => {
  let client: ScenicClient
  const createdAssetPaths: string[] = []
  const createdGameObjectInstanceIds: number[] = []

  before(() => {
    client = createTestClient()
  })

  after(async () => {
    for (const assetPath of createdAssetPaths) {
      await client.execute(`UnityEditor.AssetDatabase.DeleteAsset("${assetPath}"); UnityEditor.AssetDatabase.SaveAssets();`)
    }
    for (const instanceId of createdGameObjectInstanceIds) {
      await client.gameObjectDestroy({ instanceId }).catch(() => undefined)
    }
    await client.execute('UnityEditor.AssetDatabase.DeleteAsset("Assets/__TempTests__"); UnityEditor.AssetDatabase.SaveAssets();').catch(() => undefined)
    client.close()
  })

  it('creates, gets, and assigns a material asset', async () => {
    await client.execute(
      'if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/__TempTests__")) UnityEditor.AssetDatabase.CreateFolder("Assets", "__TempTests__");',
    )

    const assetPath = `Assets/__TempTests__/SdkMaterial_${Date.now()}.mat`
    const expectedName = assetPath.replace(/^.*\//, '').replace(/\.mat$/i, '')
    createdAssetPaths.push(assetPath)

    const created = await client.materialCreate({ assetPath })
    assert.equal(created.material.assetPath, assetPath)
    assert.equal(created.material.name, expectedName)
    assert.equal(typeof created.material.shader, 'string')
    assert.notEqual(created.material.shader.length, 0)

    const fetched = await client.materialGet({ assetPath })
    assert.equal(fetched.material.assetPath, assetPath)
    assert.equal(fetched.material.name, created.material.name)
    assert.equal(fetched.material.shader, created.material.shader)
    assert.equal(typeof fetched.material.instanceId, 'number')

    const gameObjectName = `SdkMaterialAssign_${Date.now()}`
    const gameObject = await client.gameObjectCreate({
      name: gameObjectName,
      dimension: '3d',
      primitive: 'cube',
    })
    createdGameObjectInstanceIds.push(gameObject.instanceId)

    const assigned = await client.materialAssign({
      instanceId: gameObject.instanceId,
      assetPath,
      slot: 0,
    })

    assert.equal(assigned.targetInstanceId, gameObject.instanceId)
    assert.equal(assigned.targetPath, gameObject.path)
    assert.equal(assigned.rendererIndex, 0)
    assert.equal(assigned.slot, 0)
    assert.equal(assigned.material.assetPath, assetPath)

    const properties = await client.materialPropertiesGet({ assetPath })
    assert.equal(properties.material.assetPath, assetPath)
    assert.ok(Object.keys(properties.properties).length > 0)

    const values: Record<string, unknown> = {}
    const colorEntry = Object.entries(properties.properties).find(([, value]) => value.type === 'color')
    const floatEntry = Object.entries(properties.properties).find(([, value]) => value.type === 'float')
    const rangeEntry = Object.entries(properties.properties).find(([, value]) => value.type === 'range')
    const intEntry = Object.entries(properties.properties).find(([, value]) => value.type === 'int')
    const vectorEntry = Object.entries(properties.properties).find(([, value]) => value.type === 'vector')
    const textureEntry = Object.entries(properties.properties).find(([, value]) => value.type === 'texture')

    if (colorEntry != null) {
      values[colorEntry[0]] = {
        type: 'color',
        value: { r: 0.9, g: 0.3, b: 0.2, a: 1 },
      }
    }

    if (floatEntry != null) {
      values[floatEntry[0]] = {
        type: 'float',
        value: 0.42,
      }
    }
    if (rangeEntry != null) {
      values[rangeEntry[0]] = {
        type: 'range',
        value: 0.33,
      }
    }
    if (intEntry != null) {
      values[intEntry[0]] = {
        type: 'int',
        value: 2,
      }
    }

    if (vectorEntry != null) {
      values[vectorEntry[0]] = {
        type: 'vector',
        value: { x: 1, y: 2, z: 3, w: 4 },
      }
    }

    let textureAssetPath: string | undefined
    if (textureEntry != null) {
      textureAssetPath = `Assets/__TempTests__/SdkMaterialTexture_${Date.now()}.asset`
      createdAssetPaths.push(textureAssetPath)
      await client.execute(
        `var tex = new UnityEngine.Texture2D(2,2); tex.SetPixel(0,0,UnityEngine.Color.green); tex.Apply(); UnityEditor.AssetDatabase.CreateAsset(tex, "${textureAssetPath}"); UnityEditor.AssetDatabase.SaveAssets();`,
      )

      values[textureEntry[0]] = {
        type: 'texture',
        assetPath: textureAssetPath,
      }
    }

    assert.ok(Object.keys(values).length > 0)
    const setResult = await client.materialPropertiesSet({
      assetPath,
      values: values as Record<string, { type: string; value?: unknown; assetPath?: string | null }>,
      strict: true,
    })
    assert.equal(setResult.material.assetPath, assetPath)
    assert.equal(setResult.ignoredProperties.length, 0)
    assert.ok(setResult.appliedProperties.length > 0)

    const updated = await client.materialPropertiesGet({
      assetPath,
      names: Object.keys(values),
    })
    if (colorEntry != null) {
      const colorValue = updated.properties[colorEntry[0]]
      assert.equal(colorValue.type, 'color')
    }
    if (floatEntry != null) {
      const floatValue = updated.properties[floatEntry[0]]
      assert.equal(floatValue.type, 'float')
      assert.equal(typeof floatValue.value, 'number')
    }
    if (rangeEntry != null) {
      const rangeValue = updated.properties[rangeEntry[0]]
      assert.equal(rangeValue.type, 'range')
      assert.equal(typeof rangeValue.value, 'number')
    }
    if (intEntry != null) {
      const intValue = updated.properties[intEntry[0]]
      assert.equal(intValue.type, 'int')
      assert.equal(typeof intValue.value, 'number')
    }
    if (vectorEntry != null) {
      const vectorValue = updated.properties[vectorEntry[0]]
      assert.equal(vectorValue.type, 'vector')
    }
    if (textureEntry != null && textureAssetPath != null) {
      const textureValue = updated.properties[textureEntry[0]]
      assert.equal(textureValue.type, 'texture')
      assert.equal(textureValue.assetPath, textureAssetPath)
    }
  })

})
