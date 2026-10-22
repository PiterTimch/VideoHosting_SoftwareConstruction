import { renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const { mutation } = vi.hoisted(() => ({ mutation: vi.fn() }));
vi.mock("../../src/services/api/apiVideos", () => ({
    useGenerateAccessTokenMutation: () => [mutation, { data: { token: "token-value", expiresAt: "2030-01-01T00:00:00Z" }, isLoading: false, error: undefined }],
}));

import { useVideoAccessToken } from "../../src/hooks/useVideoAccessToken";

describe("useVideoAccessToken", () => {
    beforeEach(() => mutation.mockReset());

    it("exposes token state and generates a token for the selected video", async () => {
        mutation.mockReturnValue({ unwrap: () => Promise.resolve({ token: "token-value" }) });
        const { result } = renderHook(() => useVideoAccessToken(42));
        expect(result.current.token).toBe("token-value");
        expect(result.current.isLoading).toBe(false);
        await result.current.generate();
        expect(mutation).toHaveBeenCalledWith(42);
    });
});
