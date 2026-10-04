import { fetchBaseQuery } from "@reduxjs/toolkit/query";
import { APP_ENV } from "../env";
import type { RootState } from "../store";

export const createBaseQuery = (endpoint: string) => {
    return fetchBaseQuery({
        baseUrl: `${APP_ENV.API_BASE_URL}/api/${endpoint}`,
        credentials: 'include',
        prepareHeaders: (headers, { getState }) => {
            const token = (getState() as RootState).auth?.user?.token;
            if (token) {
                headers.set('authorization', `Bearer ${token}`);
            }
            return headers;
        },
    });
}
