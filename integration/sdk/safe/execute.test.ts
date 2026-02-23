import { describe, it, before, after } from 'node:test'
import assert from 'node:assert/strict'
import type { ScenicClient } from '../../../packages/sdk/src/index.ts'
import { createTestClient } from '../../helpers/sdk-client.ts'

describe('SDK: execute', () => {
  let client: ScenicClient

  before(() => {
    client = createTestClient()
  })
  after(() => {
    client.close()
  })

  it('executes C# code and returns a result', async () => {
    const result = await client.execute('UnityEngine.Application.unityVersion')
    assert.equal(typeof result, 'string')
    assert.ok(result.length > 0)
  })
})
