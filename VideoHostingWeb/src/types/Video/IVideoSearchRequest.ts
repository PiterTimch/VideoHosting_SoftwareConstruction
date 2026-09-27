import type { IBaseSearch } from "../Additional/IBaseSearch";

export interface IVideoSearchRequest extends IBaseSearch {
    q?: string;
    title?: string;
    createYearFrom?: string;
    createYearTo?: string;
    sortBy?: string;
}
