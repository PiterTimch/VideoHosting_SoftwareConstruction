import { Link } from 'react-router-dom';
import { useGetAllVideosQuery, useDeleteVideoMutation } from '../../services/api/apiVideos';
import { VideoCard } from '../../components/video/VideoCard';
import LoadingOverlay from '../../components/ui/loading/LoadingOverlay';

function AdminPage() {
    const { data: videos = [], isLoading, isError } = useGetAllVideosQuery();
    const [deleteVideo, { isLoading: isDeleting }] = useDeleteVideoMutation();
    if (isLoading) return <LoadingOverlay />;

    return (
        <main className="mx-auto min-h-screen max-w-7xl space-y-8 p-5 text-zinc-100 md:p-10">
            <header className="flex flex-wrap items-center justify-between gap-4 rounded-3xl border border-white/10 bg-zinc-900/60 p-8">
                <div><p className="text-xs font-bold uppercase tracking-[0.25em] text-rose-400">Керування платформою</p><h1 className="mt-3 text-3xl font-black">Адмінпанель</h1></div>
                <span className="rounded-full bg-rose-500/10 px-4 py-2 text-sm text-rose-300">Відео: {videos.length}</span>
            </header>
            <section>
                <h2 className="mb-5 text-xl font-bold">Відео</h2>
                {isError ? <p className="text-rose-300">Не вдалося завантажити відео. Перевірте доступність API.</p> : videos.length === 0 ? <p className="text-zinc-400">Відео не знайдені.</p> :
                    <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
                        {videos.map(video => <article key={video.id} className="relative rounded-2xl border border-white/10 bg-zinc-900/40 p-3">
                            <VideoCard video={video} />
                            <div className="mt-3 flex items-center justify-between gap-3">
                                <Link to={`/portfolio/${video.portfolio?.ownerId}`} className="text-xs text-zinc-400 hover:text-rose-300">Відкрити портфоліо автора</Link>
                                <button type="button" disabled={isDeleting} onClick={() => void deleteVideo({ id: video.id })} className="rounded-lg bg-red-500/10 px-3 py-2 text-xs font-bold text-red-300 hover:bg-red-500/20 disabled:opacity-50">Видалити</button>
                            </div>
                        </article>)}
                    </div>}
            </section>
            <p className="text-sm text-zinc-500">Керування користувачами та довідниками потребує відповідних реалізованих endpoint-ів сервера; поточний API їх не надає.</p>
        </main>
    );
}

export default AdminPage;
