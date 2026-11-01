import { createApi } from '@reduxjs/toolkit/query/react';
import { createBaseQuery } from '../../utils/createBaseQuery';
import { serialize } from 'object-to-formdata';

export interface Portfolio {
    id: number;
    ownerId?: number;
    name: string;
    nickName: string;
    subscriberCount: number;
    description?: string;
    specialization?: string;
    contacts?: string;
    avatarImage?: string;
    bannerImage?: string;
    isSubscribed: boolean;
}

export const apiPortfolios = createApi({
    reducerPath: 'api/portfolios',
    baseQuery: createBaseQuery('Portfolios'),
    tagTypes: ['Portfolio'],
    endpoints: builder => ({
        getPortfolio: builder.query<Portfolio, number>({
            query: id => ({ url: 'get-by', method: 'GET', params: { id } }),
            providesTags: (_result, _error, id) => [{ type: 'Portfolio', id }],
        }),
        togglePortfolioSubscription: builder.mutation<void, number>({
            query: portfolioId => ({ url: 'subscribe', method: 'POST', body: { portfolioId } }),
            invalidatesTags: (_result, _error, id) => [{ type: 'Portfolio', id }],
        }),
        updatePortfolio: builder.mutation<Portfolio, { id: number; name: string; nickName: string; description: string; specialization?: string; contacts?: string; avatarImage?: File; bannerImage?: File }>({
            query: body => ({ url: '', method: 'PUT', body: serialize(body) }),
            invalidatesTags: (_result, _error, body) => [{ type: 'Portfolio', id: body.id }],
        }),
    }),
});

export const { useGetPortfolioQuery, useTogglePortfolioSubscriptionMutation, useUpdatePortfolioMutation } = apiPortfolios;
