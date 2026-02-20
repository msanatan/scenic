import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: prefab', () => {
  let client: UniBridgeClient
  const createdNames: string[] = []
  const prefabPaths: string[] = []

  before(async () => {
    client = createTestClient()
    await client.execute('if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/__TempTests__")) { UnityEditor.AssetDatabase.CreateFolder("Assets", "__TempTests__"); }')
  })

  after(async () => {
    for (const name of createdNames) {
      await client.execute(`var go = UnityEngine.GameObject.Find("${name}"); if (go != null) UnityEngine.Object.DestroyImmediate(go);`)
    }

    for (const prefabPath of prefabPaths) {
      await client.execute(`UnityEditor.AssetDatabase.DeleteAsset("${prefabPath}");`)
    }

    await client.execute('if (UnityEditor.AssetDatabase.IsValidFolder("Assets/__TempTests__")) { UnityEditor.AssetDatabase.DeleteAsset("Assets/__TempTests__"); }')

    client.close()
  })

  it('instantiates a prefab into the active scene', async () => {
    const marker = `${Date.now()}`
    const sourceName = `SdkPrefabSource_${marker}`
    const prefabPath = `Assets/__TempTests__/SdkPrefab_${marker}.prefab`
    prefabPaths.push(prefabPath)

    await client.execute(`var go = new UnityEngine.GameObject("${sourceName}"); UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, "${prefabPath}"); UnityEngine.Object.DestroyImmediate(go);`)

    const result = await client.prefabInstantiate({
      prefabPath,
      transform: {
        space: 'local',
        position: { x: 4, y: 5, z: 6 },
      },
    })

    createdNames.push(result.name)

    assert.equal(result.prefabPath, prefabPath)
    assert.ok(result.path.endsWith(`/${result.name}`))
    assert.notEqual(result.instanceId, 0)
    assert.equal(result.transform.position.x, 4)
    assert.equal(result.transform.position.y, 5)
    assert.equal(result.transform.position.z, 6)
  })

  it('saves a scene hierarchy to a prefab asset', async () => {
    const marker = `${Date.now()}`
    const rootName = `SdkPrefabSaveRoot_${marker}`
    const childName = `${rootName}_Child`
    const prefabPath = `Assets/__TempTests__/SdkSaved_${marker}.prefab`
    prefabPaths.push(prefabPath)
    createdNames.push(rootName, childName)

    const root = await client.gameObjectCreate({ name: rootName, dimension: '3d' })
    await client.gameObjectCreate({ name: childName, parentInstanceId: root.instanceId, dimension: '3d' })

    const saved = await client.prefabSave({
      instanceId: root.instanceId,
      prefabPath,
    })

    assert.equal(saved.prefabPath, prefabPath)
    assert.equal(saved.sourceName, rootName)
    assert.equal(saved.sourceInstanceId, root.instanceId)

    const hasChild = await client.execute(`var loaded = UnityEditor.PrefabUtility.LoadPrefabContents("${prefabPath}"); var exists = loaded.transform.Find("${childName}") != null; UnityEditor.PrefabUtility.UnloadPrefabContents(loaded); exists;`)
    assert.equal(hasChild, true)
  })
})
