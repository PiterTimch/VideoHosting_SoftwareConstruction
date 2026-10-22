import { useState } from "react";
import { useVideoAccessToken } from "../../hooks/useVideoAccessToken";

export function VideoAccessTokenPanel({ videoId, slug }: { videoId: number; slug: string }) {
    const { token, expiresAt, generate, isLoading } = useVideoAccessToken(videoId);
    const [copied, setCopied] = useState(false);
    const shareUrl = token ? `${window.location.origin}/video/${encodeURIComponent(slug)}?token=${encodeURIComponent(token)}` : "";

    const copyLink = async () => {
        await navigator.clipboard.writeText(shareUrl);
        setCopied(true);
    };

    return <section className="space-y-3 rounded-xl border border-zinc-700 p-4" aria-label="Video access link">
        <button type="button" onClick={() => void generate()} disabled={isLoading} className="rounded-lg bg-pink-600 px-4 py-2 text-white disabled:opacity-50">
            {isLoading ? "Generating…" : "Generate link"}
        </button>
        {shareUrl && <div className="space-y-2">
            <input aria-label="Shareable video URL" readOnly value={shareUrl} className="w-full rounded bg-zinc-800 p-2 text-sm" />
            <button type="button" onClick={() => void copyLink()} className="rounded bg-zinc-700 px-3 py-2">{copied ? "Copied" : "Copy link"}</button>
            {expiresAt && <p className="text-xs text-zinc-400">Expires {new Date(expiresAt).toLocaleString()}</p>}
        </div>}
    </section>;
}
