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
  })

})
