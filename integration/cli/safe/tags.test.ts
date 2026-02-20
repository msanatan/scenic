import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../../helpers/cli-runner.ts'

describe('CLI: tags', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('returns tags including built-in markers', async () => {
    const payload = (await runCli('tags', 'get')) as {
      success: boolean
      result?: {
        tags: Array<{
          name: string
          isBuiltIn: boolean
        }>
        total: number
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(typeof payload.result?.total, 'number')
    assert.ok(Array.isArray(payload.result?.tags))
    assert.equal(payload.result?.total, payload.result?.tags.length)
    assert.ok((payload.result?.total ?? 0) > 0)

    const untagged = payload.result?.tags.find((tag) => tag.name === 'Untagged')
    assert.ok(untagged != null)
    assert.equal(untagged?.isBuiltIn, true)
  })
})
