import { createApi } from "@reduxjs/toolkit/query/react";
import { createBaseQuery } from "../../utils/createBaseQuery";
import { serialize } from "object-to-formdata";
import type { IVideoItemResponse } from "../../types/Video/IVideoItemResponse";
import type { IVideoPrivacyItemResponse } from "../../types/Video/IVideoPrivacyItemResponse";
import type { IVideoSearchRequest } from "../../types/Video/IVideoSearchRequest";
import type { IVideoCreateRequest } from "../../types/Video/IVideoCreateRequest";
import type { IVideoEditRequest } from "../../types/Video/IVideoEditRequest";
import type { IVideoDeleteRequest } from "../../types/Video/IVideoDeleteRequest";
import type { IVideoProcessingResult } from "../../types/Video/IVideoProcessingResult";
import type { IGetByRequest } from "../../types/Additional/IGetByRequest";
import type { IPagedResult } from "../../types/Additional/IPagedResult";
import type { IVideoRecommendationRequest } from "../../types/Video/IVideoRecommendationRequest";
import type { IVideoAutocompleteResponse } from "../../types/Video/IVideoAutocompleteResponse";

export const apiVideos = createApi({
    reducerPath: "api/videos",
    baseQuery: createBaseQuery("Videos"),
    tagTypes: ["Videos", "Video", "Recommendations"],
    endpoints: (builder) => ({
        searchVideos: builder.query<IPagedResult<IVideoItemResponse>, IVideoSearchRequest>({
            query: (params) => ({
                url: "search",
                method: "GET",
                params,
            }),
            providesTags: ["Videos"],
        }),

        getRecommendations: builder.query<IVideoItemResponse[], IVideoRecommendationRequest>({
            query: (params) => ({
                url: "recommendations",
                method: "GET",
                params,
            }),
            providesTags: ["Recommendations"],
        }),

        getBy: builder.query<IVideoItemResponse, IGetByRequest>({
            query: (par) => ({
                url: "get-by",
                method: "GET",
                params: par,
            }),
            providesTags: (result) =>
                result ? [{ type: "Video", id: result.id }] : ["Video"],
        }),

        createVideo: builder.mutation<IVideoProcessingResult, IVideoCreateRequest>({
            query: (body) => ({
                url: "",
                method: "POST",
                body: serialize(body),
            }),
            invalidatesTags: ["Videos", "Recommendations"],
        }),

        editVideo: builder.mutation<IVideoProcessingResult, IVideoEditRequest>({
            query: (body) => ({
                url: "",
                method: "PUT",
                body: serialize(body),
            }),
            invalidatesTags: (_result, _error, { id }) => [{ type: "Video", id }, "Videos", "Recommendations"],
        }),

        deleteVideo: builder.mutation<IVideoItemResponse[], IVideoDeleteRequest>({
            query: (body) => ({
                url: "",
                method: "DELETE",
                body,
            }),
            invalidatesTags: (_result, _error, { id }) => [{ type: "Video", id }, "Videos"],
        }),

        getPrivacies: builder.query<IVideoPrivacyItemResponse[], void>({
            query: () => ({
                url: "privacies",
                method: "GET",
            }),
        }),

        incrementView: builder.mutation<void, number>({
            query: (id) => ({
                url: `${id}/view`,
                method: "POST",
            }),
        }),

        autocompleteVideos: builder.query<IVideoAutocompleteResponse[], string>({
            query: (q) => ({
                url: "autocomplete",
                method: "GET",
                params: { q },
            }),
        }),
    }),
});

export const {
    useSearchVideosQuery,
    useGetByQuery,
    useCreateVideoMutation,
    useEditVideoMutation,
    useDeleteVideoMutation,
    useGetPrivaciesQuery,
    useIncrementViewMutation,
    useGetRecommendationsQuery,
    useAutocompleteVideosQuery,
} = apiVideos;
