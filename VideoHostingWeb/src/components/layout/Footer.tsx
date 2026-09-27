import { Link } from 'react-router-dom';

export function Footer() {
    return (
        <footer className="bg-zinc-950 border-t border-zinc-800/60 py-8 px-4 md:px-8 text-zinc-400 text-xs">
            <div className="max-w-[1400px] mx-auto flex flex-col md:flex-row items-center justify-between gap-4">
                <div className="flex items-center gap-2">
                    <span className="font-black text-sm tracking-tight text-white">VideoHosting</span>
                    <span className="text-zinc-600">•</span>
                    <span>© {new Date().getFullYear()} Всі права захищені.</span>
                </div>
                <div className="flex items-center gap-6">
                    <Link to="/" className="hover:text-white transition-colors">Головна</Link>
                    <Link to="/video/add" className="hover:text-white transition-colors">Додати відео</Link>
                </div>
            </div>
        </footer>
    );
}

export default Footer;
