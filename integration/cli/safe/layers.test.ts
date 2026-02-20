import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'

describe('CLI: layers', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('returns paginated layer slots', async () => {
    const payload = (await runCli('layers', 'get', '--limit', '10', '--offset', '0')) as {
      success: boolean
      result?: {
        layers: Array<{
          index: number
          name: string
          isBuiltIn: boolean
          isUserEditable: boolean
          isOccupied: boolean
        }>
        total: number
        limit: number
        offset: number
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.limit, 10)
    assert.equal(payload.result?.offset, 0)
    assert.equal(payload.result?.total, 32)
    assert.equal(payload.result?.layers.length, 10)

    const first = payload.result?.layers[0]
    assert.equal(first?.index, 0)
    assert.equal(typeof first?.name, 'string')
    assert.equal(typeof first?.isBuiltIn, 'boolean')
    assert.equal(typeof first?.isUserEditable, 'boolean')
    assert.equal(typeof first?.isOccupied, 'boolean')
    assert.equal(first?.isBuiltIn, true)
    assert.equal(first?.isUserEditable, false)
  })
})
