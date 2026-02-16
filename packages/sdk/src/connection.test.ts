import { afterEach, describe, it } from 'node:test'
import assert from 'node:assert/strict'
import net from 'node:net'
import fs from 'node:fs'
import os from 'node:os'
import path from 'node:path'
import { PipeConnection } from './connection.ts'
import { stateDir } from './hash.ts'

const SOCK_PATH = '/tmp/unibridge-test.sock'

function createMockServer(handler: (data: Buffer) => Buffer) {
  const sockets = new Set<net.Socket>()
  const server = net.createServer((socket) => {
    sockets.add(socket)
    socket.on('close', () => {
      sockets.delete(socket)
    })
    let buffer = Buffer.alloc(0)
    socket.on('data', (chunk: Buffer | string) => {
      const data = typeof chunk === 'string' ? Buffer.from(chunk) : chunk
      buffer = Buffer.concat([buffer, data])
      while (buffer.length >= 4) {
        const len = buffer.readUInt32BE(0)
        if (buffer.length < 4 + len) break
        const msg = buffer.subarray(4, 4 + len)
        buffer = buffer.subarray(4 + len)
        const response = handler(msg)
        const frame = Buffer.alloc(4 + response.length)
        frame.writeUInt32BE(response.length, 0)
        response.copy(frame, 4)
        socket.write(frame)
      }
    })
  })

  try {
    fs.unlinkSync(SOCK_PATH)
  } catch {
    // ignore missing socket
  }

  server.listen(SOCK_PATH)
  const closeWithClients = server.close.bind(server)
    ; (server as net.Server & { close: net.Server['close'] }).close = ((callback?: (err?: Error) => void) => {
      for (const socket of sockets) {
        socket.destroy()
      }
      sockets.clear()
      return closeWithClients(callback)
    }) as net.Server['close']
  return server
}

describe('PipeConnection', () => {
  let server: net.Server | undefined
  let conn: PipeConnection | undefined
  let projectPath: string | undefined

  afterEach(async () => {
    conn?.disconnect()
    if (server) {
      await new Promise<void>((resolve) => server?.close(() => resolve()))
    }
    try {
      fs.unlinkSync(SOCK_PATH)
    } catch {
      // ignore missing socket
    }
    if (projectPath) {
      fs.rmSync(projectPath, { recursive: true, force: true })
      projectPath = undefined
    }
  })

  it('connects and sends/receives a message', async () => {
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      return Buffer.from(JSON.stringify({
        id: req.id,
        success: true,
        result: 'hello',
      }))
    })

    conn = new PipeConnection()
    await conn.connect(SOCK_PATH)
    const res = await conn.send({
      id: 'cmd-1',
      command: 'execute',
      params: { code: 'test' },
    })

    assert.equal(res.success, true)
    assert.equal(res.result, 'hello')
  })

  it('deletes persisted result file after socket response is processed', async () => {
    projectPath = fs.mkdtempSync(path.join(os.tmpdir(), 'unibridge-project-'))
    const stateDirectory = stateDir(projectPath)
    fs.mkdirSync(path.join(stateDirectory, 'results'), { recursive: true })

    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      const resultPath = path.join(stateDirectory, 'results', `${req.id}.json`)
      fs.writeFileSync(resultPath, JSON.stringify({
        id: req.id,
        success: true,
        result: 'socket-path',
        error: null,
      }))

      return Buffer.from(JSON.stringify({
        id: req.id,
        success: true,
        result: 'socket-path',
      }))
    })

    conn = new PipeConnection({ projectPath })
    await conn.connect(SOCK_PATH)
    const response = await conn.send({
      id: 'cmd-socket-cleanup',
      command: 'execute',
      params: { code: 'Debug.Log("x")' },
    })

    assert.equal(response.success, true)
    assert.equal(response.result, 'socket-path')
    const resultPath = path.join(stateDirectory, 'results', 'cmd-socket-cleanup.json')
    assert.equal(fs.existsSync(resultPath), false)
  })

  it('matches responses to requests by ID', async () => {
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      return Buffer.from(JSON.stringify({
        id: req.id,
        success: true,
        result: req.id,
      }))
    })

    conn = new PipeConnection()
    await conn.connect(SOCK_PATH)
    const [a, b] = await Promise.all([
      conn.send({ id: 'cmd-a', command: 'execute', params: {} }),
      conn.send({ id: 'cmd-b', command: 'execute', params: {} }),
    ])

    assert.equal(a.result, 'cmd-a')
    assert.equal(b.result, 'cmd-b')
  })

  it('reconnects after server restarts', async () => {
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      return Buffer.from(JSON.stringify({ id: req.id, success: true, result: 'ok' }))
    })

    conn = new PipeConnection({ reconnectTimeout: 5000 })
    await conn.connect(SOCK_PATH)

    await new Promise<void>((resolve) => server?.close(() => resolve()))
    try {
      fs.unlinkSync(SOCK_PATH)
    } catch {
      // ignore missing socket
    }

    await new Promise((resolve) => setTimeout(resolve, 300))
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      return Buffer.from(JSON.stringify({
        id: req.id,
        success: true,
        result: 'reconnected',
      }))
    })

    const res = await conn.send({
      id: 'cmd-2',
      command: 'execute',
      params: {},
    })

    assert.equal(res.result, 'reconnected')
  })

  it('recovers pending response after reconnect without re-sending execute', async () => {
    projectPath = fs.mkdtempSync(path.join(os.tmpdir(), 'unibridge-project-'))
    const stateDirectory = stateDir(projectPath)
    fs.mkdirSync(path.join(stateDirectory, 'results'), { recursive: true })

    let sawExecute = false
    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      if (req.command === 'execute') {
        sawExecute = true
        setTimeout(() => {
          server?.close()
          try {
            fs.unlinkSync(SOCK_PATH)
          } catch {
            // ignore missing socket
          }
          server = createMockServer((nextMsg) => {
            const recoverReq = JSON.parse(nextMsg.toString())
            if (recoverReq.command === 'recoverResults') {
              const resultPath = path.join(stateDirectory, 'results', 'cmd-recover.json')
              fs.writeFileSync(resultPath, JSON.stringify({
                id: 'cmd-recover',
                success: true,
                result: 'recovered',
                error: null,
              }))
              return Buffer.from(JSON.stringify({
                id: recoverReq.id,
                success: true,
                result: JSON.stringify({
                  results: [
                    {
                      id: 'cmd-recover',
                      success: true,
                      result: 'recovered',
                      error: null,
                    },
                  ],
                }),
              }))
            }
            return Buffer.from(JSON.stringify({
              id: recoverReq.id,
              success: false,
              error: 'unexpected command',
            }))
          })
        }, 50)
        return Buffer.alloc(0)
      }

      return Buffer.from(JSON.stringify({
        id: req.id,
        success: false,
        error: 'unexpected command',
      }))
    })

    conn = new PipeConnection({ projectPath, commandTimeout: 2000, reconnectTimeout: 5000 })
    await conn.connect(SOCK_PATH)
    const response = await conn.send({
      id: 'cmd-recover',
      command: 'execute',
      params: { code: 'Debug.Log("x")' },
    })

    assert.equal(sawExecute, true)
    assert.equal(response.success, true)
    assert.equal(response.result, 'recovered')
    const recoveredResultPath = path.join(stateDirectory, 'results', 'cmd-recover.json')
    assert.equal(fs.existsSync(recoveredResultPath), false)
  })

  it('does NOT re-send commands on reconnect', async () => {
    let commandCount = 0
    server = createMockServer(() => {
      commandCount++
      return Buffer.alloc(0)
    })

    conn = new PipeConnection({ commandTimeout: 500, reconnectTimeout: 2000 })
    await conn.connect(SOCK_PATH)

    const promise = conn.send({
      id: 'cmd-reload',
      command: 'execute',
      params: {},
    })

    await assert.rejects(promise, /timeout/i)
    assert.equal(commandCount, 1)
  })

  it('fires connect timeout when no server', async () => {
    conn = new PipeConnection({ connectTimeout: 500 })
    await assert.rejects(conn.connect('/tmp/nonexistent.sock'), /Connect timeout/i)
  })

  it('returns file result when socket response is lost during reload window', async () => {
    projectPath = fs.mkdtempSync(path.join(os.tmpdir(), 'unibridge-project-'))
    const stateDirectory = stateDir(projectPath)
    fs.mkdirSync(path.join(stateDirectory, 'results'), { recursive: true })

    server = createMockServer((msg) => {
      const req = JSON.parse(msg.toString())
      if (req.command === 'execute') {
        setTimeout(() => {
          const resultPath = path.join(stateDirectory, 'results', `${req.id}.json`)
          fs.writeFileSync(resultPath, JSON.stringify({
            id: req.id,
            success: true,
            result: 'from-file',
            error: null,
          }))
        }, 100)
        return Buffer.alloc(0)
      }

      return Buffer.from(JSON.stringify({
        id: req.id,
        success: false,
        error: 'unexpected command',
      }))
    })

    conn = new PipeConnection({ projectPath, commandTimeout: 1000, reconnectTimeout: 1000 })
    await conn.connect(SOCK_PATH)

    const response = await conn.send({
      id: 'cmd-file-fallback',
      command: 'execute',
      params: { code: 'Debug.Log("x")' },
    })

    assert.equal(response.success, true)
    assert.equal(response.result, 'from-file')
    const resultPath = path.join(stateDirectory, 'results', 'cmd-file-fallback.json')
    assert.equal(fs.existsSync(resultPath), false)
  })
})
