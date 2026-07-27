export interface HealthCheck {
  name: string
  status: string
  durationMilliseconds: number
}

export interface SystemHealth {
  status: string
  service: string
  durationMilliseconds: number
  checks: HealthCheck[]
  traceId: string
}

export async function getSystemHealth(): Promise<SystemHealth> {
  const response = await fetch('/health/ready', {
    headers: {
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw new Error(`Health check failed with HTTP ${response.status}.`)
  }

  return (await response.json()) as SystemHealth
}
