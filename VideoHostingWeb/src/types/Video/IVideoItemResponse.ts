import type { IVideoPrivacyItemResponse } from "./IVideoPrivacyItemResponse";

export interface IVideoItemResponse {
    id: number;
    title: string;
    slug: string;
    description?: string;
    dateCreated?: string;
    viewCount: number;
    image?: string;
    video?: string;
    privacy?: IVideoPrivacyItemResponse;
    likesCount?: number;
    dislikesCount?: number;
    isLiked?: boolean | null;
}
