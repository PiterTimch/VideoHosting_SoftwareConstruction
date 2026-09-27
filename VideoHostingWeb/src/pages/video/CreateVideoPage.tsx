import { useState } from 'react';
import { useCreateVideoMutation } from '../../services/api/apiVideos';
import type { IVideoCreateRequest } from '../../types/Video/IVideoCreateRequest';
import { VideoProcessingModal } from '../../components/modal/video/VideoProcessingModal';
import { VideoForm } from '../../components/video/VideoForm';

export default function CreateVideoPage() {
    const [trackingId, setTrackingId] = useState<string | null>(null);
    const [createdSlug, setCreatedSlug] = useState<string>('');
    const [createVideo, { isLoading }] = useCreateVideoMutation();

    const handleSubmit = async (form: IVideoCreateRequest) => {
        setCreatedSlug(form.slug);
        const result = await createVideo(form).unwrap();
        setTrackingId(result.trackingId);
    };

    return (
        <>
            <VideoProcessingModal trackingId={trackingId} videoSlug={createdSlug} />

            <VideoForm
                title="Створити відео"
                submitButtonText="Створити"
                onSubmit={handleSubmit}
                isLoading={isLoading}
                requireVideoFile={true}
            />
        </>
    );
}
