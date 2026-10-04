import { createApi } from "@reduxjs/toolkit/query/react";
import { createBaseQuery } from "../../utils/createBaseQuery";
import type { ICommentItemResponse } from "../../types/Comment/ICommentItemResponse";
import type { ICreateCommentRequest } from "../../types/Comment/ICreateCommentRequest";
import type { IUpdateCommentRequest } from "../../types/Comment/IUpdateCommentRequest";
import type { IBaseSearch } from "../../types/Additional/IBaseSearch";
import type { IPagedResult } from "../../types/Additional/IPagedResult";

export const apiComments = createApi({
    reducerPath: "api/comments",
    baseQuery: createBaseQuery("Comments"),
    tagTypes: ["Comments", "Comment"],
    endpoints: (builder) => ({
        getVideoComments: builder.query<IPagedResult<ICommentItemResponse>, { videoId: number; params?: IBaseSearch }>({
            query: ({ videoId, params }) => ({
                url: `video/${videoId}`,
                method: "GET",
                params,
            }),
            providesTags: (_result, _error, { videoId }) => [
                { type: "Comments" as const, id: `VIDEO_${videoId}` },
            ],
        }),

        getCommentReplies: builder.query<IPagedResult<ICommentItemResponse>, { parentId: number; params?: IBaseSearch }>({
            query: ({ parentId, params }) => ({
                url: `${parentId}/replies`,
                method: "GET",
                params,
            }),
            providesTags: (_result, _error, { parentId }) => [
                { type: "Comments" as const, id: `REPLIES_${parentId}` },
            ],
        }),

        createComment: builder.mutation<ICommentItemResponse, ICreateCommentRequest>({
            query: (body) => ({
                url: "",
                method: "POST",
                body,
            }),
            invalidatesTags: (_result, _error, { videoId, parentId }) => [
                { type: "Comments", id: `VIDEO_${videoId}` },
                ...(parentId != null ? [{ type: "Comments" as const, id: `REPLIES_${parentId}` }] : []),
            ],
        }),

        updateComment: builder.mutation<ICommentItemResponse, IUpdateCommentRequest>({
            query: ({ id, ...body }) => ({
                url: `${id}`,
                method: "PUT",
                body,
            }),
            invalidatesTags: (_result, _error, { videoId, parentId }) => [
                { type: "Comments", id: `VIDEO_${videoId}` },
                ...(parentId != null ? [{ type: "Comments" as const, id: `REPLIES_${parentId}` }] : []),
            ],
        }),

        deleteComment: builder.mutation<void, { id: number; videoId: number; parentId?: number | null }>({
            query: ({ id }) => ({
                url: `${id}`,
                method: "DELETE",
            }),
            invalidatesTags: (_result, _error, { videoId, parentId }) => [
                { type: "Comments", id: `VIDEO_${videoId}` },
                ...(parentId != null ? [{ type: "Comments" as const, id: `REPLIES_${parentId}` }] : []),
            ],
        }),
    }),
});

export const {
    useGetVideoCommentsQuery,
    useGetCommentRepliesQuery,
    useLazyGetCommentRepliesQuery,
    useCreateCommentMutation,
    useUpdateCommentMutation,
    useDeleteCommentMutation,
} = apiComments;
