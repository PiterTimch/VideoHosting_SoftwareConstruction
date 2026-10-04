import type { IBaseSearch } from "../Additional/IBaseSearch";

export interface IUserSearchRequest extends IBaseSearch {
    name?: string;
    email?: string;
}
