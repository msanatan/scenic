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

  it('gets and serializes a component', async () => {
    const name = `CliComponentsGet_${Date.now()}`
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
      '{"mass":8.25}',
    )) as {
      success: boolean
      result?: {
        instanceId: number
      }
    }
    assert.equal(addPayload.success, true)

    const getPayload = (await runCli(
      'components',
      'get',
      '--instance-id',
      String(createPayload.result?.instanceId),
      '--component-instance-id',
      String(addPayload.result?.instanceId),
    )) as {
      success: boolean
      result?: {
        component: {
          instanceId: number
          type: string
          index: number
          serialized: Record<string, unknown>
        }
      }
    }
    assert.equal(getPayload.success, true)
    assert.equal(getPayload.result?.component.instanceId, addPayload.result?.instanceId)
    assert.ok((getPayload.result?.component.type ?? '').includes('Rigidbody'))
    assert.equal(typeof getPayload.result?.component.index, 'number')

    const serialized = getPayload.result?.component.serialized ?? {}
    const serializedMass = typeof serialized['m_Mass'] === 'number'
      ? serialized['m_Mass']
      : typeof serialized.mass === 'number'
        ? serialized.mass
        : undefined
    if (serializedMass != null) {
      assert.equal(serializedMass, 8.25)
    }

    const massPayload = (await runCli(
      'execute',
      `var go = UnityEngine.GameObject.Find("${name}"); var rb = go.GetComponent<UnityEngine.Rigidbody>(); rb.mass`,
    )) as {
      success: boolean
      result?: unknown
    }
    assert.equal(massPayload.success, true)
    assert.equal(massPayload.result, 8.25)
  })

  it('removes a component', async () => {
    const name = `CliComponentsRemove_${Date.now()}`
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
    )) as {
      success: boolean
      result?: {
        instanceId: number
        type: string
      }
    }
    assert.equal(addPayload.success, true)

    const removePayload = (await runCli(
      'components',
      'remove',
      '--instance-id',
      String(createPayload.result?.instanceId),
      '--component-instance-id',
      String(addPayload.result?.instanceId),
    )) as {
      success: boolean
      result?: {
        removed: boolean
        instanceId: number
        type: string
        index: number
      }
    }
    assert.equal(removePayload.success, true)
    assert.equal(removePayload.result?.removed, true)
    assert.equal(removePayload.result?.instanceId, addPayload.result?.instanceId)
    assert.ok((removePayload.result?.type ?? '').includes('Rigidbody'))

    const existsPayload = (await runCli(
      'execute',
      `var go = UnityEngine.GameObject.Find("${name}"); go.GetComponent<UnityEngine.Rigidbody>() != null`,
    )) as {
      success: boolean
      result?: unknown
    }
    assert.equal(existsPayload.success, true)
    assert.equal(existsPayload.result, false)
  })

  it('updates a component', async () => {
    const name = `CliComponentsUpdate_${Date.now()}`
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
    )) as {
      success: boolean
      result?: {
        instanceId: number
      }
    }
    assert.equal(addPayload.success, true)

    const updatePayload = (await runCli(
      'components',
      'update',
      '--instance-id',
      String(createPayload.result?.instanceId),
      '--component-instance-id',
      String(addPayload.result?.instanceId),
      '--values',
      '{"mass":12.5,"useGravity":false}',
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

    assert.equal(updatePayload.success, true)
    assert.equal(updatePayload.result?.instanceId, addPayload.result?.instanceId)
    assert.ok((updatePayload.result?.type ?? '').includes('Rigidbody'))
    assert.ok(updatePayload.result?.appliedFields.includes('mass'))
    assert.ok(updatePayload.result?.appliedFields.includes('useGravity'))
    assert.equal(updatePayload.result?.ignoredFields.length, 0)

    const valuesPayload = (await runCli(
      'execute',
      `var go = UnityEngine.GameObject.Find("${name}"); var rb = go.GetComponent<UnityEngine.Rigidbody>(); rb.mass + "|" + rb.useGravity`,
    )) as {
      success: boolean
      result?: unknown
    }
    assert.equal(valuesPayload.success, true)
    assert.equal(valuesPayload.result, '12.5|False')
  })
})
