import { APP_ENV } from "../env";

export function buildApiUrl(relativePath: string): string {
    const base = APP_ENV.API_BASE_URL.replace(/\/$/, "");
    const path = relativePath.replace(/^\//, "");

    if (base.endsWith("/api")) {
        return `${base}/${path}`;
    }

    return `${base}/api/${path}`;
}
