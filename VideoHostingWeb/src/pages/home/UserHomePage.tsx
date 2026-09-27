import { Link, useSearchParams } from 'react-router-dom';
import { Play } from 'lucide-react';
import { APP_ENV } from '../../env';
import { useSearchVideosQuery } from '../../services/api/apiVideos';
import { Button } from '../../components/form/Button';
import { VideoCard } from '../../components/video/VideoCard';
import { Pagination } from '../../components/ui/common/Pagination';

function UserHomePage() {
    const [searchParams, setSearchParams] = useSearchParams();
    const page = searchParams.get('page') ? Number(searchParams.get('page')) : 1;

    const { data, isLoading } = useSearchVideosQuery({
        itemPerPage: 30,
        page,
    });
    const videos = data?.items;

    const heroVideo = page === 1 ? videos?.[0] : null;
    const gridVideos = page === 1 ? videos?.slice(1) || [] : videos || [];

    const handlePageChange = (newPage: number) => {
        const nextParams = new URLSearchParams(searchParams);
        nextParams.set('page', newPage.toString());
        setSearchParams(nextParams);
    };

    return (
        <div className="min-h-screen bg-theme-bg text-zinc-100 pb-12">

            {heroVideo && (
                <section className="grid grid-cols-1 lg:grid-cols-12 gap-3 lg:gap-8 items-center mb-8 pt-2 relative group px-3 sm:px-4 lg:px-0">
                    <Link
                        to={`/video/${heroVideo.slug}`}
                        className="absolute inset-0 z-10"
                        aria-label={`Дивитися відео: ${heroVideo.title}`}
                    />

                    <div className="lg:col-span-7 aspect-video bg-zinc-900 rounded-[2rem] overflow-hidden relative border border-white/5 shadow-2xl">
                        <img
                            src={heroVideo.image ? `${APP_ENV.IMAGES_400_URL}${heroVideo.image}` : '/placeholder.jpg'}
                            className="w-full h-full object-cover group-hover:scale-102 transition-transform duration-700"
                            alt={heroVideo.title}
                        />
                        <div className="absolute inset-0 bg-black/10 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                            <Button variant="play" tabIndex={-1}>
                                <Play size={26} fill="white" className="ml-1 text-white" />
                            </Button>
                        </div>
                    </div>

                    <div className="lg:col-span-5 space-y-2 sm:space-y-4 px-2 sm:px-4 lg:px-0 lg:pr-4">
                        <h1 className="text-2xl sm:text-4xl md:text-5xl font-extrabold tracking-tight leading-tight text-zinc-100">
                            {heroVideo.title}
                        </h1>

                        <p className="text-zinc-400 text-sm md:text-base font-medium leading-relaxed max-w-md line-clamp-3">
                            {heroVideo.description}
                        </p>

                        <div className="text-zinc-500 text-sm font-semibold">
                            {heroVideo.viewCount} views • {heroVideo.dateCreated}
                        </div>
                    </div>
                </section>
            )}

            {!isLoading && (!videos || videos.length === 0) ? (
                <div className="flex flex-col items-center justify-center py-16 px-4 bg-zinc-900/20 border border-zinc-800/30 rounded-[2rem] text-center my-6">
                    <p className="text-zinc-300 text-sm md:text-base mb-4 font-semibold">
                        Наразі немає завантажених відеозаписів
                    </p>
                    <Link
                        to="/video/add"
                        className="inline-flex items-center justify-center px-6 py-2.5 rounded-full bg-gradient-to-r from-rose-600 to-pink-600 hover:from-rose-500 hover:to-pink-500 text-white font-bold text-sm transition-all duration-200 active:scale-[0.98] shadow-lg shadow-rose-500/20"
                    >
                        Завантажити відео
                    </Link>
                </div>
            ) : (
                <section>
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-x-6 gap-y-10">
                        {isLoading ? (
                            [...Array(6)].map((_, i) => (
                                <div key={i} className="space-y-4 animate-pulse">
                                    <div className="aspect-video bg-zinc-900 rounded-[2rem] border border-white/5" />
                                    <div className="flex gap-3">
                                        <div className="w-10 h-10 bg-zinc-900 rounded-full" />
                                        <div className="flex-1 space-y-2">
                                            <div className="h-4 bg-zinc-900 rounded w-3/4" />
                                            <div className="h-3 bg-zinc-900 rounded w-1/2" />
                                        </div>
                                    </div>
                                </div>
                            ))
                        ) : (
                            gridVideos.map((video) => (
                                <VideoCard key={video.id} video={video} />
                            ))
                        )}
                    </div>
                </section>
            )}

            {data && data.pagination.totalPages > 1 && (
                <div className="mt-12 flex justify-center">
                    <Pagination
                        currentPage={page}
                        totalPages={data.pagination.totalPages}
                        onChange={handlePageChange}
                    />
                </div>
            )}

        </div>
    );
}

export default UserHomePage;
