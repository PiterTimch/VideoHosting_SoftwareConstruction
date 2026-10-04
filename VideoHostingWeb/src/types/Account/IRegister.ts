export interface IRegister {
    firstName: string;
    lastName: string;
    email: string;
    password?: string;
    imageFile?: File | null;
}
