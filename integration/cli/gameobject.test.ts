import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { getCliEntrypoint, runCli } from '../helpers/cli-runner.ts'

describe('CLI: gameobject', () => {
  const createdNames: string[] = []

  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  after(async () => {
    for (const name of createdNames) {
      await runCli(
        'execute',
        `var go = UnityEngine.GameObject.Find("${name}"); if (go != null) UnityEngine.Object.DestroyImmediate(go);`,
      )
    }
  })

  it('creates a 2d object', async () => {
    const name = `CliGo2d_${Date.now()}`
    createdNames.push(name)

    const payload = (await runCli(
      'gameobject',
      'create',
      name,
      '--dimension',
      '2d',
      '--position',
      '0,1,0',
    )) as {
      success: boolean
      result?: {
        name: string
        path: string
        isActive: boolean
        siblingIndex: number
        instanceId: number
      }
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.name, name)
    assert.ok((payload.result?.path ?? '').endsWith(`/${name}`))
    assert.equal(typeof payload.result?.instanceId, 'number')
    assert.notEqual(payload.result?.instanceId, 0)
  })

  it('creates a 3d primitive', async () => {
    const name = `CliGo3d_${Date.now()}`
    createdNames.push(name)

    const payload = (await runCli(
      'gameobject',
      'create',
      name,
      '--dimension',
      '3d',
      '--primitive',
      'sphere',
    )) as {
      success: boolean
      result?: {
        name: string
        path: string
        isActive: boolean
        siblingIndex: number
        instanceId: number
      }
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.name, name)
    assert.ok((payload.result?.path ?? '').endsWith(`/${name}`))
    assert.equal(typeof payload.result?.instanceId, 'number')
    assert.notEqual(payload.result?.instanceId, 0)
  })

  it('destroys an object by instance id', async () => {
    const name = `CliGoDestroy_${Date.now()}`
    createdNames.push(name)

    const createPayload = (await runCli(
      'gameobject',
      'create',
      name,
      '--dimension',
      '3d',
      '--primitive',
      'cube',
    )) as {
      success: boolean
      result?: {
        instanceId: number
      }
    }

    assert.equal(createPayload.success, true)
    assert.equal(typeof createPayload.result?.instanceId, 'number')

    const destroyPayload = (await runCli(
      'gameobject',
      'destroy',
      '--instance-id',
      String(createPayload.result?.instanceId),
    )) as {
      success: boolean
      result?: {
        destroyed: boolean
        instanceId: number
      }
    }

    assert.equal(destroyPayload.success, true)
    assert.equal(destroyPayload.result?.destroyed, true)
    assert.equal(destroyPayload.result?.instanceId, createPayload.result?.instanceId)

    const existsPayload = (await runCli('execute', `UnityEngine.GameObject.Find("${name}") != null`)) as {
      success: boolean
      result?: unknown
    }
    assert.equal(existsPayload.success, true)
    assert.equal(existsPayload.result, false)
  })

  it('updates gameobject properties by instance id', async () => {
    const originalName = `CliGoUpdate_${Date.now()}`
    const updatedName = `${originalName}_Renamed`
    createdNames.push(originalName, updatedName)

    const createPayload = (await runCli(
      'gameobject',
      'create',
      originalName,
      '--dimension',
      '3d',
      '--primitive',
      'cube',
    )) as {
      success: boolean
      result?: {
        instanceId: number
      }
    }

    assert.equal(createPayload.success, true)

    const updatePayload = (await runCli(
      'gameobject',
      'update',
      '--instance-id',
      String(createPayload.result?.instanceId),
      '--name',
      updatedName,
      '--tag',
      'EditorOnly',
      '--layer',
      'Default',
      '--is-static',
      'true',
      '--position',
      '2,3,4',
    )) as {
      success: boolean
      result?: {
        name: string
        tag: string
        layer: string
        isStatic: boolean
        instanceId: number
        transform: {
          position: { x: number; y: number; z: number }
        }
      }
    }

    assert.equal(updatePayload.success, true)
    assert.equal(updatePayload.result?.name, updatedName)
    assert.equal(updatePayload.result?.tag, 'EditorOnly')
    assert.equal(updatePayload.result?.layer, 'Default')
    assert.equal(updatePayload.result?.isStatic, true)
    assert.equal(updatePayload.result?.instanceId, createPayload.result?.instanceId)
    assert.equal(updatePayload.result?.transform.position.x, 2)
    assert.equal(updatePayload.result?.transform.position.y, 3)
    assert.equal(updatePayload.result?.transform.position.z, 4)
  })
})
