import { Box, Drawer, List, Divider, IconButton, ListItem, ListItemButton, ListItemIcon, ListItemText } from "@mui/material";
import { Category, Group, Inventory, ChevronLeft, ChevronRight, Logout } from "@mui/icons-material";
import { NavLink, useNavigate } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../store/store";
import { setSidebarOpen } from "../uiSlice";
import { useTheme } from "@mui/material/styles";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useLogoutMutation } from "../../api/authApi";
import { clearAuth } from "../../../features/Auth/authSlice";

const drawerWidth = 240;

export default function SideBar() {
  const { openSideBar } = useAppSelector((state) => state.ui);
  const dispatch = useAppDispatch();
  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("md"));
  const navigate = useNavigate();
  const [logout] = useLogoutMutation();

  const handleToggleSidebar = () => {
    dispatch(setSidebarOpen(!openSideBar));
  };

  const handleLogout = async () => {
    try {
      await logout().unwrap();
      dispatch(clearAuth());
      navigate("/signin");
      if (isSmallScreen) {
        dispatch(setSidebarOpen(false));
      }
    } catch (error) {
      console.error("Logout failed:", error);
    }
  };

  return (
    <Drawer
      variant={isSmallScreen ? "temporary" : "permanent"}
      open={openSideBar}
      onClose={handleToggleSidebar}
      sx={{
        width: openSideBar ? drawerWidth : 56,
        "& .MuiDrawer-paper": {
          width: openSideBar ? drawerWidth : isSmallScreen ? 0 : 56,
          backgroundColor: "background.paper",
          overflow: "hidden",
          zIndex: (theme) => theme.zIndex.appBar - 1,
          transition: theme.transitions.create("width", {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.enteringScreen,
          }),
        },
      }}
      ModalProps={{
        keepMounted: true, // Better performance on mobile
      }}
    >
      <Box sx={{ 
        display: "flex", 
        alignItems: "center", 
        justifyContent: "flex-end", 
        p: 1
      }}>
        <IconButton onClick={handleToggleSidebar}>
          {openSideBar ? <ChevronLeft /> : <ChevronRight />}
        </IconButton>
      </Box>
      <Divider />
      <List>
        {["Customers", "Products", "Categories"].map((text) => (
          <ListItem key={text} disablePadding>
            <ListItemButton
              component={NavLink}
              to={`/${text.toLowerCase()}`}
              sx={{ justifyContent: openSideBar ? "initial" : "center" }}
              onClick={isSmallScreen ? handleToggleSidebar : undefined}
            >
              <ListItemIcon sx={{ minWidth: 0, mr: openSideBar ? 3 : "auto" }}>
                {text === "Customers" ? <Group /> : text === "Products" ? <Inventory /> : <Category />}
              </ListItemIcon>
              <ListItemText primary={text} sx={{ opacity: openSideBar ? 1 : 0 }} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>
      <Divider />
      <List>
        <ListItem disablePadding>
          <ListItemButton 
            sx={{ justifyContent: openSideBar ? "initial" : "center" }}
            onClick={handleLogout}
          >
            <ListItemIcon sx={{ minWidth: 0, mr: openSideBar ? 3 : "auto" }}>
              <Logout />
            </ListItemIcon>
            <ListItemText primary="Logout" sx={{ opacity: openSideBar ? 1 : 0 }} />
          </ListItemButton>
        </ListItem>
      </List>
    </Drawer>
  );
}