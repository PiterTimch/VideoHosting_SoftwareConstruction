import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useGetByQuery, useEditVideoMutation } from '../../services/api/apiVideos';
import type { IVideoCreateRequest } from '../../types/Video/IVideoCreateRequest';
import { VideoProcessingModal } from '../../components/modal/video/VideoProcessingModal';
import { VideoForm } from '../../components/video/VideoForm';
import LoadingOverlay from '../../components/ui/loading/LoadingOverlay';

export default function EditVideoPage() {
    const { id } = useParams<{ id: string }>();
    const videoId = Number(id);
    const navigate = useNavigate();

    const [trackingId, setTrackingId] = useState<string | null>(null);
    const [createdSlug, setCreatedSlug] = useState<string>('');

    const { data: video, isLoading: isFetching } = useGetByQuery(
        { id: videoId },
        { skip: !videoId }
    );

    const [editVideo, { isLoading: isUpdating }] = useEditVideoMutation();

    useEffect(() => {
        if (!videoId || isNaN(videoId)) {
            navigate('/');
        }
    }, [videoId, navigate]);

    if (isFetching) return <LoadingOverlay />;
    if (!video) return null;

    const handleSubmit = async (form: IVideoCreateRequest) => {
        setCreatedSlug(form.slug);
        const result = await editVideo({
            id: video.id,
            title: form.title,
            slug: form.slug,
            description: form.description,
            image: form.image,
            video: form.video,
            privacyId: form.privacyId,
        }).unwrap();

        if (result?.trackingId) {
            setTrackingId(result.trackingId);
        } else {
            navigate(`/video/${form.slug}`);
        }
    };

    return (
        <>
            <VideoProcessingModal trackingId={trackingId} videoSlug={createdSlug} />

            <VideoForm
                title="Редагувати відео"
                submitButtonText="Зберегти зміни"
                onSubmit={handleSubmit}
                isLoading={isUpdating}
                initialData={{
                    title: video.title,
                    slug: video.slug,
                    description: video.description || '',
                    privacyId: video.privacy?.id || 1,
                }}
                requireVideoFile={false}
                initialImageUrl={video.image}
                initialVideoUrl={video.video}
            />
        </>
    );
}
