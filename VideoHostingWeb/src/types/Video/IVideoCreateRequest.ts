export interface IVideoCreateRequest {
    title: string;
    slug: string;
    description: string;
    image?: File;
    video?: File;
    privacyId: number;
}
