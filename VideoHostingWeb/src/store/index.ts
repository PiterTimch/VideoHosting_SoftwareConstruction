import { configureStore } from '@reduxjs/toolkit';
import { apiVideos } from "../services/api/apiVideos";
import { type TypedUseSelectorHook, useDispatch, useSelector } from 'react-redux';
import { setupListeners } from "@reduxjs/toolkit/query/react";
import playerReducer from "./slices/playerSlice";

export const store = configureStore({
    reducer: {
        player: playerReducer,
        [apiVideos.reducerPath]: apiVideos.reducer,
    },

    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware().concat(
            apiVideos.middleware
        )
});

setupListeners(store.dispatch);
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export const useAppDispatch: () => AppDispatch = useDispatch;
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
