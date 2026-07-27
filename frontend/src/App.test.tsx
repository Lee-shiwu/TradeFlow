import { render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import { AppProviders } from './app/providers/AppProviders'

describe('App', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            status: 'Healthy',
            service: 'TradeFlow.Api',
            durationMilliseconds: 1,
            checks: [
              {
                name: 'sqlserver',
                status: 'Healthy',
                durationMilliseconds: 1,
              },
            ],
            traceId: 'test-trace-id',
          }),
          {
            status: 200,
            headers: { 'Content-Type': 'application/json' },
          },
        ),
      ),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the connected platform status', async () => {
    render(
      <AppProviders>
        <App />
      </AppProviders>,
    )

    expect(
      await screen.findByText('Platform foundation is ready'),
    ).toBeInTheDocument()
    expect(screen.getByText(/Database status: Healthy/)).toBeInTheDocument()
  })
})
