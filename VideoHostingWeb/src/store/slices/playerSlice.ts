import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

interface PlayerState {
    volume: number;
    isMuted: boolean;
}

const getStoredVolume = (): number => {
    const stored = localStorage.getItem('player_volume');
    if (stored !== null) {
        const parsed = parseFloat(stored);
        if (!isNaN(parsed) && parsed >= 0 && parsed <= 1) {
            return parsed;
        }
    }
    return 1;
};

const getStoredMuted = (): boolean => {
    const stored = localStorage.getItem('player_muted');
    return stored === 'true';
};

const initialState: PlayerState = {
    volume: getStoredVolume(),
    isMuted: getStoredMuted(),
};

const playerSlice = createSlice({
    name: 'player',
    initialState,
    reducers: {
        setVolume: (state, action: PayloadAction<number>) => {
            const vol = Math.max(0, Math.min(1, action.payload));
            state.volume = vol;
            localStorage.setItem('player_volume', vol.toString());
        },
        setIsMuted: (state, action: PayloadAction<boolean>) => {
            state.isMuted = action.payload;
            localStorage.setItem('player_muted', action.payload.toString());
        },
    },
});

export const { setVolume, setIsMuted } = playerSlice.actions;
export default playerSlice.reducer;
