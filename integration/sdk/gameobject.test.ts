import { after, before, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../packages/sdk/src/index.ts'
import { createTestClient } from '../helpers/sdk-client.ts'

describe('SDK: gameobject', () => {
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

  it('creates a 2d object with SpriteRenderer', async () => {
    const name = `SdkGo2d_${Date.now()}`
    createdNames.push(name)

    const result = await client.gameObjectCreate({
      name,
      dimension: '2d',
      transform: {
        space: 'local',
        position: { x: 0, y: 1, z: 0 },
      },
    })

    assert.equal(result.name, name)
    assert.ok(result.path.endsWith(`/${name}`))
    assert.equal(typeof result.instanceId, 'number')
    assert.notEqual(result.instanceId, 0)
  })

  it('creates a 3d primitive', async () => {
    const name = `SdkGo3d_${Date.now()}`
    createdNames.push(name)

    const result = await client.gameObjectCreate({
      name,
      dimension: '3d',
      primitive: 'cube',
    })

    assert.equal(result.name, name)
    assert.ok(result.path.endsWith(`/${name}`))
    assert.equal(typeof result.instanceId, 'number')
    assert.notEqual(result.instanceId, 0)
  })

  it('destroys an object by instance id', async () => {
    const name = `SdkGoDestroy_${Date.now()}`

    const created = await client.gameObjectCreate({
      name,
      dimension: '3d',
      primitive: 'cube',
    })

    const destroyed = await client.gameObjectDestroy({ instanceId: created.instanceId })
    assert.equal(destroyed.destroyed, true)
    assert.equal(destroyed.instanceId, created.instanceId)

    const exists = await client.execute(`UnityEngine.GameObject.Find("${name}") != null`)
    assert.equal(exists, false)
  })

  it('updates gameobject properties by instance id', async () => {
    const originalName = `SdkGoUpdate_${Date.now()}`
    const updatedName = `${originalName}_Renamed`
    createdNames.push(originalName, updatedName)

    const created = await client.gameObjectCreate({
      name: originalName,
      dimension: '3d',
      primitive: 'cube',
    })

    const updated = await client.gameObjectUpdate({
      instanceId: created.instanceId,
      name: updatedName,
      tag: 'EditorOnly',
      layer: 'Default',
      isStatic: true,
      transform: {
        space: 'local',
        position: { x: 2, y: 3, z: 4 },
      },
    })

    assert.equal(updated.name, updatedName)
    assert.equal(updated.tag, 'EditorOnly')
    assert.equal(updated.layer, 'Default')
    assert.equal(updated.isStatic, true)
    assert.equal(updated.instanceId, created.instanceId)
    assert.equal(updated.transform.position.x, 2)
    assert.equal(updated.transform.position.y, 3)
    assert.equal(updated.transform.position.z, 4)
  })

  it('reparents a gameobject by instance id', async () => {
    const parentName = `SdkParent_${Date.now()}`
    const childName = `${parentName}_Child`
    const newParentName = `${parentName}_NewParent`
    createdNames.push(parentName, childName, newParentName)

    const parent = await client.gameObjectCreate({ name: parentName, dimension: '3d' })
    const child = await client.gameObjectCreate({
      name: childName,
      parentInstanceId: parent.instanceId,
      dimension: '3d',
    })
    const newParent = await client.gameObjectCreate({ name: newParentName, dimension: '3d' })

    const reparented = await client.gameObjectReparent({
      instanceId: child.instanceId,
      parentInstanceId: newParent.instanceId,
    })

    assert.equal(reparented.instanceId, child.instanceId)
    assert.equal(reparented.parentPath, `/${newParentName}`)
    assert.equal(reparented.path, `/${newParentName}/${childName}`)
  })

  it('gets gameobject info by instance id', async () => {
    const name = `SdkGoGet_${Date.now()}`
    createdNames.push(name)

    const created = await client.gameObjectCreate({ name, dimension: '3d' })
    const info = await client.gameObjectGet({ instanceId: created.instanceId })

    assert.equal(info.instanceId, created.instanceId)
    assert.equal(info.name, name)
    assert.equal(info.path, `/${name}`)
    assert.equal(typeof info.isActive, 'boolean')
    assert.equal(typeof info.siblingIndex, 'number')
    assert.equal(typeof info.transform.position.x, 'number')
  })

  it('finds gameobjects by query with pagination', async () => {
    const rootName = `SdkGoFindRoot_${Date.now()}`
    const childName = `${rootName}_Child`
    const inactiveName = `${rootName}_Inactive`
    createdNames.push(rootName, childName, inactiveName)

    const root = await client.gameObjectCreate({ name: rootName, dimension: '3d' })
    await client.gameObjectCreate({ name: childName, parentInstanceId: root.instanceId, dimension: '3d' })
    await client.gameObjectCreate({ name: inactiveName, dimension: '3d' })
    await client.execute(`var go = UnityEngine.GameObject.Find("${inactiveName}"); go.SetActive(false);`)

    const activeOnly = await client.gameObjectFind({
      query: rootName,
      limit: 10,
      offset: 0,
    })
    assert.ok(activeOnly.total >= 2)
    assert.ok(activeOnly.gameObjects.some((item) => item.name === rootName))
    assert.ok(activeOnly.gameObjects.some((item) => item.name === childName))
    assert.ok(!activeOnly.gameObjects.some((item) => item.name === inactiveName))

    const withInactive = await client.gameObjectFind({
      query: rootName,
      includeInactive: true,
      limit: 10,
      offset: 0,
    })
    assert.ok(withInactive.gameObjects.some((item) => item.name === inactiveName))
  })
})
