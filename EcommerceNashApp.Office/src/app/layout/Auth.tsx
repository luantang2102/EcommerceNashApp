import { Box, Container, createTheme, CssBaseline, ThemeProvider } from "@mui/material";
import { Outlet } from "react-router-dom";
import { useAppSelector } from "../store/store";

const AuthLayout = () => {
  const { darkMode } = useAppSelector((state) => state.ui);
  const paletteType = darkMode ? "dark" : "light";

  const theme = createTheme({
    palette: {
      mode: paletteType,
      background: {
        default: paletteType === "light" ? "#eaeaea" : "#121212",
      },
    },
  });

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box
        sx={{
          minHeight: "100vh",
          background: darkMode
            ? "radial-gradient(circle, rgb(62, 66, 86), #111B27)"
            : "radial-gradient(circle, rgb(179, 215, 225), #f0f9ff)",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        <Container maxWidth="sm">
          <Outlet />
        </Container>
      </Box>
    </ThemeProvider>
  );
};

export default AuthLayout;