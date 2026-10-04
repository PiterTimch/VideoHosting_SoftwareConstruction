import { useState, useEffect } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { Plus, Video, User as UserIcon } from 'lucide-react';
import { Button } from '../form/Button';
import { themes, applyTheme, getActiveTheme, initThemeSystem } from '../../themes';
import { SearchAutocomplete } from '../form/SearchAutocomplete';
import { SelectField } from '../form/SelectField';
import { useAppSelector } from '../../store';
import { APP_ENV } from '../../env';

function Header() {
    const [scrolled, setScrolled] = useState(false);
    const [activeTheme, setActiveTheme] = useState(() => getActiveTheme());
    const navigate = useNavigate();
    const location = useLocation();

    const isSearchPage = location.pathname.startsWith('/search');
    const { user } = useAppSelector(state => state.auth);

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

                {user ? (
                    <Button
                        variant="profile"
                        onClick={() => navigate('/account')}
                        className="group flex items-center gap-0 sm:gap-2 bg-zinc-800 hover:bg-zinc-700 p-1 sm:py-1.5 sm:px-3 rounded-full border border-zinc-700 transition-all shrink-0 cursor-pointer"
                    >
                        <div className="w-7 h-7 sm:w-6 sm:h-6 rounded-full overflow-hidden border border-rose-500/30 flex items-center justify-center bg-zinc-900">
                            {user.image ? (
                                <img
                                    src={`${APP_ENV.IMAGES_400_URL}${user.image}`}
                                    alt={user.name}
                                    className="w-full h-full object-cover"
                                />
                            ) : (
                                <UserIcon size={14} className="text-zinc-400" />
                            )}
                        </div>
                        <span className="text-xs font-bold text-zinc-200 group-hover:text-white transition-colors hidden sm:inline">
                            {user.name}
                        </span>
                    </Button>
                ) : (
                    <button
                        onClick={() => navigate('/login')}
                        className="relative shrink-0 w-auto text-[10px] sm:text-xs font-extrabold tracking-wider uppercase p-[2px] rounded-full text-white overflow-hidden transition-all duration-300 active:scale-95 group shadow-[0_0_15px_rgba(236,72,153,0.15)] hover:shadow-[0_0_25px_rgba(236,72,153,0.35)] cursor-pointer"
                        style={{
                            background: 'rgb(var(--color-bg))',
                            backgroundImage: 'linear-gradient(to right, #f43f5e, #ec4899, #a855f7)',
                        }}
                    >
                        <span
                            className="block px-3 py-1.5 sm:px-5 sm:py-2 rounded-full bg-theme-bg text-zinc-200 group-hover:text-white group-hover:bg-theme-bg/80 transition-all duration-300 whitespace-nowrap">
                            Увійти
                        </span>
                    </button>
                )}
            </div>
        </header>
    );
}

export default Header;
