import { ChevronLeft } from 'lucide-react';
import type { ButtonHTMLAttributes } from 'react';
import { Button } from '../../form/Button';

interface BackButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    label: string;
}

export const BackButton = ({ label, className = '', ...props }: BackButtonProps) => (
    <Button
        type="button"
        variant="ghost"
        className={`gap-2 mb-8 group ${className}`}
        icon={<ChevronLeft size={20} className="group-hover:-translate-x-1 transition-transform" />}
        {...props}
    >
        <span className="text-[10px] font-black uppercase tracking-widest">{label}</span>
    </Button>
);
