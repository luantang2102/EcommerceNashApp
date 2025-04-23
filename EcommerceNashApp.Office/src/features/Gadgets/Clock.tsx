import { Typography, Box } from "@mui/material";
import AccessTimeIcon from "@mui/icons-material/AccessTime";
import CalendarTodayIcon from "@mui/icons-material/CalendarToday";
import { useState, useEffect } from "react";

const Clock = () => {
  const [time, setTime] = useState(new Date());

  useEffect(() => {
    const timer = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, ml: 2 }}>
      <CalendarTodayIcon sx={{ fontSize: 18, color: 'text.secondary' }} />
      <Typography variant="body2" color="text.secondary" style={{ fontWeight: 800}}>
        {time.toLocaleDateString()}
      </Typography>

      <AccessTimeIcon sx={{ fontSize: 18, color: 'text.secondary', ml: 1 }} />
      <Typography variant="body2" color="text.secondary">
        {time.toLocaleTimeString()}
      </Typography>
    </Box>
  );
};

export default Clock;
