import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'

describe('CLI: material', () => {
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

  it('creates and gets a material asset', async () => {
    await runCli(
      'execute',
      'if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/__TempTests__")) UnityEditor.AssetDatabase.CreateFolder("Assets", "__TempTests__");',
    )

    const assetPath = `Assets/__TempTests__/CliMaterial_${Date.now()}.mat`
    const expectedName = assetPath.replace(/^.*\//, '').replace(/\.mat$/i, '')
    createdAssetPaths.push(assetPath)

    const createPayload = (await runCli('material', 'create', assetPath)) as {
      success: boolean
      result?: {
        material: {
          assetPath: string
          name: string
          shader: string
          instanceId: number
        }
      }
      error?: string
    }

    assert.equal(createPayload.success, true)
    assert.equal(createPayload.result?.material.assetPath, assetPath)
    assert.equal(createPayload.result?.material.name, expectedName)
    assert.equal(typeof createPayload.result?.material.shader, 'string')
    assert.ok((createPayload.result?.material.shader.length ?? 0) > 0)

    const getPayload = (await runCli('material', 'get', assetPath)) as {
      success: boolean
      result?: {
        material: {
          assetPath: string
          name: string
          shader: string
          instanceId: number
        }
      }
      error?: string
    }

    assert.equal(getPayload.success, true)
    assert.equal(getPayload.result?.material.assetPath, assetPath)
    assert.equal(getPayload.result?.material.name, expectedName)
    assert.equal(getPayload.result?.material.shader, createPayload.result?.material.shader)
    assert.equal(typeof getPayload.result?.material.instanceId, 'number')
  })

})
