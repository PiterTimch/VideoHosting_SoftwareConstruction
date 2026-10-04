import { configureStore } from '@reduxjs/toolkit';
import { apiVideos } from "../services/api/apiVideos";
import { apiAccount } from "../services/api/apiAccount";
import { apiComments } from "../services/api/apiComments";
import { apiUsers } from "../services/api/apiUsers";
import { type TypedUseSelectorHook, useDispatch, useSelector } from 'react-redux';
import { setupListeners } from "@reduxjs/toolkit/query/react";
import authReducer from "./slices/authSlice";
import playerReducer from "./slices/playerSlice";

export const store = configureStore({
    reducer: {
        auth: authReducer,
        player: playerReducer,
        [apiVideos.reducerPath]: apiVideos.reducer,
        [apiAccount.reducerPath]: apiAccount.reducer,
        [apiComments.reducerPath]: apiComments.reducer,
        [apiUsers.reducerPath]: apiUsers.reducer,
    },

    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware().concat(
            apiVideos.middleware,
            apiAccount.middleware,
            apiComments.middleware,
            apiUsers.middleware
        )
});

setupListeners(store.dispatch);
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export const useAppDispatch: () => AppDispatch = useDispatch;
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
