import { DarkMode, LightMode, Menu } from "@mui/icons-material";
import { styled } from '@mui/material/styles';
import { Box, IconButton, LinearProgress, Toolbar } from "@mui/material";
import MuiAppBar, { AppBarProps as MuiAppBarProps } from '@mui/material/AppBar';
import { NavLink } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../../store/store";
import { setSidebarOpen, toggleDarkMode } from "../uiSlice";
import Logo from "../../../assets/logo.png";
import MoodButton from "../../../features/Gadgets/MoodButton";
import Clock from "../../../features/Gadgets/Clock";

interface AppBarProps extends MuiAppBarProps {
    open?: boolean;
}

const AppBar = styled(MuiAppBar, {
    shouldForwardProp: (prop) => prop !== 'open',
})<AppBarProps>(({ theme }) => ({
    zIndex: theme.zIndex.drawer + 1,
    transition: theme.transitions.create(['width', 'margin'], {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.leavingScreen,
    }),
    marginLeft: 0,
    width: '100%',
}));

export default function NavBar() {
    const { isLoading, darkMode, openSideBar } = useAppSelector(state => state.ui);
    const dispatch = useAppDispatch();

    const handleToggleSidebar = () => {
        dispatch(setSidebarOpen(!openSideBar));
    };

    return (
        <AppBar
            position='fixed'
            color='default'
            open={openSideBar}
            elevation={0}
            sx={{ borderBottom: 1, borderColor: 'divider', backgroundColor: darkMode ? "#121212" : "rgb(125, 166, 254)" }}
        >
            <Toolbar variant='regular' sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box display='flex' alignItems='center'>
                    <IconButton
                    color="inherit"
                    aria-label="open drawer"
                    onClick={handleToggleSidebar}
                    edge="start"
                    sx={{ marginRight: 2 }}
                    >
                    <Menu />
                    </IconButton>
                    <NavLink to="/">
                    <Box
                        sx={{
                        display: "flex",
                        justifyContent: "center",
                        alignItems: "center",
                        }}
                    >
                        <img
                        src={Logo}
                        alt="Logo"
                        style={{
                            maxWidth: "120px",
                            maxHeight: "40px",
                            objectFit: "contain",
                        }}
                        />
                    </Box>
                    </NavLink>
                    <IconButton onClick={() => dispatch(toggleDarkMode())}>
                    {darkMode ? <DarkMode /> : <LightMode />}
                    </IconButton>
                </Box>

                <Box display='flex' alignItems='center'>
                    <MoodButton />
                    <Clock />
                </Box>
                </Toolbar>
            {isLoading && (
                <Box sx={{ width: '100%' }}>
                    <LinearProgress color='info' />
                </Box>
            )}
        </AppBar>
    );
}