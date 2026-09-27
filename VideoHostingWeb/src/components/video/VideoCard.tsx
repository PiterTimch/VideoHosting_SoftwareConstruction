import { Link } from 'react-router-dom';
import { Play, Lock, EyeOff } from 'lucide-react';
import { motion } from 'framer-motion';
import { APP_ENV } from '../../env';
import type { IVideoItemResponse } from '../../types/Video/IVideoItemResponse';

interface VideoCardProps {
    video: IVideoItemResponse;
}

export function VideoCard({ video }: VideoCardProps) {
    const isPrivate = video.privacy?.systemCode === 'PRIVATE';
    const isUrlOnly = video.privacy?.systemCode === 'URL_ONLY';
    const isNotPublic = isPrivate || isUrlOnly;

    return (
        <motion.div
            whileHover={{ y: -4 }}
            className="group relative space-y-4"
        >
            <Link
                to={`/video/${video.slug}`}
                className="absolute inset-0 z-10"
                aria-label={`Дивитися відео: ${video.title}`}
            />

            <div className="relative aspect-video bg-zinc-900 rounded-[2rem] overflow-hidden border border-white/5 group-hover:border-rose-500/30 transition-colors duration-500">
                <img
                    src={video.image ? `${APP_ENV.IMAGES_400_URL}${video.image}` : '/placeholder.jpg'}
                    alt={video.title}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
                />

                {isNotPublic && (
                    <div className="absolute top-4 left-4 z-20 flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-black/60 backdrop-blur-md border border-white/10 text-xs font-semibold text-zinc-200 shadow-lg">
                        {isPrivate ? (
                            <>
                                <Lock size={12} className="text-rose-400" />
                                <span>Приватне</span>
                            </>
                        ) : (
                            <>
                                <EyeOff size={12} className="text-amber-400" />
                                <span>За посиланням</span>
                            </>
                        )}
                    </div>
                )}

                <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity bg-black/20">
                    <div className="w-12 h-12 bg-rose-600 rounded-full flex items-center justify-center shadow-lg">
                        <Play size={20} fill="white" className="ml-0.5 text-white" />
                    </div>
                </div>
            </div>

            <div className="flex gap-3 px-1">
                <div className="flex-1 min-w-0 space-y-1">
                    <h3 className="font-bold text-base text-zinc-100 group-hover:text-rose-500 transition-colors line-clamp-2 leading-snug">
                        {video.title}
                    </h3>

                    <p className="text-zinc-500 text-xs font-semibold">
                        {video.viewCount} views • {video.dateCreated}
                    </p>
                </div>
            </div>
        </motion.div>
    );
}
