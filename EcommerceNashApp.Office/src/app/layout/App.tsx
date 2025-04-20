import { Box, Container, createTheme, CssBaseline, Fab, ThemeProvider } from "@mui/material";
import { Outlet } from "react-router-dom";
import { KeyboardArrowUp } from "@mui/icons-material";
import { useAppSelector } from "../store/store";
import SideBar from "./Sidebar";
import NavBar from "./Navbar";

function App() {
  const { darkMode } = useAppSelector((state) => state.ui);
  const paletteType = darkMode ? "dark" : "light";

  const theme = createTheme({
    palette: {
      mode: paletteType,
      background: {
        default: paletteType === "light" ? "#eaeaea" : "#121212",
      },
    },
    components: {
      MuiTableHead: {
        defaultProps: {
          sx: {
            backgroundColor: paletteType === "light" ? "#f5f5f5" : "#424242",
          },
        }
      }
    }
  });

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <NavBar />
      <Box
        sx={{
          minHeight: "100vh",
          background: darkMode
            ? "radial-gradient(circle, rgb(62, 66, 86), #111B27)"
            : "radial-gradient(circle, rgb(179, 215, 225), #f0f9ff)",
          display: "flex",
        }}
      >
        <SideBar />
        <Container maxWidth="xl" sx={{ mt: 8, flexGrow: 1 }}>
          <Outlet />
        </Container>
        <Fab
          color="info"
          onClick={scrollToTop}
          sx={{
            position: "fixed",
            bottom: 16,
            right: 16,
            zIndex: 1000,
          }}
        >
          <KeyboardArrowUp />
        </Fab>
      </Box>
    </ThemeProvider>
  );
}

export default App;