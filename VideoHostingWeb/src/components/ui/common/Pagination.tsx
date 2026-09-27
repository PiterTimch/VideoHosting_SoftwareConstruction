import { ChevronLeft, ChevronRight, MoreHorizontal } from 'lucide-react';
import { Button } from '../../form/Button';

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    onChange: (page: number) => void;
}

export function Pagination({ currentPage, totalPages, onChange }: PaginationProps) {
    if (totalPages <= 1) return null;

    const getPageNumbers = () => {
        const pages: (number | string)[] = [];
        const maxVisible = 5;

        if (totalPages <= maxVisible) {
            for (let i = 1; i <= totalPages; i++) pages.push(i);
        } else {
            pages.push(1);
            let start = Math.max(2, currentPage - 1);
            let end = Math.min(totalPages - 1, currentPage + 1);

            if (currentPage <= 3) {
                start = 2;
                end = 4;
            } else if (currentPage >= totalPages - 2) {
                start = totalPages - 3;
                end = totalPages - 1;
            }

            if (start > 2) pages.push('...');
            for (let i = start; i <= end; i++) pages.push(i);
            if (end < totalPages - 1) pages.push('...');
            pages.push(totalPages);
        }

        return pages;
    };

    return (
        <div className="flex items-center justify-center gap-1.5 py-4">
            <Button
                variant="paginationNav"
                onClick={() => onChange(currentPage - 1)}
                disabled={currentPage === 1}
                aria-label="Попередня сторінка"
            >
                <ChevronLeft size={18} />
            </Button>

            <div className="flex items-center gap-1.5">
                {getPageNumbers().map((page, index) => {
                    if (page === '...') {
                        return (
                            <span key={`dots-${index}`} className="w-10 h-10 flex items-center justify-center text-zinc-500">
                                <MoreHorizontal size={16} />
                            </span>
                        );
                    }

                    const pageNum = page as number;
                    const isActive = pageNum === currentPage;

                    return (
                        <Button
                            key={pageNum}
                            variant="paginationPage"
                            active={isActive}
                            onClick={() => onChange(pageNum)}
                        >
                            {pageNum}
                        </Button>
                    );
                })}
            </div>

            <Button
                variant="paginationNav"
                onClick={() => onChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                aria-label="Наступна сторінка"
            >
                <ChevronRight size={18} />
            </Button>
        </div>
    );
}
