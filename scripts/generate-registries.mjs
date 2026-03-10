#!/usr/bin/env node
import { readFileSync, readdirSync, writeFileSync } from 'node:fs'
import path from 'node:path'
import ts from 'typescript'

const ROOT = path.resolve(import.meta.dirname, '..')
const SDK_COMMANDS_DIR = path.join(ROOT, 'packages', 'sdk', 'src', 'commands')
const CLI_COMMANDS_DIR = path.join(ROOT, 'packages', 'cli', 'src', 'commands')
const REGISTRY_OUT = path.join(SDK_COMMANDS_DIR, 'registry.ts')
const CONTRACTS_OUT = path.join(SDK_COMMANDS_DIR, 'contracts.ts')
const CLI_INDEX_OUT = path.join(ROOT, 'packages', 'cli', 'src', 'index.ts')
const ALIASES_PATH = path.join(SDK_COMMANDS_DIR, 'contracts.aliases.json')

function readText(filePath) {
  return readFileSync(filePath, 'utf-8')
}

function parseSource(filePath) {
  return ts.createSourceFile(filePath, readText(filePath), ts.ScriptTarget.Latest, true, ts.ScriptKind.TS)
}

function hasExportModifier(node) {
  return (ts.getCombinedModifierFlags(node) & ts.ModifierFlags.Export) !== 0
}

function isDefineCommandCall(node) {
  return ts.isCallExpression(node)
    && ts.isIdentifier(node.expression)
    && node.expression.text === 'defineCommand'
}

function collectSdkDomains() {
  const entries = readdirSync(SDK_COMMANDS_DIR, { withFileTypes: true })
  const domains = []
  for (const entry of entries) {
    if (!entry.isDirectory()) {
      continue
    }
    const contractPath = path.join(SDK_COMMANDS_DIR, entry.name, 'contract.ts')
    try {
      readFileSync(contractPath)
    } catch {
      continue
    }

    const source = parseSource(contractPath)
    const commandNames = []
    const typeNames = []

    for (const statement of source.statements) {
      if (ts.isVariableStatement(statement) && hasExportModifier(statement)) {
        for (const decl of statement.declarationList.declarations) {
          if (!ts.isIdentifier(decl.name)) {
            continue
          }
          const exportedName = decl.name.text
          if (decl.initializer && isDefineCommandCall(decl.initializer)) {
            if (!exportedName.endsWith('Command')) {
              throw new Error(`Command export '${exportedName}' in ${contractPath} must end with 'Command'.`)
            }
            commandNames.push(exportedName)
          }
        }
      }

      if (ts.isTypeAliasDeclaration(statement) && hasExportModifier(statement)) {
        typeNames.push(statement.name.text)
      }

      if (ts.isInterfaceDeclaration(statement) && hasExportModifier(statement)) {
        typeNames.push(statement.name.text)
      }
    }

    if (commandNames.length === 0) {
      throw new Error(`No defineCommand exports found in ${contractPath}.`)
    }

    commandNames.sort((a, b) => a.localeCompare(b))
    typeNames.sort((a, b) => a.localeCompare(b))

    domains.push({
      domain: entry.name,
      importPath: `./${entry.name}/contract.ts`,
      commandNames,
      typeNames,
    })
  }

  domains.sort((a, b) => a.domain.localeCompare(b.domain))
  return domains
}

function collectCliRegisterModules() {
  const entries = readdirSync(CLI_COMMANDS_DIR, { withFileTypes: true })
  const modules = []

  for (const entry of entries) {
    if (!entry.isFile() || !entry.name.endsWith('.ts') || entry.name.endsWith('.test.ts')) {
      continue
    }

    const filePath = path.join(CLI_COMMANDS_DIR, entry.name)
    const source = parseSource(filePath)
    const registerNames = []

    for (const statement of source.statements) {
      if (ts.isFunctionDeclaration(statement) && hasExportModifier(statement) && statement.name) {
        const fnName = statement.name.text
        if (fnName.startsWith('register')) {
          registerNames.push(fnName)
        }
      }

      if (ts.isVariableStatement(statement) && hasExportModifier(statement)) {
        for (const decl of statement.declarationList.declarations) {
          if (!ts.isIdentifier(decl.name)) {
            continue
          }
          const varName = decl.name.text
          if (varName.startsWith('register')) {
            registerNames.push(varName)
          }
        }
      }
    }

    if (registerNames.length === 0) {
      continue
    }

    if (registerNames.length > 1) {
      throw new Error(`Expected one register* export in ${filePath}, found ${registerNames.length}.`)
    }

    modules.push({
      fileBase: entry.name.slice(0, -3),
      registerName: registerNames[0],
    })
  }

  modules.sort((a, b) => a.fileBase.localeCompare(b.fileBase))
  return modules
}

function readAliasConfig() {
  const raw = readText(ALIASES_PATH)
  const parsed = JSON.parse(raw)
  if (parsed == null || typeof parsed !== 'object' || Array.isArray(parsed)) {
    throw new Error(`Alias config at ${ALIASES_PATH} must be an object.`)
  }
  return parsed
}

function getAlias(aliasConfig, domain, typeName) {
  const domainMap = aliasConfig[domain]
  if (domainMap == null) {
    return undefined
  }
  if (typeof domainMap !== 'object' || Array.isArray(domainMap)) {
    throw new Error(`Alias config for domain '${domain}' must be an object.`)
  }
  const alias = domainMap[typeName]
  if (alias == null) {
    return undefined
  }
  if (typeof alias !== 'string' || alias.trim() === '') {
    throw new Error(`Alias for ${domain}.${typeName} must be a non-empty string.`)
  }
  return alias
}

function generateRegistry(domains) {
  const lines = []
  lines.push('// @generated by scripts/generate-registries.mjs. Do not edit manually.')
  lines.push('')

  for (const domain of domains) {
    lines.push(`import { ${domain.commandNames.join(', ')} } from '${domain.importPath}'`)
  }

  lines.push('')
  lines.push('export const allCommands = [')
  for (const domain of domains) {
    for (const commandName of domain.commandNames) {
      lines.push(`  ${commandName},`)
    }
  }
  lines.push('] as const')
  lines.push('')

  return lines.join('\n')
}

function generateContracts(domains, aliasConfig) {
  const lines = []
  lines.push('// @generated by scripts/generate-registries.mjs. Do not edit manually.')
  lines.push('')

  const exportedNames = new Map()

  for (const domain of domains) {
    if (domain.typeNames.length === 0) {
      continue
    }

    const rendered = []
    for (const typeName of domain.typeNames) {
      const alias = getAlias(aliasConfig, domain.domain, typeName)
      const exportName = alias ?? typeName

      if (exportedNames.has(exportName)) {
        const prior = exportedNames.get(exportName)
        throw new Error(`Type export collision for '${exportName}' between ${prior} and ${domain.domain}.${typeName}. Add an alias in ${path.relative(ROOT, ALIASES_PATH)}.`)
      }
      exportedNames.set(exportName, `${domain.domain}.${typeName}`)

      rendered.push(alias ? `${typeName} as ${alias}` : typeName)
    }

    lines.push('export type {')
    for (const name of rendered) {
      lines.push(`  ${name},`)
    }
    lines.push(`} from '${domain.importPath}'`)
  }

  lines.push('')
  return lines.join('\n')
}

function generateCliIndex(cliModules) {
  const lines = []
  lines.push('#!/usr/bin/env node')
  lines.push('// @generated by scripts/generate-registries.mjs. Do not edit manually.')
  lines.push("import { readFileSync } from 'node:fs'")
  lines.push("import path from 'node:path'")
  lines.push("import { program } from 'commander'")

  for (const mod of cliModules) {
    lines.push(`import { ${mod.registerName} } from './commands/${mod.fileBase}.ts'`)
  }

  lines.push('')
  lines.push('const { version } = JSON.parse(')
  lines.push("  readFileSync(path.join(import.meta.dirname, '..', 'package.json'), 'utf-8'),")
  lines.push(') as { version: string }')
  lines.push('')
  lines.push('program')
  lines.push("  .name('scenic')")
  lines.push("  .description('Bridge between Unity and your code')")
  lines.push('  .version(version)')
  lines.push("  .option('-p, --project <path>', 'Path to Unity project')")
  lines.push("  .option('--json', 'Output result as JSON')")
  lines.push("  .option('--no-execute', 'Disable execute tool for this invocation')")
  lines.push('')

  for (const mod of cliModules) {
    lines.push(`${mod.registerName}(program)`)
  }

  lines.push('')
  lines.push('program.parse()')
  lines.push('')

  return lines.join('\n')
}

function writeIfChanged(filePath, content) {
  const prior = readText(filePath)
  if (prior === content) {
    return false
  }
  writeFileSync(filePath, content)
  return true
}

function main() {
  const domains = collectSdkDomains()
  const cliModules = collectCliRegisterModules()
  const aliasConfig = readAliasConfig()

  const registryContent = generateRegistry(domains)
  const contractsContent = generateContracts(domains, aliasConfig)
  const cliIndexContent = generateCliIndex(cliModules)

  const changed = []
  if (writeIfChanged(REGISTRY_OUT, registryContent)) {
    changed.push(path.relative(ROOT, REGISTRY_OUT))
  }
  if (writeIfChanged(CONTRACTS_OUT, contractsContent)) {
    changed.push(path.relative(ROOT, CONTRACTS_OUT))
  }
  if (writeIfChanged(CLI_INDEX_OUT, cliIndexContent)) {
    changed.push(path.relative(ROOT, CLI_INDEX_OUT))
  }

  if (changed.length === 0) {
    console.log('No generated file changes.')
    return
  }

  console.log('Updated generated files:')
  for (const file of changed) {
    console.log(`- ${file}`)
  }
}

main()
