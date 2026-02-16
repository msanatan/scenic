import { describe, it, mock } from 'node:test'
import assert from 'node:assert/strict'
import { handleInit } from './init.ts'

describe('CLI init', () => {
  it('logs project and plugin details', async () => {
    const logs: string[] = []
    const mockConsole = { log: (s: string) => logs.push(s) }
    const mockInit = mock.fn(async () => ({
      projectPath: '/tmp/MyGame',
      unityVersion: '2022.3.10f1',
      pluginVersion: '0.1.0',
      pluginSource: 'git' as const,
    }))

    await handleInit({}, { init: mockInit, console: mockConsole })

    assert.equal(logs[0], 'Found Unity 2022.3.10f1 at /tmp/MyGame')
    assert.equal(logs[1], 'Installed com.msanatan.unibridge@0.1.0')
    assert.equal(logs[2], 'Open Unity to load the plugin.')
  })
})
