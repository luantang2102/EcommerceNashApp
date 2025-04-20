import { SearchOff } from "@mui/icons-material";
import { Button, Paper, Typography } from "@mui/material";
import { Link } from "react-router-dom";
import { useAppDispatch } from "../store/store";
import { useEffect } from "react";
import { setSidebarOpen } from "../layout/uiSlice";

export default function NotFound() {

  const dispatch = useAppDispatch();
  useEffect(() => {
    dispatch(setSidebarOpen(false))
  }, [dispatch]);
    
  return (
    <Paper
        sx={{height: 400, display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', p: 6, marginTop: '2vh'}}
    >
        <SearchOff sx={{fontSize: 100}} color='primary' />
        <Typography gutterBottom variant='h3'>
            Oops - We could not find what you were looking for :(
        </Typography>
        <Button fullWidth component={Link} to="/">
            Go back to dashboard
        </Button>
    </Paper>
  )
}
