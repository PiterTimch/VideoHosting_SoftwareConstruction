export interface IVideoEditRequest {
    id: number;
    title: string;
    slug: string;
    description?: string;
    image?: File;
    video?: File;
    privacyId: number;
}
