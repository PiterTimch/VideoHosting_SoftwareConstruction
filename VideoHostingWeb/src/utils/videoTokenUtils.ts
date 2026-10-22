export function getVideoTokenExpiry(token: string): number | null {
    try {
        const normalized = token.replace(/-/g, "+").replace(/_/g, "/");
        const raw = atob(normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "="));
        const parts = raw.split(":");
        if (parts.length !== 3 || !/^\d+$/.test(parts[0]) || !/^\d+$/.test(parts[1])) return null;
        return Number(parts[1]) * 1000;
    } catch {
        return null;
    }
}

export function isVideoTokenValid(token: string | null | undefined): boolean {
    if (!token) return false;
    const expiresAt = getVideoTokenExpiry(token);
    return expiresAt !== null && expiresAt > Date.now();
}
