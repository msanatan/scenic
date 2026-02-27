import { readFileSync, writeFileSync, existsSync, mkdirSync } from 'node:fs'
import path from 'node:path'

const ROOT = path.resolve(import.meta.dirname, '..')
const TEMPLATES = path.join(ROOT, '.claude', 'skills', 'add-feature', 'templates')

interface CommandNames {
  namespace: string
  action: string
  Namespace: string
  Action: string
  wireCommand: string
  sdkMethod: string
  SdkMethod: string
  guard: string
  requiresExecute: string
}

function capitalize(s: string): string {
  return s.charAt(0).toUpperCase() + s.slice(1)
}

function deriveNames(namespace: string, action: string, guardFlag: boolean): CommandNames {
  return {
    namespace,
    action,
    Namespace: capitalize(namespace),
    Action: capitalize(action),
    wireCommand: `${namespace}.${action}`,
    sdkMethod: `${namespace}${capitalize(action)}`,
    SdkMethod: `${capitalize(namespace)}${capitalize(action)}`,
    guard: guardFlag ? "\n  guard: 'execute'," : '',
    requiresExecute: guardFlag ? ', RequiresExecuteEnabled = true' : '',
  }
}

function applyTemplate(template: string, names: CommandNames): string {
  let result = template
  for (const [key, value] of Object.entries(names)) {
    result = result.replaceAll(`{{${key}}}`, value)
  }
  return result
}

function readTemplate(name: string): string {
  return readFileSync(path.join(TEMPLATES, name), 'utf-8')
}

function ensureDir(filePath: string): void {
  const dir = path.dirname(filePath)
  if (!existsSync(dir)) {
    mkdirSync(dir, { recursive: true })
  }
}

function writeIfNew(filePath: string, content: string, dryRun: boolean): 'created' | 'skipped' {
  const rel = path.relative(ROOT, filePath)
  if (existsSync(filePath)) {
    console.log(`  Skipped (exists): ${rel}`)
    return 'skipped'
  }
  if (dryRun) {
    console.log(`  Would create: ${rel}`)
    return 'created'
  }
  ensureDir(filePath)
  writeFileSync(filePath, content)
  console.log(`  Created: ${rel}`)
  return 'created'
}

function appendAtAnchor(
  filePath: string,
  anchor: string,
  content: string,
  dryRun: boolean,
): 'appended' | 'skipped' {
  const rel = path.relative(ROOT, filePath)
  if (!existsSync(filePath)) {
    return 'skipped'
  }
  const existing = readFileSync(filePath, 'utf-8')
  if (!existing.includes(anchor)) {
    console.log(`  Warning: anchor "${anchor}" not found in ${rel}`)
    return 'skipped'
  }
  if (dryRun) {
    console.log(`  Would append to: ${rel}`)
    return 'appended'
  }
  const updated = existing.replace(anchor, content)
  writeFileSync(filePath, updated)
  console.log(`  Appended: ${rel}`)
  return 'appended'
}

function scaffoldSdkContract(names: CommandNames, dryRun: boolean): void {
  const contractPath = path.join(ROOT, 'packages', 'sdk', 'src', 'commands', names.namespace, 'contract.ts')

  if (existsSync(contractPath)) {
    const appendTemplate = readTemplate('sdk-contract-append.ts.tmpl')
    const content = applyTemplate(appendTemplate, names)
    appendAtAnchor(contractPath, '// --- end ---', content, dryRun)
  } else {
    const template = readTemplate('sdk-contract.ts.tmpl')
    const content = applyTemplate(template, names)
    writeIfNew(contractPath, content, dryRun)
  }
}

function scaffoldCliCommand(names: CommandNames, dryRun: boolean): void {
  const cliPath = path.join(ROOT, 'packages', 'cli', 'src', 'commands', `${names.namespace}.ts`)

  if (existsSync(cliPath)) {
    // Append the new handler function before the register function
    const handlerTemplate = readTemplate('cli-command-append.ts.tmpl')
    const handlerContent = applyTemplate(handlerTemplate, names)

    // Append the new subcommand at the anchor inside register function
    const subcommandTemplate = readTemplate('cli-subcommand-append.ts.tmpl')
    const subcommandContent = applyTemplate(subcommandTemplate, names)

    // First add the import types
    const existing = readFileSync(cliPath, 'utf-8')
    if (dryRun) {
      console.log(`  Would append to: ${path.relative(ROOT, cliPath)}`)
      return
    }

    // Insert handler before the register function and subcommand at anchor
    let updated = existing

    // Add import types to the existing import block
    const importLine = `  ${names.SdkMethod}Input,\n  ${names.SdkMethod}Result,`
    const lastImportMatch = updated.match(/} from '@scenicai\/sdk'/)
    if (lastImportMatch) {
      updated = updated.replace(
        "} from '@scenicai/sdk'",
        `${importLine}\n} from '@scenicai/sdk'`,
      )
    }

    // Add handler function before the register function
    const registerMatch = updated.match(new RegExp(`export function register${names.Namespace}`))
    if (registerMatch && registerMatch.index != null) {
      updated = updated.slice(0, registerMatch.index) + handlerContent + '\n' + updated.slice(registerMatch.index)
    }

    // Add subcommand at anchor
    updated = updated.replace('  // ADD_ACTION: subcommand', subcommandContent)

    writeFileSync(cliPath, updated)
    console.log(`  Appended: ${path.relative(ROOT, cliPath)}`)
  } else {
    const template = readTemplate('cli-command.ts.tmpl')
    const content = applyTemplate(template, names)
    writeIfNew(cliPath, content, dryRun)
  }
}

function scaffoldUnity(names: CommandNames, dryRun: boolean): void {
  const unityDir = path.join(ROOT, 'unity', 'Editor', 'Commands', names.Namespace)
  const testDir = path.join(ROOT, 'unity', 'Tests', 'Editor', 'Commands', names.Namespace)

  const handlerPath = path.join(unityDir, `${names.Namespace}${names.Action}CommandHandler.cs`)
  const modelsPath = path.join(unityDir, `${names.Namespace}${names.Action}CommandModels.cs`)
  const testPath = path.join(testDir, `${names.Namespace}${names.Action}CommandHandlerTests.cs`)

  const handlerTemplate = readTemplate('unity-handler.cs.tmpl')
  const modelsTemplate = readTemplate('unity-models.cs.tmpl')
  const testTemplate = readTemplate('unity-test.cs.tmpl')

  writeIfNew(handlerPath, applyTemplate(handlerTemplate, names), dryRun)
  writeIfNew(modelsPath, applyTemplate(modelsTemplate, names), dryRun)
  writeIfNew(testPath, applyTemplate(testTemplate, names), dryRun)
}

function scaffoldIntegrationTests(names: CommandNames, dryRun: boolean): void {
  const sdkTestPath = path.join(ROOT, 'integration', 'sdk', 'safe', `${names.namespace}.test.ts`)
  const cliTestPath = path.join(ROOT, 'integration', 'cli', 'safe', `${names.namespace}.test.ts`)

  if (existsSync(sdkTestPath)) {
    const appendTemplate = readTemplate('integration-sdk-append.test.ts.tmpl')
    const content = applyTemplate(appendTemplate, names)
    appendAtAnchor(sdkTestPath, '  // ADD_ACTION: test', content, dryRun)
  } else {
    const template = readTemplate('integration-sdk.test.ts.tmpl')
    writeIfNew(sdkTestPath, applyTemplate(template, names), dryRun)
  }

  if (existsSync(cliTestPath)) {
    const appendTemplate = readTemplate('integration-cli-append.test.ts.tmpl')
    const content = applyTemplate(appendTemplate, names)
    appendAtAnchor(cliTestPath, '  // ADD_ACTION: test', content, dryRun)
  } else {
    const template = readTemplate('integration-cli.test.ts.tmpl')
    writeIfNew(cliTestPath, applyTemplate(template, names), dryRun)
  }
}

function main(): void {
  const args = process.argv.slice(2)
  const positional: string[] = []
  let guardFlag = false
  let dryRun = false

  for (let i = 0; i < args.length; i++) {
    if (args[i] === '--guard' && args[i + 1] === 'execute') {
      guardFlag = true
      i++
    } else if (args[i] === '--dry-run') {
      dryRun = true
    } else if (!args[i].startsWith('--')) {
      positional.push(args[i])
    } else {
      console.error(`Unknown flag: ${args[i]}`)
      process.exit(1)
    }
  }

  if (positional.length !== 2) {
    console.error('Usage: scaffold-command <namespace> <action> [--guard execute] [--dry-run]')
    console.error('')
    console.error('Examples:')
    console.error('  npx tsx scripts/scaffold-command.ts material create --guard execute')
    console.error('  npx tsx scripts/scaffold-command.ts material get')
    console.error('  npx tsx scripts/scaffold-command.ts material get --dry-run')
    process.exit(1)
  }

  const [namespace, action] = positional
  if (!/^[a-z][a-z0-9]*$/.test(namespace) || !/^[a-z][a-z0-9]*$/.test(action)) {
    console.error('Error: namespace and action must be lowercase alphanumeric (e.g., "material", "create").')
    process.exit(1)
  }

  const names = deriveNames(namespace, action, guardFlag)

  console.log(`\nScaffolding: ${names.wireCommand}${dryRun ? ' (dry run)' : ''}\n`)

  scaffoldSdkContract(names, dryRun)
  scaffoldCliCommand(names, dryRun)
  scaffoldUnity(names, dryRun)
  scaffoldIntegrationTests(names, dryRun)

  console.log(`\nManual steps remaining:`)
  console.log(`  1. Add ${names.sdkMethod}Command to allCommands in packages/sdk/src/commands/registry.ts`)
  console.log(`  2. Export types from packages/sdk/src/commands/contracts.ts`)
  console.log(`  3. Add register${names.Namespace}(program) to packages/cli/src/index.ts`)
  console.log(`  4. Open Unity Editor to generate .meta files`)
  console.log(`  5. Fill in TODO markers in generated files`)
  console.log('')
}

main()
