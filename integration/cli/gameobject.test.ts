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
})
