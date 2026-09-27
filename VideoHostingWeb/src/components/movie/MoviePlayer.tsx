import React, { useState, useRef, useEffect } from 'react';
import { Button } from '../form/Button';
import {
    Play,
    Pause,
    Volume2,
    VolumeX,
    Maximize,
    Settings,
    RotateCcw,
    RotateCw,
} from 'lucide-react';
import { APP_ENV, VIDEO_QUALITIES, type VideoQuality } from "../../env";
import { useAppDispatch, useAppSelector } from '../../store';
import { setVolume, setIsMuted } from '../../store/slices/playerSlice';

interface MoviePlayerProps {
    videoName?: string;
    src?: string;
}

type Quality = VideoQuality;

export function MoviePlayer({ videoName, src }: MoviePlayerProps) {
    const videoRef = useRef<HTMLVideoElement>(null);
    const containerRef = useRef<HTMLDivElement>(null);
    const dispatch = useAppDispatch();

    const volume = useAppSelector((state) => state.player.volume);
    const isMuted = useAppSelector((state) => state.player.isMuted);

    const [isPlaying, setIsPlaying] = useState(false);
    const [currentTime, setCurrentTime] = useState(0);
    const [duration, setDuration] = useState(0);
    const [showControls, setShowControls] = useState(true);
    const [quality, setQuality] = useState<Quality>('1080');
    const [showSettings, setShowSettings] = useState(false);

    const controlsTimeoutRef = useRef<number | null>(null);

    const qualities = VIDEO_QUALITIES;

    const getUrlForQuality = (q: Quality) => {
        if (src) return src;
        if (!videoName) return '';
        switch (q) {
            case '1080': return APP_ENV.VIDEO_1080_URL + videoName;
            case '720': return APP_ENV.VIDEO_720_URL + videoName;
            case '480': return APP_ENV.VIDEO_480_URL + videoName;
            case '360': return APP_ENV.VIDEO_360_URL + videoName;
            default: return APP_ENV.VIDEO_1080_URL + videoName;
        }
    };

    const togglePlay = () => {
        if (videoRef.current) {
            if (isPlaying) {
                videoRef.current.pause();
            } else {
                videoRef.current.play();
            }
            setIsPlaying(!isPlaying);
        }
    };

    const handleTimeUpdate = () => {
        if (videoRef.current) {
            setCurrentTime(videoRef.current.currentTime);
        }
    };

    const handleLoadedMetadata = () => {
        if (videoRef.current) {
            setDuration(videoRef.current.duration);
        }
    };

    const handleSeek = (e: React.ChangeEvent<HTMLInputElement>) => {
        const time = parseFloat(e.target.value);
        if (videoRef.current) {
            videoRef.current.currentTime = time;
            setCurrentTime(time);
        }
    };

    const handleVolumeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = parseFloat(e.target.value);
        dispatch(setVolume(val));
        if (val === 0) {
            dispatch(setIsMuted(true));
        } else if (isMuted) {
            dispatch(setIsMuted(false));
        }
    };

    const toggleMute = () => {
        const newMuted = !isMuted;
        dispatch(setIsMuted(newMuted));
        if (!newMuted && volume === 0) {
            dispatch(setVolume(0.5));
        }
    };

    const toggleFullscreen = async () => {
        if (containerRef.current) {
            if (!document.fullscreenElement) {
                try {
                    if (containerRef.current.requestFullscreen) {
                        await containerRef.current.requestFullscreen();
                    }
                } catch (e) {
                    console.error("Fullscreen error:", e);
                }
            } else {
                if (document.exitFullscreen) {
                    await document.exitFullscreen().catch(() => {});
                }
            }
        }
    };

    const changeQuality = (newQuality: Quality) => {
        if (videoRef.current) {
            const prevTime = videoRef.current.currentTime;
            const prevIsPlaying = isPlaying;
            setQuality(newQuality);
            setShowSettings(false);

            const handleCanPlay = () => {
                if (videoRef.current) {
                    videoRef.current.currentTime = prevTime;
                    if (prevIsPlaying) videoRef.current.play();
                    videoRef.current.removeEventListener('canplay', handleCanPlay);
                }
            };
            videoRef.current.addEventListener('canplay', handleCanPlay);
        }
    };

    const formatTime = (time: number) => {
        const minutes = Math.floor(time / 60);
        const seconds = Math.floor(time % 60);
        return `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
    };

    const handleMouseMove = () => {
        setShowControls(true);
        if (controlsTimeoutRef.current) {
            window.clearTimeout(controlsTimeoutRef.current);
        }
        controlsTimeoutRef.current = window.setTimeout(() => {
            if (isPlaying) setShowControls(false);
        }, 3000);
    };

    useEffect(() => {
        return () => {
            if (controlsTimeoutRef.current) {
                window.clearTimeout(controlsTimeoutRef.current);
            }
        };
    }, []);

    const skip = (amount: number) => {
        if (videoRef.current) {
            videoRef.current.currentTime += amount;
        }
    };

    return (
        <div 
            ref={containerRef}
            className="relative group rounded-xl overflow-hidden bg-black aspect-video flex items-center justify-center select-none"
            onMouseMove={handleMouseMove}
            onMouseLeave={() => isPlaying && setShowControls(false)}
        >
            <video
                ref={(el) => {
                    if (el) {
                        el.volume = volume;
                        el.muted = isMuted;
                    }
                    (videoRef as React.MutableRefObject<HTMLVideoElement | null>).current = el;
                }}
                src={getUrlForQuality(quality)}
                className="w-full h-full cursor-pointer"
                onClick={togglePlay}
                onTimeUpdate={handleTimeUpdate}
                onLoadedMetadata={handleLoadedMetadata}
                onPlay={() => setIsPlaying(true)}
                onPause={() => setIsPlaying(false)}
                playsInline
            />

            {!isPlaying && (
                <div 
                    className="absolute inset-0 flex items-center justify-center bg-black/20 cursor-pointer"
                    onClick={togglePlay}
                >
                    <div className="w-20 h-20 bg-[#FF2D7A] rounded-full flex items-center justify-center shadow-2xl transform hover:scale-110 transition-transform">
                        <Play fill="white" stroke="none" size={40} className="ml-1" />
                    </div>
                </div>
            )}

            <div className={`absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/90 via-black/40 to-transparent pt-8 pb-1 transition-opacity duration-300 ${showControls ? 'opacity-100' : 'opacity-0'}`}>

                <div className="flex items-center justify-between px-4 mb-2">
                    <div className="flex items-center gap-3">
                        <Button
                            variant="player"
                            onClick={togglePlay}
                        >
                            {isPlaying ? <Pause size={16} fill="currentColor" /> : <Play size={16} fill="currentColor" />}
                        </Button>
                        
                        <div className="flex items-center gap-1">
                             <Button
                                 variant="playerMuted"
                                 onClick={() => skip(-10)}
                             >
                                <RotateCcw size={16} />
                             </Button>
                             <Button
                                 variant="playerMuted"
                                 onClick={() => skip(10)}
                             >
                                <RotateCw size={16} />
                             </Button>
                        </div>

                        <div className="flex items-center group/volume ml-1">
                            <Button
                                variant="player"
                                onClick={toggleMute}
                            >
                                {isMuted || volume === 0 ? <VolumeX size={16} /> : <Volume2 size={16} />}
                            </Button>
                            <div className="w-0 overflow-hidden group-hover/volume:w-24 group-hover/volume:ml-2 transition-all duration-300 flex items-center">
                                <input
                                    type="range"
                                    min="0"
                                    max="1"
                                    step="0.1"
                                    value={isMuted ? 0 : volume}
                                    onChange={handleVolumeChange}
                                    className="w-24 h-1 bg-zinc-600 rounded-lg appearance-none cursor-pointer accent-[#FF2D7A]"
                                />
                            </div>
                        </div>

                        <div className="text-white text-sm font-medium ml-2">
                            {formatTime(currentTime)} / {formatTime(duration)}
                        </div>
                    </div>

                    <div className="flex items-center gap-3 relative">
                        {!src && (
                            <div className="relative">
                                <Button
                                    variant="player"
                                    onClick={() => setShowSettings(!showSettings)}
                                    className={`transition-all duration-300 ${showSettings ? 'rotate-90 text-[#FF2D7A]' : ''}`}
                                >
                                    <Settings size={16} />
                                </Button>

                                {showSettings && (
                                    <div className="absolute bottom-full right-0 mb-4 bg-zinc-900 border border-zinc-700 rounded-xl overflow-hidden shadow-2xl min-w-[120px]">
                                        <div className="p-2 border-b border-zinc-700 text-xs font-bold text-zinc-300 uppercase tracking-wider">
                                            Якість
                                        </div>
                                        {qualities.map((q) => (
                                            <Button
                                                key={q}
                                                variant="menuItem"
                                                active={quality === q}
                                                onClick={() => changeQuality(q)}
                                            >
                                                {q}p
                                            </Button>
                                        ))}
                                    </div>
                                )}
                            </div>
                        )}

                        <Button
                            variant="player"
                            onClick={toggleFullscreen}
                        >
                            <Maximize size={16} />
                        </Button>
                    </div>
                </div>

                <div className="px-4 pb-2 relative">
                    <div className="relative w-full h-1.5 flex items-center group/progress">
                        <div className="absolute w-full h-1 bg-zinc-600 rounded-full" />

                        <div
                            className="absolute h-1 bg-[#FF2D7A] rounded-full pointer-events-none z-10"
                            style={{ width: `${(currentTime / (duration || 1)) * 100}%` }}
                        />

                        <input
                            type="range"
                            min="0"
                            max={duration || 0}
                            step="any"
                            value={currentTime}
                            onChange={handleSeek}
                            className="absolute w-full h-6 opacity-0 cursor-pointer z-20"
                        />

                        <div
                            className="absolute h-3 w-3 bg-[#FF2D7A] rounded-full shadow-lg opacity-0 group-hover/progress:opacity-100 transition-opacity pointer-events-none z-10"
                            style={{
                                left: `calc(${(currentTime / (duration || 1)) * 100}% - 6px)`
                            }}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
}
