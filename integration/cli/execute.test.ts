import { describe, it, before } from 'node:test'
import assert from 'node:assert/strict'
import { existsSync } from 'node:fs'
import { runCli, getCliEntrypoint } from '../helpers/cli-runner.ts'

describe('CLI: execute', () => {
  before(() => {
    assert.ok(
      existsSync(getCliEntrypoint()),
      'CLI dist entrypoint not found. Run "npm run build:cli" before integration tests.',
    )
  })

  it('executes C# code and returns a result', async () => {
    const payload = (await runCli('execute', 'UnityEngine.Application.unityVersion')) as {
      success: boolean
      result?: unknown
      error?: string
    }

    assert.equal(payload.success, true)
    assert.equal(typeof payload.result, 'string')
    assert.ok((payload.result as string).length > 0)
  })
})
