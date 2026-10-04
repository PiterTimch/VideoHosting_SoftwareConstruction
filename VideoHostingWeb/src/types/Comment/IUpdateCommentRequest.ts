export interface IUpdateCommentRequest {
    id: number;
    videoId: number;
    content: string;
    parentId?: number | null;
}
