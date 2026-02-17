import * as v from 'valibot'
import type { CommandRuntime, ExecuteGuard } from './runtime.ts'

export interface CommandDef<
  TMethod extends string,
  TArgs extends unknown[],
  TResult,
> {
  readonly method: TMethod
  readonly wire: string
  readonly params: (...args: TArgs) => Record<string, unknown>
  readonly result: v.GenericSchema<unknown, TResult>
  readonly guard?: 'execute'
}

export function defineCommand<
  TMethod extends string,
  TArgs extends unknown[],
  TResult,
>(def: CommandDef<TMethod, TArgs, TResult>): CommandDef<TMethod, TArgs, TResult> {
  return def
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type InferResult<T> = T extends CommandDef<string, any[], infer R> ? R : never

type CommandMethod<T> = T extends CommandDef<string, infer TArgs, infer TResult>
  ? (...args: TArgs) => Promise<TResult>
  : never

export type CommandMethods<T extends readonly CommandDef<string, any[], any>[]> = {
  [K in T[number] as K['method']]: CommandMethod<K>
}

export async function invokeCommand<TArgs extends unknown[], TResult>(
  def: CommandDef<string, TArgs, TResult>,
  runtime: CommandRuntime & ExecuteGuard,
  ...args: TArgs
): Promise<TResult> {
  if (def.guard === 'execute') {
    runtime.ensureExecuteEnabled()
  }
  const params = def.params(...args)
  const raw = await runtime.send(def.wire, params)
  return v.parse(def.result, raw)
}

export function buildClientMethods<
  const T extends readonly CommandDef<string, any[], any>[],
>(runtime: CommandRuntime & ExecuteGuard, defs: T): CommandMethods<T> {
  const methods: Record<string, (...args: unknown[]) => Promise<unknown>> = {}
  for (const def of defs) {
    methods[def.method] = (...args: unknown[]) => invokeCommand(def, runtime, ...args)
  }
  return methods as CommandMethods<T>
}
