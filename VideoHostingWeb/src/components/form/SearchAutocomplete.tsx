import { useState, useEffect, useRef } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Search } from "lucide-react";
import { useAutocompleteVideosQuery } from "../../services/api/apiVideos";
import { APP_ENV } from "../../env";
import { Button } from "./Button";

export function SearchAutocomplete() {
    const [searchParams] = useSearchParams();
    const query = searchParams.get('q') || '';
    const [searchVal, setSearchVal] = useState(query);
    const [debouncedVal, setDebouncedVal] = useState("");
    const [isOpen, setIsOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);
    const navigate = useNavigate();

    useEffect(() => {
        setSearchVal(query);
    }, [query]);

    useEffect(() => {
        const handler = setTimeout(() => {
            setDebouncedVal(searchVal);
        }, 1000);

        return () => {
            clearTimeout(handler);
        };
    }, [searchVal]);

    const { data: suggestions = [] } = useAutocompleteVideosQuery(debouncedVal, {
        skip: !debouncedVal.trim(),
    });

    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    const handleFormSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        setIsOpen(false);
        if (searchVal.trim()) {
            navigate(`/search?q=${encodeURIComponent(searchVal.trim())}`);
        } else {
            navigate("/search");
        }
    };

    const handleSuggestionClick = (slug: string) => {
        setIsOpen(false);
        navigate(`/video/${slug}`);
    };

    return (
        <div ref={containerRef} className="relative w-full">
            <form onSubmit={handleFormSubmit} className="flex items-center gap-2">
                <div className="relative w-full">
                    <input
                        type="text"
                        placeholder="Пошук"
                        value={searchVal}
                        onChange={(e) => {
                            setSearchVal(e.target.value);
                            setIsOpen(true);
                        }}
                        onFocus={() => setIsOpen(true)}
                        className="w-full bg-zinc-800/90 border border-zinc-700/60 rounded-xl py-2 pl-5 pr-12 text-sm font-medium focus:outline-none focus:border-rose-500/50 focus:bg-zinc-800 transition-all text-zinc-100 placeholder-zinc-500"
                    />
                    <Button type="submit" variant="iconInline" className="absolute right-4 top-2.5">
                        <Search size={20} strokeWidth={2.5} />
                    </Button>
                </div>
            </form>

            {isOpen && searchVal.trim() && suggestions.length > 0 && (
                <div className="absolute left-0 right-0 mt-2 bg-zinc-800 border border-zinc-700/80 rounded-xl shadow-2xl overflow-hidden z-[100] max-h-96 overflow-y-auto">
                    {suggestions.map((video) => (
                        <div
                            key={video.slug}
                            onClick={() => handleSuggestionClick(video.slug)}
                            className="flex items-center gap-3 px-4 py-2.5 hover:bg-zinc-900 cursor-pointer transition-colors"
                        >
                            <div className="w-12 h-8 rounded overflow-hidden bg-zinc-800 flex-shrink-0">
                                <img
                                    src={video.image ? `${APP_ENV.IMAGES_100_URL}${video.image}` : "/images/video/default.png"}
                                    alt={video.title}
                                    className="w-full h-full object-cover"
                                />
                            </div>
                            <span className="text-sm font-medium text-zinc-200 truncate hover:text-zinc-50">
                                {video.title}
                            </span>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}
