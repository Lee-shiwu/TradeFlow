import {
  Alert,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Stack,
  Typography,
} from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { getSystemHealth } from '../api/getSystemHealth'

export function SystemStatusPage() {
  const health = useQuery({
    queryKey: ['system', 'health'],
    queryFn: getSystemHealth,
    refetchInterval: 30_000,
  })

  if (health.isPending) {
    return (
      <Card>
        <CardContent>
          <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
            <CircularProgress size={24} />
            <Typography>Checking API and database connectivity…</Typography>
          </Stack>
        </CardContent>
      </Card>
    )
  }

  if (health.isError) {
    return (
      <Alert
        severity="warning"
        action={
          <Button color="inherit" onClick={() => health.refetch()}>
            Retry
          </Button>
        }
      >
        TradeFlow is not ready. Start the API and SQL Server, then retry.
      </Alert>
    )
  }

  return (
    <Alert severity="success">
      <Typography sx={{ fontWeight: 700 }}>
        Platform foundation is ready
      </Typography>
      <Typography variant="body2">
        {health.data.service} is connected. Database status:{' '}
        {health.data.checks[0]?.status ?? 'Healthy'}.
      </Typography>
    </Alert>
  )
}
