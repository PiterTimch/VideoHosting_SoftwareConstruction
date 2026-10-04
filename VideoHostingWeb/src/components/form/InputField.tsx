import React, { useState, type InputHTMLAttributes } from "react";
import { Eye, EyeOff } from "lucide-react";
import { Button } from "./Button";

interface InputFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'error'> {
    label?: string;
    error?: string | string[];
    icon?: React.ReactNode;
    inputClassName?: string;
    wrapperClassName?: string;
    labelClassName?: string;
}

export const InputField = React.forwardRef<HTMLInputElement, InputFieldProps>(
    ({
        label,
        name,
        value,
        placeholder,
        type = 'text',
        required = false,
        error,
        onChange,
        icon,
        inputClassName = '',
        wrapperClassName = '',
        labelClassName = '',
        ...props
    }, ref) => {
        const [showPassword, setShowPassword] = useState(false);
        const isPassword = type === 'password';
        const currentType = isPassword ? (showPassword ? 'text' : 'password') : type;

        const errorMessage = Array.isArray(error) ? error[0] : error;

        return (
            <div className={`flex flex-col ${wrapperClassName}`}>
                {label && (
                    <label className={`text-zinc-400 mb-1 font-semibold ${labelClassName}`}>
                        {label}
                    </label>
                )}

                <div className="relative w-full">
                    {icon && (
                        <div className="absolute left-4 top-1/2 -translate-y-1/2 text-zinc-500 flex items-center justify-center pointer-events-none">
                            {icon}
                        </div>
                    )}

                    <input
                        ref={ref}
                        type={currentType}
                        name={name}
                        value={value}
                        placeholder={placeholder}
                        required={required}
                        onChange={onChange}
                        className={`w-full bg-zinc-800 text-zinc-100 rounded-xl py-3 px-4 border transition outline-none placeholder:text-zinc-500
                            ${icon ? 'pl-12' : ''}
                            ${isPassword ? 'pr-12' : ''}
                            ${errorMessage ? 'border-red-500' : 'border-zinc-700 focus:border-red-600'}
                            [&::-webkit-calendar-picker-indicator]:cursor-pointer
                            [&::-webkit-calendar-picker-indicator]:opacity-80
                            hover:[&::-webkit-calendar-picker-indicator]:opacity-100
                            ${inputClassName}
                        `}
                        {...props}
                    />

                    {isPassword && (
                        <Button
                            type="button"
                            variant="iconInline"
                            className="absolute right-4 top-1/2 -translate-y-1/2"
                            onClick={() => setShowPassword(!showPassword)}
                        >
                            {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                        </Button>
                    )}
                </div>

                {errorMessage && (
                    <span className="text-red-500 text-sm mt-1 font-semibold">
                        {errorMessage}
                    </span>
                )}
            </div>
        );
    }
);

InputField.displayName = "InputField";
