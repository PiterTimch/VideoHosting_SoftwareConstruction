import { useMemo, useState } from 'react';
import { Info, Loader2 } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAppSelector } from '../../store';
import { useGetPortfolioQuery, useTogglePortfolioSubscriptionMutation } from '../../services/api/apiPortfolios';
import { useSearchVideosQuery } from '../../services/api/apiVideos';
import { APP_ENV } from '../../env';
import { VideoCard } from '../../components/video/VideoCard';
import { Button } from '../../components/form/Button';
import { TabButtons } from '../../components/ui/common/TabButton';
import { Pagination } from '../../components/ui/common/Pagination';

type PortfolioTab = 'Головна' | 'Відео' | 'Про автора';

function PortfolioPage() {
    const { authorId } = useParams<{ authorId: string }>();
    const portfolioId = Number(authorId);
    const navigate = useNavigate();
    const { user } = useAppSelector(state => state.auth);
    const [activeTab, setActiveTab] = useState<PortfolioTab>('Головна');
    const [sortBy, setSortBy] = useState('date');
    const [page, setPage] = useState(1);
    const [subscriptionError, setSubscriptionError] = useState('');
    const validId = Number.isInteger(portfolioId) && portfolioId > 0;
    const { data: portfolio, isLoading: isPortfolioLoading, isError: isPortfolioError } = useGetPortfolioQuery(portfolioId, { skip: !validId });
    const [toggleSubscription, { isLoading: isSubscribing }] = useTogglePortfolioSubscriptionMutation();
    const isOwnPortfolio = user?.id === portfolioId;
    const videoRequest = useMemo(() => ({ portfolioId, sortBy: activeTab === 'Головна' ? 'views' : sortBy, page: activeTab === 'Головна' ? 1 : page, itemPerPage: activeTab === 'Головна' ? 5 : 24 }), [portfolioId, sortBy, activeTab, page]);
    const { data: videosPage, isLoading: areVideosLoading, isError: areVideosError } = useSearchVideosQuery(videoRequest, { skip: !validId || !portfolio || activeTab === 'Про автора' });

    const handleSubscription = async () => {
        if (!user) {
            navigate('/login');
            return;
        }
        setSubscriptionError('');
        try {
            await toggleSubscription(portfolioId).unwrap();
        } catch {
            setSubscriptionError('Не вдалося змінити підписку. Спробуйте ще раз.');
        }
    };

    if (isPortfolioLoading) return <div className="flex min-h-[50vh] items-center justify-center"><Loader2 className="animate-spin text-rose-500" size={40} /></div>;
    if (!validId || isPortfolioError || !portfolio) return <p className="p-8 text-center text-zinc-400">Портфоліо автора не знайдено.</p>;

    const videos = videosPage?.items ?? [];

    return (
        <main className="mx-auto max-w-[1600px] space-y-8 px-4 py-6 text-zinc-100 sm:px-6 lg:px-8">
            <div className="h-48 overflow-hidden rounded-[2rem] bg-zinc-800 md:h-64">
                {portfolio.bannerImage ? <img src={`${APP_ENV.IMAGES_1200_URL}${portfolio.bannerImage}`} alt={`Банер ${portfolio.name}`} className="h-full w-full object-cover" /> : <div className="h-full bg-gradient-to-r from-rose-950 via-zinc-800 to-zinc-900" />}
            </div>
            <header className="flex flex-col justify-between gap-6 sm:flex-row sm:items-center">
                <div className="flex items-center gap-5">
                    <div className="h-24 w-24 shrink-0 overflow-hidden rounded-full border-4 border-zinc-950 bg-zinc-800 sm:h-32 sm:w-32">
                        <img src={portfolio.avatarImage ? `${APP_ENV.IMAGES_400_URL}${portfolio.avatarImage}` : '/images/user/default.jpg'} alt={portfolio.name} className="h-full w-full object-cover" />
                    </div>
                    <div className="min-w-0">
                        <h1 className="text-2xl font-black sm:text-3xl">{portfolio.name}</h1>
                        <div className="mt-1 flex flex-wrap items-center gap-2 text-sm text-zinc-400">
                            <span className="text-zinc-300">@{portfolio.nickName}</span><span aria-hidden="true">•</span>
                            <span>{portfolio.subscriberCount} підписників</span>
                        </div>
                        {portfolio.specialization && <p className="mt-1 text-sm text-rose-300">{portfolio.specialization}</p>}
                    </div>
                </div>
                <div className="shrink-0">
                    {isOwnPortfolio ? <Button variant="secondary" size="lg" className="rounded-full" onClick={() => navigate(`/portfolio/${portfolioId}/edit`)}>Редагувати портфоліо</Button> :
                        <Button variant={portfolio.isSubscribed ? 'secondary' : 'primary'} size="lg" className="rounded-full" onClick={() => void handleSubscription()} disabled={isSubscribing}>
                            {isSubscribing ? 'Зачекайте…' : portfolio.isSubscribed ? 'Відписатися' : 'Підписатися'}
                        </Button>}
                </div>
            </header>
            {subscriptionError && <p role="alert" className="rounded-xl border border-red-500/20 bg-red-500/5 p-3 text-sm text-red-300">{subscriptionError}</p>}
            <div className="border-b border-white/5 pb-2"><TabButtons tabList={['Головна', 'Відео', 'Про автора']} activeTab={activeTab} onTabChange={tab => setActiveTab(tab as PortfolioTab)} /></div>

            {(activeTab === 'Головна' || activeTab === 'Відео') && <section className="space-y-6">
                <div className="flex items-center justify-between gap-4">
                    <h2 className="text-xl font-bold">{activeTab === 'Головна' ? 'Популярні відео' : 'Відео автора'}</h2>
                    {activeTab === 'Відео' && <div className="flex gap-2">
                        <Button size="sm" variant={sortBy === 'views' ? 'chip' : 'ghost'} onClick={() => { setSortBy('views'); setPage(1); }}>Популярні</Button>
                        <Button size="sm" variant={sortBy === 'date' ? 'chip' : 'ghost'} onClick={() => { setSortBy('date'); setPage(1); }}>Нові</Button>
                    </div>}
                </div>
                {areVideosLoading ? <div className="flex justify-center py-12"><Loader2 className="animate-spin text-rose-500" /></div> : areVideosError ? <p className="rounded-2xl border border-red-500/20 p-8 text-center text-red-300">Не вдалося завантажити відео портфоліо.</p> : videos.length === 0 ? <p className="rounded-2xl border border-white/10 p-8 text-center text-zinc-400">У портфоліо поки немає відео.</p> : <>
                    <div className="grid grid-cols-1 gap-x-4 gap-y-8 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">{videos.map(video => <VideoCard key={video.id} video={video} />)}</div>
                    {activeTab === 'Відео' && videosPage?.pagination && <Pagination currentPage={videosPage.pagination.currentPage} totalPages={videosPage.pagination.totalPages} onChange={setPage} />}
                </>}
            </section>}

            {activeTab === 'Про автора' && <section className="max-w-4xl rounded-[2rem] border border-zinc-800 bg-zinc-900/50 p-6 md:p-8">
                <h2 className="mb-6 flex items-center gap-2 text-xl font-bold"><Info size={24} className="text-rose-500" />Про автора</h2>
                <p className="whitespace-pre-wrap leading-relaxed text-zinc-300">{portfolio.description || 'Автор ще не додав опис.'}</p>
                {portfolio.contacts && <><hr className="my-6 border-zinc-800" /><h3 className="mb-2 font-semibold">Контакти</h3><p className="whitespace-pre-wrap text-zinc-400">{portfolio.contacts}</p></>}
                <hr className="my-6 border-zinc-800" />
                <div className="flex justify-between rounded-xl border border-zinc-800 bg-zinc-900 p-4 text-sm"><span className="font-semibold text-zinc-300">Підписників</span><span>{portfolio.subscriberCount}</span></div>
            </section>}
        </main>
    );
}

export default PortfolioPage;
