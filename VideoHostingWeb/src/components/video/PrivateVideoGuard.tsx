import type { ReactNode } from "react";
import { isVideoTokenValid } from "../../utils/videoTokenUtils";

export function PrivateVideoGuard({ token, children }: { token?: string | null; children: ReactNode }) {
    if (!isVideoTokenValid(token)) {
        return <div role="alert" className="flex aspect-video items-center justify-center bg-zinc-950 text-zinc-100">Access Denied</div>;
    }
    return <>{children}</>;
}
