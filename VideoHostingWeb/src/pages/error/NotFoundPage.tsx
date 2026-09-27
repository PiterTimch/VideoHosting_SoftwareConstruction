import { Link } from 'react-router-dom';
import PageTransition from '../../components/layout/PageTransition';
import { Button } from '../../components/form/Button';

export default function NotFoundPage() {
    return (
        <PageTransition>
            <div className="min-h-[80vh] flex items-center justify-center p-6 text-center">
                <div className="max-w-md w-full bg-zinc-900/40 border border-white/5 backdrop-blur-xl p-8 rounded-[2.5rem] shadow-2xl space-y-6">
                    <h1 className="text-8xl font-black text-rose-500 tracking-tighter">404</h1>
                    <div className="space-y-2">
                        <h2 className="text-2xl font-bold text-zinc-100">Сторінку не знайдено</h2>
                        <p className="text-zinc-400 text-sm">
                            Сторінка, яку ви шукаєте, не існує або була переміщена.
                        </p>
                    </div>
                    <Link to="/" className="inline-block">
                        <Button variant="primary" className="rounded-2xl px-8">
                            На головну
                        </Button>
                    </Link>
                </div>
            </div>
        </PageTransition>
    );
}
