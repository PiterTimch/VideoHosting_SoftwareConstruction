export interface ICreateCommentRequest {
    videoId: number;
    content: string;
    parentId?: number | null;
}
