export function LoadingOverlay() {
    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-md transition-opacity">
            <div className="flex flex-col items-center gap-4">
                <div className="relative flex items-center justify-center">
                    <div className="w-14 h-14 border-4 border-rose-500/20 rounded-full animate-pulse" />
                    <div className="absolute w-14 h-14 border-4 border-t-rose-500 border-r-pink-500 border-b-transparent border-l-transparent rounded-full animate-spin" />
                </div>
                <p className="text-xs font-bold uppercase tracking-widest text-zinc-400 animate-pulse">
                    Завантаження...
                </p>
            </div>
        </div>
    );
}

export default LoadingOverlay;
