import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../packages/sdk/src/index.ts'
import { createTestClient } from '../helpers/sdk-client.ts'

describe('SDK: components', () => {
  let client: UniBridgeClient
  const createdNames: string[] = []

  before(() => {
    client = createTestClient()
  })

  after(async () => {
    for (const name of createdNames) {
      await client.execute(
        `var go = UnityEngine.GameObject.Find("${name}"); if (go != null) UnityEngine.Object.DestroyImmediate(go);`,
      )
    }
    client.close()
  })

  it('lists components with pagination and filtering', async () => {
    const name = `SdkComponents_${Date.now()}`
    createdNames.push(name)

    const created = await client.gameObjectCreate({ name, dimension: '3d' })
    await client.execute(
      `var go = UnityEngine.GameObject.Find("${name}"); go.AddComponent<UnityEngine.Rigidbody>(); go.AddComponent<UnityEngine.BoxCollider>();`,
    )

    const page = await client.componentsList({
      instanceId: created.instanceId,
      limit: 10,
      offset: 0,
    })
    assert.equal(page.limit, 10)
    assert.equal(page.offset, 0)
    assert.ok(page.total >= 3)
    assert.ok(page.components.some((component) => component.type.includes('Transform')))
    assert.ok(page.components.some((component) => component.type.includes('Rigidbody')))
    assert.ok(page.components.some((component) => component.type.includes('BoxCollider')))

    const filtered = await client.componentsList({
      instanceId: created.instanceId,
      type: 'Rigidbody',
      limit: 10,
      offset: 0,
    })
    assert.ok(filtered.components.length >= 1)
    for (const component of filtered.components) {
      assert.ok(component.type.includes('Rigidbody'))
    }
  })

  it('adds a component with initial values', async () => {
    const name = `SdkComponentsAdd_${Date.now()}`
    createdNames.push(name)

    const created = await client.gameObjectCreate({ name, dimension: '3d' })
    const added = await client.componentsAdd({
      instanceId: created.instanceId,
      type: 'UnityEngine.Rigidbody',
      initialValues: {
        mass: 5.5,
        useGravity: false,
      },
      strict: true,
    })

    assert.ok(added.type.includes('Rigidbody'))
    assert.ok(added.appliedFields.includes('mass'))
    assert.ok(added.appliedFields.includes('useGravity'))
    assert.equal(added.ignoredFields.length, 0)

    const values = await client.execute(
      `var go = UnityEngine.GameObject.Find("${name}"); var rb = go.GetComponent<UnityEngine.Rigidbody>(); rb.mass + "|" + rb.useGravity`,
    )
    assert.equal(values, '5.5|False')
  })

  it('gets and serializes a component', async () => {
    const name = `SdkComponentsGet_${Date.now()}`
    createdNames.push(name)

    const created = await client.gameObjectCreate({ name, dimension: '3d' })
    const added = await client.componentsAdd({
      instanceId: created.instanceId,
      type: 'UnityEngine.Rigidbody',
      initialValues: {
        mass: 8.25,
      },
    })

    const result = await client.componentsGet({
      instanceId: created.instanceId,
      componentInstanceId: added.instanceId,
    })

    assert.equal(result.component.instanceId, added.instanceId)
    assert.ok(result.component.type.includes('Rigidbody'))
    assert.equal(typeof result.component.index, 'number')
    assert.equal(typeof result.component.serialized, 'object')
    assert.notEqual(result.component.serialized, null)

    const serialized = result.component.serialized as Record<string, unknown>
    const serializedMass = typeof serialized['m_Mass'] === 'number'
      ? serialized['m_Mass']
      : typeof serialized.mass === 'number'
        ? serialized.mass
        : undefined
    if (serializedMass != null) {
      assert.equal(serializedMass, 8.25)
    }

    const actualMass = await client.execute(
      `var go = UnityEngine.GameObject.Find("${name}"); var rb = go.GetComponent<UnityEngine.Rigidbody>(); rb.mass`,
    )
    assert.equal(actualMass, 8.25)
  })

  it('removes a component', async () => {
    const name = `SdkComponentsRemove_${Date.now()}`
    createdNames.push(name)

    const created = await client.gameObjectCreate({ name, dimension: '3d' })
    const added = await client.componentsAdd({
      instanceId: created.instanceId,
      type: 'UnityEngine.Rigidbody',
    })

    const removed = await client.componentsRemove({
      instanceId: created.instanceId,
      componentInstanceId: added.instanceId,
    })
    assert.equal(removed.removed, true)
    assert.equal(removed.instanceId, added.instanceId)
    assert.ok(removed.type.includes('Rigidbody'))

    const exists = await client.execute(
      `var go = UnityEngine.GameObject.Find("${name}"); go.GetComponent<UnityEngine.Rigidbody>() != null`,
    )
    assert.equal(exists, false)
  })
})
