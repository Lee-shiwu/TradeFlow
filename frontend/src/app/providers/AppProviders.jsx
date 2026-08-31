/** @typedef {import('react').PropsWithChildren} PropsWithChildren */

import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter } from "react-router-dom";
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 30_000,
    },
  },
});
const theme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#075985",
    },
    background: {
      default: "#f4f7f9",
    },
  },
  shape: {
    borderRadius: 10,
  },
});
/**
 * @param {PropsWithChildren} props
 */
export function AppProviders({ children }) {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <BrowserRouter>{children}</BrowserRouter>
      </ThemeProvider>
    </QueryClientProvider>
  );
}
