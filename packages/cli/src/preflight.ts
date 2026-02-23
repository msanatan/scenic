import { findUnityProject, isPluginInstalled } from '@scenicai/sdk'

export function resolveCommandProject(
  opts: { project?: string; execute?: boolean },
  config: { requirePlugin: boolean; requiresExecute?: boolean },
): string {
  let projectPath: string
  try {
    projectPath = findUnityProject(opts.project)
  } catch {
    throw new Error('Unity project not found. Run inside a Unity project or pass --project <path>.')
  }

  if (config.requiresExecute && opts.execute === false) {
    throw new Error('Execute is disabled for this invocation (--no-execute).')
  }

  if (config.requirePlugin && !isPluginInstalled(projectPath)) {
    throw new Error('com.msanatan.scenic is not installed. Run `scenic init` (or `scenic update`) first.')
  }

  return projectPath
}
