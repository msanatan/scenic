import { describe, it, mock } from 'node:test'
import assert from 'node:assert/strict'
import { handleExecute } from './execute.ts'

describe('CLI execute', () => {
  it('outputs raw result by default', async () => {
    const logs: string[] = []
    const mockConsole = {
      log: (s: string) => logs.push(s),
      error: (_s: string) => undefined,
    }
    const mockExecute = mock.fn(async () => 'Player')

    await handleExecute('Selection.activeGameObject?.name', { timeout: '30000' }, {
      execute: mockExecute,
      console: mockConsole,
    })

    assert.equal(logs[0], 'Player')
  })

  it('outputs JSON envelope with --json flag', async () => {
    const logs: string[] = []
    const mockConsole = {
      log: (s: string) => logs.push(s),
      error: (_s: string) => undefined,
    }
    const mockExecute = mock.fn(async () => 'Player')

    await handleExecute('code', { json: true, timeout: '30000' }, {
      execute: mockExecute,
      console: mockConsole,
    })

    const parsed = JSON.parse(logs[0] ?? '{}')
    assert.equal(parsed.success, true)
    assert.equal(parsed.result, 'Player')
  })

  it('outputs JSON error with --json flag on failure', async () => {
    const logs: string[] = []
    const mockConsole = {
      log: (s: string) => logs.push(s),
      error: (s: string) => logs.push(s),
    }
    const mockExecute = mock.fn(async () => {
      throw new Error('Compilation failed')
    })

    await handleExecute('bad', { json: true, timeout: '30000' }, {
      execute: mockExecute,
      console: mockConsole,
      exit: (_code) => undefined,
    })

    const parsed = JSON.parse(logs[0] ?? '{}')
    assert.equal(parsed.success, false)
    assert.equal(parsed.error, 'Compilation failed')
  })
})
