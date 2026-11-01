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
    portfolio?: { id: number; ownerId?: number | null; name?: string; nickName?: string; description?: string; specialization?: string; contacts?: string; avatarImage?: string; bannerImage?: string; subscriberCount?: number; isSubscribed?: boolean };
    likesCount?: number;
    dislikesCount?: number;
    isLiked?: boolean | null;
}
