export interface RuntimeConfig {
  temporaryOrganisationId: string
  temporaryUserId: string
}

const guidPattern =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

const emptyGuid = '00000000-0000-0000-0000-000000000000'

function readRequiredGuid(name: string, value: unknown): string {
  if (typeof value !== 'string' || value.trim().length === 0) {
    throw new Error(`Environment variable '${name}' is required.`)
  }

  const normalizedValue = value.trim().toLowerCase()

  if (!guidPattern.test(normalizedValue) || normalizedValue === emptyGuid) {
    throw new Error(
      `Environment variable '${name}' must contain a non-empty GUID.`,
    )
  }

  return normalizedValue
}

export function getRuntimeConfig(): RuntimeConfig {
  return {
    temporaryOrganisationId: readRequiredGuid(
      'VITE_TEMP_ORGANISATION_ID',
      import.meta.env.VITE_TEMP_ORGANISATION_ID,
    ),
    temporaryUserId: readRequiredGuid(
      'VITE_TEMP_USER_ID',
      import.meta.env.VITE_TEMP_USER_ID,
    ),
  }
}
