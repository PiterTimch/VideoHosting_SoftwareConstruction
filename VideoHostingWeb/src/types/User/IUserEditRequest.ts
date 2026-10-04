export interface IUserEditRequest {
    id?: number;
    firstName: string;
    lastName: string;
    email: string;
    image?: File | null;
}
