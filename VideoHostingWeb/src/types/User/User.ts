export interface User {
    id?: number;
    name: string;
    email: string;
    image?: string;
    token: string;
    roles: string[];
    channelId?: number;
}
