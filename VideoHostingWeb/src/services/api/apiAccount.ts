import { createApi } from "@reduxjs/toolkit/query/react";
import { createBaseQuery } from "../../utils/createBaseQuery";
import { serialize } from "object-to-formdata";
import { loginSuccess } from "../../store/slices/authSlice";
import type { Dispatch } from "@reduxjs/toolkit";
import type { IUserEditRequest } from "../../types/User/IUserEditRequest";
import type { IForgotPasswordRequest } from "../../types/Account/IForgotPasswordRequest";
import type { IValidateTokenRequest } from "../../types/Account/IValidateTokenRequest";
import type { IResetPasswordRequest } from "../../types/Account/IResetPasswordRequest";
import type { IChangePasswordRequest } from "../../types/Account/IChangePasswordRequest";
import type { IValidateResetToken } from "../../types/Account/IValidateResetToken";
import type { IUserHasPasswordResponse } from "../../types/User/IUserHasPasswordResponse";
import type { ILogin } from "../../types/Account/ILogin";
import type { IRegister } from "../../types/Account/IRegister";

const handleAuthSuccess = async (
    queryFulfilled: Promise<{ data: { token: string } }>,
    dispatch: Dispatch
) => {
    try {
        const { data } = await queryFulfilled;
        if (data?.token) {
            dispatch(loginSuccess(data.token));
        }
    } catch (error) {
        console.error('Auth error:', error);
    }
};

export const apiAccount = createApi({
    reducerPath: 'api/account',
    baseQuery: createBaseQuery('Account'),
    tagTypes: ['Account', 'AccountPassword'],
    endpoints: (builder) => ({
        login: builder.mutation<{ token: string }, ILogin>({
            query: (credentials) => ({
                url: 'login',
                method: 'POST',
                body: credentials
            }),
            onQueryStarted: async (_arg, { dispatch, queryFulfilled }) =>
                handleAuthSuccess(queryFulfilled, dispatch)
        }),
        loginByGoogle: builder.mutation<{ token: string }, string>({
            query: (token) => ({
                url: 'GoogleLogin',
                method: 'POST',
                body: { token }
            }),
            onQueryStarted: async (_arg, { dispatch, queryFulfilled }) =>
                handleAuthSuccess(queryFulfilled, dispatch)
        }),
        forgotPassword: builder.mutation<void, IForgotPasswordRequest>({
            query: (data) => ({
                url: 'ForgotPassword',
                method: 'POST',
                body: data
            })
        }),
        validateResetToken: builder.query<IValidateResetToken, IValidateTokenRequest>({
            query: (params) => ({
                url: 'validate-reset-token',
                params
            }),
            providesTags: ['Account']
        }),
        resetPassword: builder.mutation<void, IResetPasswordRequest>({
            query: (data) => ({
                url: 'ResetPassword',
                method: 'POST',
                body: data
            })
        }),
        changePassword: builder.mutation<void, IChangePasswordRequest>({
            query: (data) => ({
                url: 'ChangePassword',
                method: 'POST',
                body: data
            }),
            invalidatesTags: ['AccountPassword']
        }),
        register: builder.mutation<{ token: string }, IRegister>({
            query: (credentials) => {
                const formData = serialize(credentials);

                return {
                    url: 'register',
                    method: 'POST',
                    body: formData
                };
            },
            onQueryStarted: async (_arg, { dispatch, queryFulfilled }) =>
                handleAuthSuccess(queryFulfilled, dispatch)
        }),
        deleteAccount: builder.mutation<void, void>({
            query: () => {
                return {
                    url: 'delete',
                    method: 'DELETE',
                };
            },
            invalidatesTags: ['AccountPassword']
        }),
        editAccount: builder.mutation<{ token: string }, IUserEditRequest>({
            query: (credentials) => {
                const formData = serialize(credentials);

                return {
                    url: 'EditAccount',
                    method: 'PUT',
                    body: formData,
                };
            },
            invalidatesTags: ['AccountPassword']
        }),
        hasPassword: builder.query<IUserHasPasswordResponse, void>({
            query: () => {
                return {
                    url: 'has-password',
                    method: 'GET'
                };
            },
            providesTags: ['AccountPassword']
        }),
        refreshToken: builder.mutation<{ token: string }, void>({
            query: () => ({
                url: 'RefreshToken',
                method: 'POST'
            }),
            onQueryStarted: async (_arg, { dispatch, queryFulfilled }) =>
                handleAuthSuccess(queryFulfilled, dispatch)
        }),
        logout: builder.mutation<void, void>({
            query: () => ({
                url: 'logout',
                method: 'POST'
            })
        })
    })
});

export const {
    useLoginMutation,
    useForgotPasswordMutation,
    useResetPasswordMutation,
    useValidateResetTokenQuery,
    useRegisterMutation,
    useChangePasswordMutation,
    useLoginByGoogleMutation,
    useDeleteAccountMutation,
    useEditAccountMutation,
    useHasPasswordQuery,
    useRefreshTokenMutation,
    useLogoutMutation,
} = apiAccount;
