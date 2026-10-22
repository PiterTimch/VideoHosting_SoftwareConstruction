import { useGenerateAccessTokenMutation } from "../services/api/apiVideos";

export function useVideoAccessToken(videoId: number) {
    const [generateToken, { data, isLoading, error }] = useGenerateAccessTokenMutation();
    const generate = () => generateToken(videoId).unwrap();
    return { token: data?.token, expiresAt: data?.expiresAt, generate, isLoading, error };
}
