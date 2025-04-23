import { useState } from "react";
import { IconButton, Tooltip, Typography } from "@mui/material";
import EmojiEmotionsIcon from "@mui/icons-material/EmojiEmotions";
import MoodBadIcon from "@mui/icons-material/MoodBad";
import SentimentSatisfiedAltIcon from "@mui/icons-material/SentimentSatisfiedAlt";
import SentimentVeryDissatisfiedIcon from "@mui/icons-material/SentimentVeryDissatisfied";
import SentimentVerySatisfiedIcon from "@mui/icons-material/SentimentVerySatisfied";

const moods = [
  { label: "Happy", icon: <EmojiEmotionsIcon color="warning" /> },
  { label: "Sad", icon: <MoodBadIcon color="primary" /> },
  { label: "Okay", icon: <SentimentSatisfiedAltIcon color="success" /> },
  { label: "Angry", icon: <SentimentVeryDissatisfiedIcon color="error" /> },
  { label: "Awesome!", icon: <SentimentVerySatisfiedIcon color="secondary" /> },
];

const MoodButton = () => {
  const [mood, setMood] = useState(moods[0]);

  const handleClick = () => {
    const randomMood = moods[Math.floor(Math.random() * moods.length)];
    setMood(randomMood);
  };

  return (
    <Tooltip title={`Current mood: ${mood.label}`}>
      <IconButton onClick={handleClick} sx={{ display: "flex", flexDirection: "column" }}>
        {mood.icon}
        <Typography variant="caption" sx={{ fontSize: "0.6rem" }}>
          {mood.label}
        </Typography>
      </IconButton>
    </Tooltip>
  );
};

export default MoodButton;
