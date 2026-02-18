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
  })
})
