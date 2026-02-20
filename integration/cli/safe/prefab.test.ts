import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { getCliEntrypoint, runCli } from '../../helpers/cli-runner.ts'

describe('CLI: prefab', () => {
  const createdNames: string[] = []
  const prefabPaths: string[] = []

  before(async () => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )

    await runCli('execute', 'if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/__TempTests__")) { UnityEditor.AssetDatabase.CreateFolder("Assets", "__TempTests__"); }')
  })

  after(async () => {
    for (const name of createdNames) {
      await runCli('execute', `var go = UnityEngine.GameObject.Find("${name}"); if (go != null) UnityEngine.Object.DestroyImmediate(go);`)
    }

    for (const prefabPath of prefabPaths) {
      await runCli('execute', `UnityEditor.AssetDatabase.DeleteAsset("${prefabPath}");`)
    }

    await runCli('execute', 'if (UnityEditor.AssetDatabase.IsValidFolder("Assets/__TempTests__")) { UnityEditor.AssetDatabase.DeleteAsset("Assets/__TempTests__"); }')
  })

  it('instantiates a prefab into the active scene', async () => {
    const marker = `${Date.now()}`
    const sourceName = `CliPrefabSource_${marker}`
    const prefabPath = `Assets/__TempTests__/CliPrefab_${marker}.prefab`
    prefabPaths.push(prefabPath)

    await runCli('execute', `var go = new UnityEngine.GameObject("${sourceName}"); UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, "${prefabPath}"); UnityEngine.Object.DestroyImmediate(go);`)

    const payload = (await runCli(
      'prefab',
      'instantiate',
      prefabPath,
      '--position',
      '1,2,3',
    )) as {
      success: boolean
      result?: {
        prefabPath: string
        name: string
        path: string
        instanceId: number
        siblingIndex: number
        isActive: boolean
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.prefabPath, prefabPath)
    assert.equal(typeof payload.result?.name, 'string')
    assert.ok((payload.result?.path ?? '').endsWith(`/${payload.result?.name}`))
    assert.notEqual(payload.result?.instanceId, 0)

    if (payload.result?.name != null) {
      createdNames.push(payload.result.name)
    }
  })

  it('saves a scene hierarchy to a prefab asset', async () => {
    const marker = `${Date.now()}`
    const rootName = `CliPrefabSaveRoot_${marker}`
    const childName = `${rootName}_Child`
    const prefabPath = `Assets/__TempTests__/CliSaved_${marker}.prefab`
    prefabPaths.push(prefabPath)
    createdNames.push(rootName, childName)

    const createdRoot = (await runCli('gameobject', 'create', rootName, '--dimension', '3d')) as {
      success: boolean
      result?: { instanceId: number }
      error?: string
    }
    assert.equal(createdRoot.success, true)

    const rootInstanceId = createdRoot.result?.instanceId
    assert.equal(typeof rootInstanceId, 'number')

    const createdChild = (await runCli('gameobject', 'create', childName, '--dimension', '3d', '--parent-instance-id', String(rootInstanceId))) as {
      success: boolean
      result?: { instanceId: number }
      error?: string
    }
    assert.equal(createdChild.success, true)

    const payload = (await runCli(
      'prefab',
      'save',
      prefabPath,
      '--instance-id',
      String(rootInstanceId),
    )) as {
      success: boolean
      result?: {
        prefabPath: string
        sourceName: string
        sourcePath: string
        sourceInstanceId: number
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.prefabPath, prefabPath)
    assert.equal(payload.result?.sourceName, rootName)
    assert.equal(payload.result?.sourceInstanceId, rootInstanceId)

    const validate = (await runCli(
      'execute',
      `var loaded = UnityEditor.PrefabUtility.LoadPrefabContents("${prefabPath}"); var exists = loaded.transform.Find("${childName}") != null; UnityEditor.PrefabUtility.UnloadPrefabContents(loaded); exists;`,
    )) as {
      success: boolean
      result?: unknown
      error?: string
    }

    assert.equal(validate.success, true)
    assert.equal(validate.result, true)
  })
})
