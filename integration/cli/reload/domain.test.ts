import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../helpers/cli-runner.ts'

describe('CLI: domain', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('triggers a domain reload', async () => {
    const payload = (await runCli('domain', 'reload')) as {
      success: boolean
      result?: {
        triggered: boolean
      }
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(payload.result?.triggered, true)
  })
})
