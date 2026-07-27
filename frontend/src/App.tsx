import { Box, Container, Typography } from '@mui/material'
import { SystemStatusPage } from './features/system/pages/SystemStatusPage'

export default function App() {
  return (
    <Box component="main" sx={{ minHeight: '100vh', py: { xs: 4, md: 8 } }}>
      <Container maxWidth="md">
        <Typography
          component="h1"
          variant="h3"
          gutterBottom
          sx={{ fontWeight: 700 }}
        >
          TradeFlow
        </Typography>
        <Typography color="text.secondary" sx={{ mb: 4 }}>
          Enterprise purchasing, inventory and sales platform.
        </Typography>
        <SystemStatusPage />
      </Container>
    </Box>
  )
}
