import { useState, useEffect } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { Plus, Video } from 'lucide-react';
import { Button } from '../form/Button';
import { themes, applyTheme, getActiveTheme, initThemeSystem } from '../../themes';
import { SearchAutocomplete } from '../form/SearchAutocomplete';
import { SelectField } from '../form/SelectField';

function Header() {
    const [scrolled, setScrolled] = useState(false);
    const [activeTheme, setActiveTheme] = useState(() => getActiveTheme());
    const navigate = useNavigate();
    const location = useLocation();

    const isSearchPage = location.pathname.startsWith('/search');

    useEffect(() => {
        const handleScroll = () => {
            setScrolled(window.scrollY > 10);
        };
        window.addEventListener('scroll', handleScroll);
        const cleanupTheme = initThemeSystem((newTheme) => {
            setActiveTheme(newTheme);
        });
        return () => {
            window.removeEventListener('scroll', handleScroll);
            cleanupTheme();
        };
    }, []);

    const handleThemeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const nextTheme = e.target.value;
        applyTheme(nextTheme);
        setActiveTheme(nextTheme);
    };

    return (
        <header
            className={`h-14 sticky top-0 z-[40] px-2 sm:px-4 md:px-6 flex items-center justify-between gap-1.5 sm:gap-3 transition-all duration-300 ${scrolled ? 'bg-theme-bg/90 backdrop-blur-xl shadow-lg shadow-black/20' : 'bg-transparent'}`}
        >
            <div className="flex items-center shrink-0">
                <Link to="/" className="flex items-center shrink-0 md:hidden active:scale-95 transition-transform">
                    <div className="w-8 h-8 rounded-xl bg-gradient-to-tr from-rose-600 to-pink-500 flex items-center justify-center text-white shadow-md">
                        <Video size={18} />
                    </div>
                    <span
                        className="ml-1.5 text-sm sm:text-base font-black tracking-tight bg-clip-text text-transparent hidden sm:inline"
                        style={{
                            backgroundImage: 'linear-gradient(to right, #f43f5e, #ec4899, #a855f7)',
                        }}
                    >
                        VideoHosting
                    </span>
                </Link>

                {isSearchPage && (
                    <h2 className="text-sm font-black uppercase tracking-widest text-zinc-400 hidden md:block">
                        Результати <span className="text-rose-500">пошуку</span>
                    </h2>
                )}
            </div>

            <div className="flex-1 min-w-0 max-w-[170px] xs:max-w-[220px] sm:max-w-xs md:max-w-md lg:max-w-xl mx-1 sm:mx-4">
                <SearchAutocomplete />
            </div>

            <div className="flex items-center gap-1.5 sm:gap-3 md:gap-4 shrink-0">
                <Button
                    variant="icon"
                    onClick={() => navigate('/video/add')}
                    className="flex items-center justify-center bg-zinc-800 p-2.5 rounded-full border border-zinc-700/40 text-zinc-300 hover:text-white hover:bg-zinc-700 transition-all"
                    title="Додати відео"
                >
                    <Plus size={20}/>
                </Button>

                <div className="block min-w-[90px] xs:min-w-[110px] sm:min-w-[130px]">
                    <SelectField
                        name="theme"
                        variant="filter"
                        value={activeTheme}
                        options={themes}
                        onChange={handleThemeChange}
                    />
                </div>
            </div>
        </header>
    );
}

export default Header;
