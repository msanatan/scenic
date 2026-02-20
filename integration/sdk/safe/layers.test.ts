import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { UniBridgeClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: layers', () => {
  let client: UniBridgeClient

  before(() => {
    client = createTestClient()
  })

  after(() => {
    client.close()
  })

  it('returns paginated layer slots', async () => {
    const result = await client.layersGet({ limit: 10, offset: 0 })

    assert.equal(result.limit, 10)
    assert.equal(result.offset, 0)
    assert.equal(result.total, 32)
    assert.equal(result.layers.length, 10)

    const first = result.layers[0]
    assert.equal(first.index, 0)
    assert.equal(typeof first.name, 'string')
    assert.equal(typeof first.isBuiltIn, 'boolean')
    assert.equal(typeof first.isUserEditable, 'boolean')
    assert.equal(typeof first.isOccupied, 'boolean')
    assert.equal(first.isBuiltIn, true)
    assert.equal(first.isUserEditable, false)
  })
})
