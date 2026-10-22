import { describe, expect, it, vi, afterEach } from "vitest";
import { getVideoTokenExpiry, isVideoTokenValid } from "../../src/utils/videoTokenUtils";

describe("video token utilities", () => {
    afterEach(() => vi.restoreAllMocks());

    it("reads expiry and accepts a token that has not expired", () => {
        const token = btoa(`12:${Math.floor(Date.now() / 1000) + 60}:signature`);
        expect(getVideoTokenExpiry(token)).toBeGreaterThan(Date.now());
        expect(isVideoTokenValid(token)).toBe(true);
    });

    it("rejects malformed, missing, and expired tokens", () => {
        expect(getVideoTokenExpiry("bad-token")).toBeNull();
        expect(isVideoTokenValid(undefined)).toBe(false);
        expect(isVideoTokenValid(btoa("12:1:signature"))).toBe(false);
    });
});
