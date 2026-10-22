import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PrivateVideoGuard } from "../../src/components/video/PrivateVideoGuard";

describe("PrivateVideoGuard", () => {
    it("blocks when token is missing or expired", () => {
        render(<PrivateVideoGuard><div data-testid="player" /></PrivateVideoGuard>);
        expect(screen.getByRole("alert")).toHaveTextContent("Access Denied");
        expect(screen.queryByTestId("player")).not.toBeInTheDocument();
    });

    it("renders player when token has not expired", () => {
        const token = btoa(`12:${Math.floor(Date.now() / 1000) + 60}:signature`);
        render(<PrivateVideoGuard token={token}><div data-testid="player" /></PrivateVideoGuard>);
        expect(screen.getByTestId("player")).toBeInTheDocument();
    });
});
