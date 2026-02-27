export function parseOptionalInt(value: string | undefined, label: string): number | undefined {
  if (value == null) {
    return undefined
  }

  const parsed = Number.parseInt(value, 10)
  if (!Number.isInteger(parsed)) {
    throw new Error(`${label} must be an integer.`)
  }

  return parsed
}
