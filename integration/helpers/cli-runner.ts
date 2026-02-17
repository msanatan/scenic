import path from 'node:path'
import { execFile } from 'node:child_process'
import { promisify } from 'node:util'

const execFileAsync = promisify(execFile)

export const projectPath = path.resolve(import.meta.dirname, '../../TestProjects/UniBridge3Dv6.3')
const cliEntrypoint = path.resolve(import.meta.dirname, '../../packages/cli/dist/index.js')

export function getCliEntrypoint(): string {
  return cliEntrypoint
}

export async function runCli(...args: string[]): Promise<unknown> {
  const { stdout } = await execFileAsync(
    process.execPath,
    [cliEntrypoint, ...args, '--json', '--project', projectPath],
    { encoding: 'utf-8' },
  )
  return parseLastJsonLine(stdout)
}

function parseLastJsonLine(text: string): unknown {
  const lines = text
    .split('\n')
    .map((line) => line.trim())
    .filter((line) => line.length > 0)
  if (lines.length === 0) {
    throw new Error('CLI produced no output.')
  }
  return JSON.parse(lines[lines.length - 1])
}
