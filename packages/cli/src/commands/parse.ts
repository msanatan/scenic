export function parseName(value: string, label: string, maxLength?: number): string {
  const trimmed = value.trim()
  if (trimmed.length === 0) {
    throw new Error(`${label} is required.`)
  }
  if (trimmed.includes(',')) {
    throw new Error(`${label} cannot contain ','.`)
  }
  if (maxLength != null && trimmed.length > maxLength) {
    throw new Error(`${label} must be ${maxLength} characters or fewer.`)
  }
  return trimmed
}

export function parseIntWithMinimum(
  value: string | undefined,
  label: string,
  defaultValue: number,
  minimum: number,
): number {
  if (value == null) {
    return defaultValue
  }

  const parsed = Number.parseInt(value, 10)
  if (!Number.isFinite(parsed) || parsed < minimum) {
    if (minimum <= 0) {
      throw new Error(`${label} must be a non-negative integer.`)
    }
    throw new Error(`${label} must be an integer >= ${minimum}.`)
  }

  return parsed
}

export function normalizeLabels(labels: string[]): string[] {
  const seen = new Set<string>()
  const result: string[] = []
  for (const label of labels) {
    const trimmed = label.trim()
    if (trimmed.length > 0 && !seen.has(trimmed)) {
      seen.add(trimmed)
      result.push(trimmed)
    }
  }
  return result
}

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
