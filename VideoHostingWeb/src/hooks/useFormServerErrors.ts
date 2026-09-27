import { useState } from 'react';
import type { IServerValidationErrors } from "../types/Additional/IServerValidationErrors";

export const useFormServerErrors = () => {
    const [errors, setErrors] = useState<IServerValidationErrors>({});

    const setServerErrors = (serverErrors: IServerValidationErrors) => {
        const normalized: IServerValidationErrors = {};
        Object.entries(serverErrors).forEach(([key, value]) => {
            const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
            normalized[camelKey] = value;
        });
        setErrors(normalized);
    };

    const clearError = (field: string) => {
        setErrors(prev => {
            const copy = { ...prev };
            delete copy[field];
            return copy;
        });
    };

    const clearAllErrors = () => {
        setErrors({});
    };

    return {
        errors,
        setServerErrors,
        clearError,
        clearAllErrors,
    };
};
