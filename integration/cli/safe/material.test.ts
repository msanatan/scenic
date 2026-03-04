import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'

describe('CLI: material', () => {
  const createdAssetPaths: string[] = []
  const createdGameObjectInstanceIds: number[] = []

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
    for (const instanceId of createdGameObjectInstanceIds) {
      await runCli('gameobject', 'destroy', '--instance-id', String(instanceId))
    }
    await runCli('execute', 'UnityEditor.AssetDatabase.DeleteAsset("Assets/__TempTests__"); UnityEditor.AssetDatabase.SaveAssets();')
  })

  it('creates, gets, and assigns a material asset', async () => {
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

    const gameObjectName = `CliMaterialAssign_${Date.now()}`
    const createGoPayload = (await runCli(
      'gameobject',
      'create',
      gameObjectName,
      '--dimension',
      '3d',
      '--primitive',
      'cube',
    )) as {
      success: boolean
      result?: {
        path: string
        instanceId: number
      }
    }
    assert.equal(createGoPayload.success, true)
    assert.equal(typeof createGoPayload.result?.instanceId, 'number')
    if (createGoPayload.result?.instanceId != null) {
      createdGameObjectInstanceIds.push(createGoPayload.result.instanceId)
    }

    const assignPayload = (await runCli(
      'material',
      'assign',
      '--instance-id',
      String(createGoPayload.result?.instanceId),
      '--asset-path',
      assetPath,
      '--slot',
      '0',
    )) as {
      success: boolean
      result?: {
        targetPath: string
        targetInstanceId: number
        rendererIndex: number
        slot: number
        material: {
          assetPath: string
        }
      }
    }
    assert.equal(assignPayload.success, true)
    assert.equal(assignPayload.result?.targetPath, createGoPayload.result?.path)
    assert.equal(assignPayload.result?.targetInstanceId, createGoPayload.result?.instanceId)
    assert.equal(assignPayload.result?.rendererIndex, 0)
    assert.equal(assignPayload.result?.slot, 0)
    assert.equal(assignPayload.result?.material.assetPath, assetPath)

    const propsGetPayload = (await runCli('material', 'props', 'get', assetPath)) as {
      success: boolean
      result?: {
        material: {
          assetPath: string
        }
        properties: Record<string, {
          type: 'float' | 'range' | 'int' | 'color' | 'vector' | 'texture'
          value?: unknown
          assetPath?: string | null
        }>
      }
    }
    assert.equal(propsGetPayload.success, true)
    assert.equal(propsGetPayload.result?.material.assetPath, assetPath)
    const properties = propsGetPayload.result?.properties ?? {}
    assert.ok(Object.keys(properties).length > 0)

    const values: Record<string, unknown> = {}
    const colorEntry = Object.entries(properties).find(([, value]) => value.type === 'color')
    const floatEntry = Object.entries(properties).find(([, value]) => value.type === 'float')
    const rangeEntry = Object.entries(properties).find(([, value]) => value.type === 'range')
    const intEntry = Object.entries(properties).find(([, value]) => value.type === 'int')
    const vectorEntry = Object.entries(properties).find(([, value]) => value.type === 'vector')
    const textureEntry = Object.entries(properties).find(([, value]) => value.type === 'texture')

    if (colorEntry != null) {
      values[colorEntry[0]] = {
        type: 'color',
        value: { r: 0.1, g: 0.8, b: 0.2, a: 1 },
      }
    }

    if (floatEntry != null) {
      values[floatEntry[0]] = {
        type: 'float',
        value: 0.77,
      }
    }
    if (rangeEntry != null) {
      values[rangeEntry[0]] = {
        type: 'range',
        value: 0.66,
      }
    }
    if (intEntry != null) {
      values[intEntry[0]] = {
        type: 'int',
        value: 4,
      }
    }

    if (vectorEntry != null) {
      values[vectorEntry[0]] = {
        type: 'vector',
        value: { x: 9, y: 8, z: 7, w: 6 },
      }
    }

    let textureAssetPath: string | undefined
    if (textureEntry != null) {
      textureAssetPath = `Assets/__TempTests__/CliMaterialTexture_${Date.now()}.asset`
      createdAssetPaths.push(textureAssetPath)
      await runCli(
        'execute',
        `var tex = new UnityEngine.Texture2D(2,2); tex.SetPixel(0,0,UnityEngine.Color.blue); tex.Apply(); UnityEditor.AssetDatabase.CreateAsset(tex, "${textureAssetPath}"); UnityEditor.AssetDatabase.SaveAssets();`,
      )

      values[textureEntry[0]] = {
        type: 'texture',
        assetPath: textureAssetPath,
      }
    }

    assert.ok(Object.keys(values).length > 0)
    const setPayload = (await runCli(
      'material',
      'props',
      'set',
      assetPath,
      '--values',
      JSON.stringify(values),
      '--strict',
    )) as {
      success: boolean
      result?: {
        material: {
          assetPath: string
        }
        appliedProperties: string[]
        ignoredProperties: string[]
      }
    }
    assert.equal(setPayload.success, true)
    assert.equal(setPayload.result?.material.assetPath, assetPath)
    assert.equal(setPayload.result?.ignoredProperties.length, 0)
    assert.ok((setPayload.result?.appliedProperties.length ?? 0) > 0)
  })

})
