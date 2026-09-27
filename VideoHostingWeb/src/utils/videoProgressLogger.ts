const PREFIX = "[VideoProgress]";
const DEBUG = import.meta.env.DEV && import.meta.env.VITE_VIDEO_PROGRESS_DEBUG === "true";

export function logVideoProgress(step: string, data?: unknown) {
    if (!DEBUG) return;

    if (data === undefined) {
        console.log(`${PREFIX} ${step}`);
        return;
    }

    console.log(`${PREFIX} ${step}`, data);
}

export function warnVideoProgress(step: string, data?: unknown) {
    if (data === undefined) {
        console.warn(`${PREFIX} ${step}`);
        return;
    }

    console.warn(`${PREFIX} ${step}`, data);
}

export function errorVideoProgress(step: string, data?: unknown) {
    if (data === undefined) {
        console.error(`${PREFIX} ${step}`);
        return;
    }

    console.error(`${PREFIX} ${step}`, data);
}
