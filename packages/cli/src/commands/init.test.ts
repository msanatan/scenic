import assert from 'node:assert/strict'
import { describe, it } from 'node:test'
import { handleInit } from './init.ts'

function createBaseDeps(overrides: Partial<Parameters<typeof handleInit>[2]> = {}) {
  const logs: string[] = []
  const errors: string[] = []
  const writes: Array<{ projectPath: string; enabled: boolean }> = []

  const deps = {
    init: async () => ({
      projectPath: '/tmp/project',
      unityVersion: '2022.3.10f1',
      pluginVersion: '1.2.3',
      pluginSource: 'git' as const,
      executeEnabled: false,
    }),
    createClient: () => ({
      settingsUpdate: async () => ({ executeEnabled: true }),
      close: () => {
        return
      },
    }),
    readExecuteEnabled: () => false,
    writeExecuteEnabled: (projectPath: string, enabled: boolean) => {
      writes.push({ projectPath, enabled })
    },
    console: {
      log: (message: string) => {
        logs.push(message)
      },
      error: (message: string) => {
        errors.push(message)
      },
    },
    ...overrides,
  }

  return { deps, logs, errors, writes }
}

describe('handleInit', () => {
  it('applies settings live when Unity is reachable', async () => {
    const { deps, logs, writes } = createBaseDeps({
      createClient: () => ({
        settingsUpdate: async ({ executeEnabled }) => ({ executeEnabled }),
        close: () => {
          return
        },
      }),
    })

    await handleInit({ enableExecute: true }, false, deps)

    assert.equal(writes.length, 0)
    assert.ok(logs.some((line) => line.includes('Settings: applied live + persisted')))
  })

  it('falls back to persisted settings when Unity is unreachable', async () => {
    const { deps, logs, writes } = createBaseDeps({
      createClient: () => ({
        settingsUpdate: async () => {
          throw new Error('Connect timeout (3s)')
        },
        close: () => {
          return
        },
      }),
    })

    await handleInit({ enableExecute: true }, false, deps)

    assert.deepEqual(writes, [{ projectPath: '/tmp/project', enabled: true }])
    assert.ok(logs.some((line) => line.includes('Settings: persisted for next Unity startup')))
  })

  it('falls back to persisted settings when older plugin lacks settings.update', async () => {
    const { deps, logs, writes } = createBaseDeps({
      createClient: () => ({
        settingsUpdate: async () => {
          throw new Error('Unknown command: settings.update')
        },
        close: () => {
          return
        },
      }),
    })

    await handleInit({ disableExecute: true }, false, deps)

    assert.deepEqual(writes, [{ projectPath: '/tmp/project', enabled: false }])
    assert.ok(logs.some((line) => line.includes('runtime apply is unavailable')))
  })

  it('does not fallback-persist when reachable plugin rejects update', async () => {
    const { deps, errors, writes } = createBaseDeps({
      createClient: () => ({
        settingsUpdate: async () => {
          throw new Error('params.executeEnabled is required.')
        },
        close: () => {
          return
        },
      }),
    })

    await handleInit({ enableExecute: true }, false, deps)

    assert.equal(writes.length, 0)
    assert.ok(errors.some((line) => line.includes('params.executeEnabled is required')))
  })
})
