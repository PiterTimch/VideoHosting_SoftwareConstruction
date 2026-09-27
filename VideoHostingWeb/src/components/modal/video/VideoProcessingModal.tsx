import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import * as signalR from "@microsoft/signalr";
import { Modal, Progress } from "antd";
import { APP_ENV } from "../../../env";
import { buildApiUrl } from "../../../utils/buildApiUrl";
import { errorVideoProgress, logVideoProgress, warnVideoProgress } from "../../../utils/videoProgressLogger";
import type { IVideoProcessingResponse } from "../../../types/Video/IVideoProcessingResponse";

const POLL_INTERVAL_MS = 5000;

const isCompletedStatus = (status?: string) =>
    status === "Completed" || status === "Завершено";

function normalizeProgress(data: Record<string, unknown>): IVideoProcessingResponse {
    return {
        percentage: Number(data.percentage ?? data.Percentage ?? 0),
        status: String(data.status ?? data.Status ?? ""),
        estimatedTimeRemaining: String(
            data.estimatedTimeRemaining ?? data.EstimatedTimeRemaining ?? "",
        ),
    };
}

function buildHubConnection(): signalR.HubConnection {
    logVideoProgress("SignalR: building connection", {
        hubUrl: APP_ENV.SIGNALR_HUB_URL,
        apiBaseUrl: APP_ENV.API_BASE_URL,
    });

    return new signalR.HubConnectionBuilder()
        .withUrl(APP_ENV.SIGNALR_HUB_URL, {
            transport:
                signalR.HttpTransportType.WebSockets
                | signalR.HttpTransportType.ServerSentEvents
                | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();
}

async function safeJoinChannel(
    connection: signalR.HubConnection,
    channelId: string,
) {
    logVideoProgress("SignalR: JoinChannel attempt", {
        channelId,
        state: signalR.HubConnectionState[connection.state],
    });

    if (connection.state !== signalR.HubConnectionState.Connected) {
        warnVideoProgress("SignalR: JoinChannel skipped — not connected", {
            state: signalR.HubConnectionState[connection.state],
        });
        return;
    }

    try {
        await connection.invoke("JoinChannel", channelId);
        logVideoProgress("SignalR: JoinChannel success", { channelId });
    } catch (error) {
        errorVideoProgress("SignalR: JoinChannel failed", error);
    }
}

async function startHubConnection(
    connection: signalR.HubConnection,
    trackingId: string,
): Promise<boolean> {
    logVideoProgress("SignalR: start attempt", {
        trackingId,
        state: signalR.HubConnectionState[connection.state],
    });

    try {
        if (connection.state !== signalR.HubConnectionState.Disconnected) {
            logVideoProgress("SignalR: stopping stale connection before start");
            await connection.stop();
        }
    } catch (error) {
        warnVideoProgress("SignalR: stop before start failed (ignored)", error);
    }

    try {
        await connection.start();
        logVideoProgress("SignalR: start success", {
            connectionId: connection.connectionId,
        });
        await safeJoinChannel(connection, trackingId);
        return true;
    } catch (error) {
        warnVideoProgress("SignalR: start failed — fallback to HTTP polling", error);
        return false;
    }
}

async function stopHubConnection(connection: signalR.HubConnection) {
    logVideoProgress("SignalR: stop", {
        state: signalR.HubConnectionState[connection.state],
    });

    if (connection.state === signalR.HubConnectionState.Disconnected) {
        return;
    }

    try {
        await connection.stop();
        logVideoProgress("SignalR: stop success");
    } catch (error) {
        warnVideoProgress("SignalR: stop failed (ignored)", error);
    }
}

async function fetchProgressFromApi(trackingId: string): Promise<IVideoProcessingResponse | null> {
    const url = buildApiUrl(`Videos/progress/${encodeURIComponent(trackingId)}`);
    logVideoProgress("Poll: GET progress", { url, trackingId });

    const response = await fetch(url, { 
        credentials: 'include'
    });

    logVideoProgress("Poll: GET progress response", {
        status: response.status,
        ok: response.ok,
    });

    if (!response.ok) {
        warnVideoProgress("Poll: progress not found or error", {
            status: response.status,
            trackingId,
        });
        return null;
    }

    const data = await response.json();
    const normalized = normalizeProgress(data);
    logVideoProgress("Poll: progress received", { raw: data, normalized });
    return normalized;
}

async function fetchVideoReadyBySlug(videoSlug: string): Promise<boolean> {
    if (!videoSlug) {
        return false;
    }

    const url = buildApiUrl(`Videos/get-by?slug=${encodeURIComponent(videoSlug)}`);
    logVideoProgress("Poll: GET video by slug", { url, videoSlug });

    const response = await fetch(url, { 
        credentials: 'include'
    });

    logVideoProgress("Poll: GET video by slug response", {
        status: response.status,
        ok: response.ok,
    });

    if (!response.ok) {
        return false;
    }

    const data = await response.json();
    const video = data.video ?? data.Video;
    const isReady = Boolean(video && video !== "processing...");

    logVideoProgress("Poll: video ready check", { video, isReady });
    return isReady;
}

function useVideoProgress(trackingId: string | null, videoSlug: string) {
    const [progress, setProgress] = useState<IVideoProcessingResponse | null>(null);
    const [isLive, setIsLive] = useState(false);
    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const trackingIdRef = useRef(trackingId);
    const videoSlugRef = useRef(videoSlug);

    const applyProgress = useCallback((source: string, update: IVideoProcessingResponse) => {
        setProgress((prev) => {
            if (
                prev
                && prev.percentage === update.percentage
                && prev.status === update.status
                && prev.estimatedTimeRemaining === update.estimatedTimeRemaining
            ) {
                return prev;
            }

            logVideoProgress("Progress: updated", { source, update });
            return update;
        });
    }, []);

    const pollProgress = useCallback(async (reason: string) => {
        const id = trackingIdRef.current;
        if (!id) {
            warnVideoProgress("Poll: skipped — no trackingId", { reason });
            return;
        }

        logVideoProgress("Poll: start", { reason, trackingId: id, videoSlug: videoSlugRef.current });

        try {
            const update = await fetchProgressFromApi(id);
            if (update) {
                applyProgress("poll-api", update);
                if (isCompletedStatus(update.status)) {
                    logVideoProgress("Poll: completed via progress API");
                    return;
                }
            }

            const slug = videoSlugRef.current;
            if (slug && await fetchVideoReadyBySlug(slug)) {
                logVideoProgress("Poll: completed via video slug check");
                applyProgress("poll-slug", {
                    percentage: 100,
                    status: "Завершено",
                    estimatedTimeRemaining: "00:00:00",
                });
            }
        } catch (error) {
            errorVideoProgress("Poll: failed", { reason, error });
        }
    }, [applyProgress]);

    const connectHub = useCallback(async (reason: string) => {
        const id = trackingIdRef.current;
        const connection = connectionRef.current;

        logVideoProgress("SignalR: connectHub called", {
            reason,
            trackingId: id,
            visibility: document.visibilityState,
            hasConnection: Boolean(connection),
        });

        if (!id || !connection || document.visibilityState === "hidden") {
            warnVideoProgress("SignalR: connectHub skipped", {
                reason,
                hasTrackingId: Boolean(id),
                hasConnection: Boolean(connection),
                visibility: document.visibilityState,
            });
            return;
        }

        const connected = await startHubConnection(connection, id);
        setIsLive(connected);
        logVideoProgress("SignalR: connectHub result", { reason, connected });
    }, []);

    useEffect(() => {
        trackingIdRef.current = trackingId;
        videoSlugRef.current = videoSlug;
        logVideoProgress("Refs updated", { trackingId, videoSlug });
    }, [trackingId, videoSlug]);

    useEffect(() => {
        if (!trackingId) {
            logVideoProgress("Hook: no trackingId — reset state");
            setProgress(null);
            setIsLive(false);
            return;
        }

        logVideoProgress("Hook: init tracking", { trackingId, videoSlug });

        const connection = buildHubConnection();
        connectionRef.current = connection;

        connection.on("ReceiveProgress", (update: IVideoProcessingResponse) => {
            logVideoProgress("SignalR: ReceiveProgress event", update);
            applyProgress("signalr", normalizeProgress(update as unknown as Record<string, unknown>));
            setIsLive(true);
        });

        connection.onreconnecting((error) => {
            warnVideoProgress("SignalR: reconnecting", error);
            setIsLive(false);
        });

        connection.onreconnected(async (connectionId) => {
            logVideoProgress("SignalR: reconnected", { connectionId });
            setIsLive(true);
            await safeJoinChannel(connection, trackingId);
            await pollProgress("signalr-reconnected");
        });

        connection.onclose((error) => {
            warnVideoProgress("SignalR: closed", error);
            setIsLive(false);
        });

        void connectHub("init");
        void pollProgress("init");

        const pollTimer = window.setInterval(() => {
            if (document.visibilityState === "visible") {
                void pollProgress("interval");
            }
        }, POLL_INTERVAL_MS);

        const handleVisibilityChange = () => {
            logVideoProgress("Event: visibility/focus/online", {
                visibility: document.visibilityState,
            });

            if (document.visibilityState === "hidden") {
                void stopHubConnection(connection);
                setIsLive(false);
                return;
            }

            void connectHub("tab-visible");
            void pollProgress("tab-visible");
        };

        document.addEventListener("visibilitychange", handleVisibilityChange);
        window.addEventListener("online", handleVisibilityChange);
        window.addEventListener("focus", handleVisibilityChange);

        return () => {
            logVideoProgress("Hook: cleanup", { trackingId });
            window.clearInterval(pollTimer);
            document.removeEventListener("visibilitychange", handleVisibilityChange);
            window.removeEventListener("online", handleVisibilityChange);
            window.removeEventListener("focus", handleVisibilityChange);

            if (connectionRef.current) {
                connectionRef.current
                    .invoke("LeaveChannel", trackingId)
                    .catch((error) => errorVideoProgress("SignalR: LeaveChannel failed", error));
                connectionRef.current.stop().catch((error) => errorVideoProgress("SignalR: cleanup stop failed", error));
                connectionRef.current = null;
            }
        };
    }, [trackingId, applyProgress, pollProgress, connectHub, videoSlug]);

    return { progress, isLive };
}

interface VideoProcessingModalProps {
    trackingId: string | null;
    videoSlug: string;
}

export function VideoProcessingModal({ trackingId, videoSlug }: VideoProcessingModalProps) {
    const navigate = useNavigate();
    const { progress, isLive } = useVideoProgress(trackingId, videoSlug);
    const isCompleted = isCompletedStatus(progress?.status);

    useEffect(() => {
        logVideoProgress("Modal: render state", {
            trackingId,
            videoSlug,
            isLive,
            progress,
            isCompleted,
        });
    }, [trackingId, videoSlug, isLive, progress, isCompleted]);

    useEffect(() => {
        if (!isCompleted) {
            return;
        }

        logVideoProgress("Modal: redirect scheduled", { videoSlug });
        const timer = setTimeout(() => {
            logVideoProgress("Modal: redirecting", { videoSlug });
            navigate(`/video/${videoSlug}`);
        }, 2000);
        return () => clearTimeout(timer);
    }, [isCompleted, navigate, videoSlug]);

    return (
        <Modal
            open={!!trackingId}
            footer={null}
            closable={false}
            centered
            styles={{
                mask: { backdropFilter: "blur(8px)", backgroundColor: "rgba(0, 0, 0, 0.6)" },
            }}
            width="100%"
            style={{ maxWidth: 600, padding: "0 12px", margin: "0 auto" }}
            modalRender={() => (
                <div className="bg-theme-bg border border-zinc-800 rounded-2xl sm:rounded-[32px] p-5 sm:p-8 shadow-2xl overflow-hidden text-zinc-100 w-full max-w-[600px] mx-auto">
                    <div className="flex flex-col sm:flex-row sm:items-center justify-between mb-6 sm:mb-8 pb-4 border-b border-zinc-800 gap-3">
                        <h2 className="text-zinc-100 text-base sm:text-xl font-black uppercase tracking-tight leading-snug">
                            Завантаження та обробка відео
                        </h2>
                        <div className="flex items-center gap-2 px-3 py-1 bg-zinc-900 rounded-full border border-zinc-800 self-start sm:self-auto shrink-0">
                            <div className={`w-2 h-2 rounded-full ${isLive ? "bg-green-500 shadow-[0_0_10px_#22c55e]" : "bg-amber-500 animate-pulse"}`} />
                            <span className="text-[10px] font-bold text-zinc-400 uppercase tracking-widest">
                                {isLive ? "Live" : "Polling"}
                            </span>
                        </div>
                    </div>

                    {progress ? (
                        <div className="space-y-6 sm:space-y-8">
                            <div className="relative">
                                <Progress
                                    percent={Math.round(progress.percentage)}
                                    status={isCompleted ? "success" : "active"}
                                    strokeColor={isCompleted ? "#22c55e" : "#dc2626"}
                                    trailColor="rgb(var(--color-zinc-800))"
                                    format={(percent) => <span className="text-zinc-100 text-base sm:text-lg font-black tracking-tighter">{percent}%</span>}
                                    strokeWidth={14}
                                />
                            </div>

                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                                <div className="p-3.5 sm:p-4 bg-zinc-900/60 rounded-xl sm:rounded-2xl border border-zinc-800/80 backdrop-blur-sm">
                                    <p className="text-[10px] text-zinc-500 uppercase font-black tracking-widest mb-1">Статус процесу</p>
                                    <p className="text-xs sm:text-sm font-bold text-zinc-100 flex items-center gap-2">
                                        <span className="w-1.5 h-1.5 bg-red-600 rounded-full shrink-0" />
                                        <span className="truncate">{progress.status}</span>
                                    </p>
                                </div>
                                <div className="p-3.5 sm:p-4 bg-zinc-900/60 rounded-xl sm:rounded-2xl border border-zinc-800/80 backdrop-blur-sm">
                                    <p className="text-[10px] text-zinc-500 uppercase font-black tracking-widest mb-1">Залишилось часу</p>
                                    <p className="text-xs sm:text-sm font-bold text-zinc-100 italic truncate">
                                        {progress.estimatedTimeRemaining || "Calculating..."}
                                    </p>
                                </div>
                            </div>

                            {isCompleted && (
                                <div className="p-4 sm:p-5 bg-green-500/10 border border-green-500/30 rounded-xl sm:rounded-2xl text-green-600 dark:text-green-400 text-xs sm:text-sm text-center font-bold animate-in fade-in zoom-in duration-500">
                                    <div className="mb-1.5 flex justify-center">
                                        <div className="w-7 h-7 sm:w-8 sm:h-8 bg-green-500 rounded-full flex items-center justify-center text-white font-bold text-xs sm:text-sm shadow-md">✓</div>
                                    </div>
                                    Відео успішно завантажено та оброблено!
                                    <br />
                                    <span className="text-zinc-500 font-medium text-[11px] sm:text-xs">Перенаправлення на сторінку перегляду...</span>
                                </div>
                            )}
                        </div>
                    ) : (
                        <div className="flex flex-col items-center justify-center py-8 sm:py-12 space-y-4 sm:space-y-6">
                            <div className="relative">
                                <div className="w-12 h-12 sm:w-16 sm:h-16 border-4 border-zinc-800 rounded-full" />
                                <div className="absolute top-0 left-0 w-12 h-12 sm:w-16 sm:h-16 border-4 border-red-600 border-t-transparent rounded-full animate-spin" />
                            </div>
                            <div className="text-center px-2">
                                <p className="text-zinc-100 font-bold tracking-tight text-sm sm:text-base">Ініціалізація завантаження</p>
                                <p className="text-zinc-500 text-xs mt-1">Очікуємо відповідь від сервера...</p>
                            </div>
                        </div>
                    )}
                </div>
            )}
        />
    );
}
