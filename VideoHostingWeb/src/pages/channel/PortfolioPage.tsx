import { useParams } from 'react-router-dom';
import { useGetAllVideosQuery } from '../../services/api/apiVideos';
import LoadingOverlay from '../../components/ui/loading/LoadingOverlay';
import { VideoCard } from '../../components/video/VideoCard';

function PortfolioPage() {
    const { authorId } = useParams<{ authorId: string }>();
    const id = Number(authorId);
    const { data: videos = [], isLoading, isError } = useGetAllVideosQuery();
    const portfolioVideos = videos.filter(video => video.portfolio?.ownerId === id && video.privacy?.systemCode === 'PUBLIC');

    if (isLoading) return <LoadingOverlay />;
    if (!Number.isInteger(id) || id <= 0 || isError) return <p className="p-10 text-center text-zinc-400">Не вдалося завантажити портфоліо автора.</p>;

    return (
        <main className="mx-auto min-h-screen max-w-7xl space-y-8 p-5 text-zinc-100 md:p-10">
            <header className="rounded-3xl border border-white/10 bg-zinc-900/60 p-8">
                <p className="text-xs font-bold uppercase tracking-[0.25em] text-rose-400">Портфоліо автора</p>
                <h1 className="mt-3 text-3xl font-black">Відео автора</h1>
                <p className="mt-2 text-zinc-400">Публічні роботи автора та місце для підписки.</p>
                <p className="mt-4 text-sm text-zinc-500">Підписки стануть доступні, коли API каналів буде реалізовано на сервері.</p>
            </header>
            {portfolioVideos.length ? (
                <section className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
                    {portfolioVideos.map(video => <VideoCard key={video.id} video={video} />)}
                </section>
            ) : <p className="rounded-2xl border border-white/10 p-8 text-center text-zinc-400">У портфоліо поки немає публічних відео.</p>}
        </main>
    );
}

export default PortfolioPage;
