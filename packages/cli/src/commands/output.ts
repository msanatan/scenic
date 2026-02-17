export interface CommandOutputDeps {
  console: Pick<Console, 'log' | 'error'>
  exit?: (code: number) => void
}

export async function runWithOutput<TResult>(
  jsonOutput: boolean,
  deps: CommandOutputDeps,
  execute: () => Promise<TResult>,
  renderText: (result: TResult, consoleLike: Pick<Console, 'log'>) => void,
): Promise<void> {
  try {
    const result = await execute()
    if (jsonOutput) {
      deps.console.log(JSON.stringify({ success: true, result }))
      return
    }

    renderText(result, deps.console)
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error)
    if (jsonOutput) {
      deps.console.log(JSON.stringify({ success: false, error: message }))
    } else {
      deps.console.error(`Error: ${message}`)
    }
    deps.exit?.(1)
  }
}
