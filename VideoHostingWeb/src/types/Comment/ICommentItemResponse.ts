export interface ICommentItemResponse {
    id: number;
    videoId: number;
    userId: number;
    userName: string;
    userImage?: string;
    content: string;
    dateCreated: string;
    isEdited: boolean;
    parentId?: number | null;
    repliesCount: number;
}
