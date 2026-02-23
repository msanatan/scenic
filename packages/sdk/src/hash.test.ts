import { describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { pipePath, projectHash, stateDir } from './hash.ts'

describe('projectHash', () => {
  it('returns a 12-char hex string', () => {
    const hash = projectHash('/Users/me/MyGame')
    assert.equal(hash.length, 12)
    assert.match(hash, /^[a-f0-9]{12}$/)
  })

  it('is deterministic', () => {
    const a = projectHash('/Users/me/MyGame')
    const b = projectHash('/Users/me/MyGame')
    assert.equal(a, b)
  })

  it('differs for different paths', () => {
    const a = projectHash('/Users/me/GameA')
    const b = projectHash('/Users/me/GameB')
    assert.notEqual(a, b)
  })

  it('normalizes trailing slash differences', () => {
    const a = projectHash('/Users/me/MyGame')
    const b = projectHash('/Users/me/MyGame/')
    assert.equal(a, b)
  })

  it('handles paths with spaces', () => {
    const hash = projectHash('/Users/me/My Game')
    assert.match(hash, /^[a-f0-9]{12}$/)
  })
})

describe('pipePath', () => {
  it('returns a unix socket path on non-Windows', () => {
    const path = pipePath('/Users/me/MyGame')
    assert.match(path, /^\/tmp\/scenic\/[a-f0-9]{12}\/bridge\.sock$/)
  })
})

describe('stateDir', () => {
  it('returns the temp directory for the project', () => {
    const dir = stateDir('/Users/me/MyGame')
    assert.match(dir, /^\/tmp\/scenic\/[a-f0-9]{12}$/)
  })
})
