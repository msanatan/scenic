import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { getCliEntrypoint, runCli } from '../helpers/cli-runner.ts'

describe('CLI: components', () => {
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

  it('lists components with pagination and type filter', async () => {
    const name = `CliComponents_${Date.now()}`
    createdNames.push(name)

    const createPayload = (await runCli('gameobject', 'create', name, '--dimension', '3d')) as {
      success: boolean
      result?: {
        instanceId: number
      }
    }
    assert.equal(createPayload.success, true)

    await runCli(
      'execute',
      `var go = UnityEngine.GameObject.Find("${name}"); go.AddComponent<UnityEngine.Rigidbody>(); go.AddComponent<UnityEngine.BoxCollider>();`,
    )

    const pagePayload = (await runCli(
      'components',
      'list',
      '--instance-id',
      String(createPayload.result?.instanceId),
      '--limit',
      '10',
      '--offset',
      '0',
    )) as {
      success: boolean
      result?: {
        components: Array<{
          type: string
          instanceId: number
          index: number
          enabled?: boolean | null
        }>
        total: number
        limit: number
        offset: number
      }
    }

    assert.equal(pagePayload.success, true)
    assert.equal(pagePayload.result?.limit, 10)
    assert.equal(pagePayload.result?.offset, 0)
    assert.ok((pagePayload.result?.total ?? 0) >= 3)
    assert.ok((pagePayload.result?.components ?? []).some((component) => component.type.includes('Transform')))
    assert.ok((pagePayload.result?.components ?? []).some((component) => component.type.includes('Rigidbody')))
    assert.ok((pagePayload.result?.components ?? []).some((component) => component.type.includes('BoxCollider')))

    const filteredPayload = (await runCli(
      'components',
      'list',
      '--instance-id',
      String(createPayload.result?.instanceId),
      '--type',
      'Rigidbody',
      '--limit',
      '10',
      '--offset',
      '0',
    )) as {
      success: boolean
      result?: {
        components: Array<{ type: string }>
      }
    }

    assert.equal(filteredPayload.success, true)
    assert.ok((filteredPayload.result?.components.length ?? 0) >= 1)
    for (const component of filteredPayload.result?.components ?? []) {
      assert.ok(component.type.includes('Rigidbody'))
    }
  })

  it('adds a component with initial values', async () => {
    const name = `CliComponentsAdd_${Date.now()}`
    createdNames.push(name)

    const createPayload = (await runCli('gameobject', 'create', name, '--dimension', '3d')) as {
      success: boolean
      result?: {
        instanceId: number
      }
    }
    assert.equal(createPayload.success, true)

    const addPayload = (await runCli(
      'components',
      'add',
      '--instance-id',
      String(createPayload.result?.instanceId),
      '--type',
      'UnityEngine.Rigidbody',
      '--values',
      '{"mass":5.5,"useGravity":false}',
      '--strict',
    )) as {
      success: boolean
      result?: {
        instanceId: number
        type: string
        appliedFields: string[]
        ignoredFields: string[]
      }
    }

    assert.equal(addPayload.success, true)
    assert.ok((addPayload.result?.type ?? '').includes('Rigidbody'))
    assert.ok(addPayload.result?.appliedFields.includes('mass'))
    assert.ok(addPayload.result?.appliedFields.includes('useGravity'))
    assert.equal(addPayload.result?.ignoredFields.length, 0)

    const valuesPayload = (await runCli(
      'execute',
      `var go = UnityEngine.GameObject.Find("${name}"); var rb = go.GetComponent<UnityEngine.Rigidbody>(); rb.mass + "|" + rb.useGravity`,
    )) as {
      success: boolean
      result?: unknown
    }
    assert.equal(valuesPayload.success, true)
    assert.equal(valuesPayload.result, '5.5|False')
  })
})
