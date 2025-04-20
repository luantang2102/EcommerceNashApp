import { Box, Drawer, List, Divider, IconButton, ListItem, ListItemButton, ListItemIcon, ListItemText } from "@mui/material";
import { Category, Group, Inventory, Settings, ChevronLeft, ChevronRight } from "@mui/icons-material";
import { NavLink } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../store/store";
import { setSidebarOpen } from "../uiSlice";
import { useTheme } from "@mui/material/styles";
import useMediaQuery from "@mui/material/useMediaQuery";

const drawerWidth = 240;

export default function SideBar() {
  const { openSideBar } = useAppSelector((state) => state.ui);
  const dispatch = useAppDispatch();
  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("sm"));

  const handleToggleSidebar = () => {
    dispatch(setSidebarOpen(!openSideBar));
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
        p: 1,
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
          <ListItemButton sx={{ justifyContent: openSideBar ? "initial" : "center" }}>
            <ListItemIcon sx={{ minWidth: 0, mr: openSideBar ? 3 : "auto" }}>
              <Settings />
            </ListItemIcon>
            <ListItemText primary="Settings" sx={{ opacity: openSideBar ? 1 : 0 }} />
          </ListItemButton>
        </ListItem>
      </List>
    </Drawer>
  );
}