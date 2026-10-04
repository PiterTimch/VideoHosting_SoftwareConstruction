export interface ServerError {
    status?: number;
    data?: {
        message?: string;
        errors?: Record<string, string[]>;
    };
}
