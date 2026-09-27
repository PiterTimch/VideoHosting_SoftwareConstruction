import React, { useState, useRef, useEffect, type SelectHTMLAttributes } from "react";
import { ChevronDown, Check } from "lucide-react";

export interface SelectFieldProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'error'> {
    label?: string;
    name: string;
    options: { id: string | number; name: string }[];
    error?: string | string[];
    wrapperClassName?: string;
    selectClassName?: string;
    variant?: 'default' | 'filter';
}

export const SelectField = React.forwardRef<HTMLSelectElement, SelectFieldProps>(
    ({
         label,
         name,
         value,
         options,
         error,
         onChange,
         wrapperClassName = '',
         selectClassName = '',
         variant = 'default',
         disabled,
     }) => {
        const errorMessage = Array.isArray(error) ? error[0] : error;
        const [isOpen, setIsOpen] = useState(false);
        const containerRef = useRef<HTMLDivElement>(null);

        const selectedOption = options.find(o => String(o.id) === String(value));

        useEffect(() => {
            const handleClickOutside = (e: MouseEvent) => {
                if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
                    setIsOpen(false);
                }
            };
            document.addEventListener('mousedown', handleClickOutside);
            return () => document.removeEventListener('mousedown', handleClickOutside);
        }, []);

        const handleSelect = (optionId: string | number) => {
            if (onChange) {
                const syntheticEvent = {
                    target: { value: String(optionId), name },
                } as React.ChangeEvent<HTMLSelectElement>;
                onChange(syntheticEvent);
            }
            setIsOpen(false);
        };

        const isFilter = variant === 'filter';

        const buttonClass = isFilter
            ? `flex items-center justify-between gap-2 w-full bg-zinc-800/60 hover:bg-zinc-800 text-zinc-200 border border-zinc-700/40 rounded-full py-2 pl-4 pr-3 text-sm font-medium transition-all cursor-pointer ${isOpen ? 'ring-1 ring-zinc-600 bg-zinc-800' : ''}`
            : `flex items-center justify-between gap-2 w-full bg-zinc-800 text-zinc-100 rounded-xl px-4 py-3 border ${errorMessage ? 'border-red-500' : 'border-zinc-700'} focus:border-red-500 text-sm transition-all cursor-pointer ${isOpen ? 'border-red-500' : ''} ${selectClassName}`;

        return (
            <div className={`flex flex-col ${wrapperClassName}`} ref={containerRef}>
                {label && (
                    <label className="text-zinc-400 mb-1 font-semibold text-sm">{label}</label>
                )}

                <div className="relative w-full">
                    <button
                        type="button"
                        disabled={disabled}
                        onClick={() => !disabled && setIsOpen(prev => !prev)}
                        className={buttonClass}
                        aria-haspopup="listbox"
                        aria-expanded={isOpen}
                    >
                        <span className="truncate">{selectedOption?.name ?? <span className="text-zinc-500 italic">Оберіть...</span>}</span>
                        <ChevronDown
                            size={15}
                            className={`shrink-0 text-zinc-400 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`}
                        />
                    </button>

                    {isOpen && (
                        <div
                            className={`
                                absolute z-[200] mt-2 w-full min-w-[160px]
                                bg-zinc-900/95 backdrop-blur-xl
                                border border-zinc-700/60
                                rounded-2xl shadow-2xl shadow-black/60
                                overflow-hidden
                                animate-in fade-in slide-in-from-top-2 duration-150
                                ${isFilter ? 'left-0' : 'left-0 right-0'}
                            `}
                            role="listbox"
                        >
                            <div className="h-px w-full bg-gradient-to-r from-rose-500/60 via-purple-500/40 to-blue-500/40" />

                            <div className="py-1.5 max-h-64 overflow-y-auto">
                                {options.map((option) => {
                                    const isSelected = String(option.id) === String(value);
                                    return (
                                        <button
                                            key={option.id}
                                            type="button"
                                            role="option"
                                            aria-selected={isSelected}
                                            onClick={() => handleSelect(option.id)}
                                            className={`
                                                w-full flex items-center justify-between
                                                px-4 py-2.5 text-sm font-medium
                                                transition-all duration-150 cursor-pointer text-left
                                                ${isSelected
                                                    ? 'text-rose-400 bg-rose-500/10'
                                                    : 'text-zinc-300 hover:text-zinc-100 hover:bg-zinc-800/60'
                                                }
                                            `}
                                        >
                                            <span className="truncate">{option.name}</span>
                                            {isSelected && (
                                                <Check size={14} className="shrink-0 ml-2 text-rose-500" />
                                            )}
                                        </button>
                                    );
                                })}
                            </div>

                            <div className="h-px w-full bg-gradient-to-r from-blue-500/20 via-purple-500/20 to-rose-500/20" />
                        </div>
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

SelectField.displayName = "SelectField";
