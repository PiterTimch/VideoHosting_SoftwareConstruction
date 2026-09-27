import { useState, useRef, useEffect, type ChangeEvent, type FormEvent } from 'react';
import { ImagePlus, Upload, AlertCircle } from 'lucide-react';
import { useGetPrivaciesQuery } from '../../services/api/apiVideos';
import type { IVideoCreateRequest } from '../../types/Video/IVideoCreateRequest';

import { useFormServerErrors } from "../../hooks/useFormServerErrors";
import LoadingOverlay from "../ui/loading/LoadingOverlay";
import { MoviePlayer } from '../movie/MoviePlayer';
import { APP_ENV } from '../../env';
import { slugify } from '../../utils/slugify';

export interface VideoFormProps {
    title: string;
    submitButtonText: string;
    onSubmit: (form: IVideoCreateRequest) => Promise<void>;
    isLoading: boolean;
    initialData?: Partial<IVideoCreateRequest>;
    requireVideoFile?: boolean;
    initialImageUrl?: string;
    initialVideoUrl?: string;
}

export function VideoForm({
    title,
    submitButtonText,
    onSubmit,
    isLoading,
    initialData,
    requireVideoFile = true,
    initialImageUrl,
    initialVideoUrl,
}: VideoFormProps) {
    const { data: privaciesData } = useGetPrivaciesQuery();

    const imageInputRef = useRef<HTMLInputElement>(null);
    const videoInputRef = useRef<HTMLInputElement>(null);

    const {
        errors,
        setServerErrors,
        clearError,
    } = useFormServerErrors();

    const [form, setForm] = useState<IVideoCreateRequest>(() => ({
        title: initialData?.title ?? '',
        slug: initialData?.slug ?? '',
        description: initialData?.description ?? '',
        image: initialData?.image ?? undefined,
        video: initialData?.video ?? undefined,
        privacyId: initialData?.privacyId ?? 1,
    }));

    const [imagePreview, setImagePreview] = useState<string>('');
    const [videoPreview, setVideoPreview] = useState<string>('');

    useEffect(() => {
        if (form.image instanceof File) {
            const url = URL.createObjectURL(form.image);
            setImagePreview(url);
            return () => URL.revokeObjectURL(url);
        } else if (initialImageUrl) {
            setImagePreview(`${APP_ENV.IMAGES_400_URL}${initialImageUrl}`);
        } else {
            setImagePreview('');
        }
    }, [form.image, initialImageUrl]);

    useEffect(() => {
        if (form.video instanceof File) {
            const url = URL.createObjectURL(form.video);
            setVideoPreview(url);
            return () => URL.revokeObjectURL(url);
        } else {
            setVideoPreview('');
        }
    }, [form.video]);

    const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setForm(prev => {
            const nextState = { ...prev, [name]: value };
            if (name === 'title' && (requireVideoFile || !prev.slug)) {
                nextState.slug = slugify(value);
            }
            return nextState;
        });
        clearError(name);
        if (name === 'title') clearError('slug');
    };

    const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
        const { name, files } = e.target;
        if (files?.[0]) {
            setForm(prev => ({ ...prev, [name]: files[0] }));
            clearError(name);
        }
    };

    const validateClient = () => {
        const validationErrors: Record<string, string[]> = {};

        if (!form.title.trim()) validationErrors.title = ['Назва обовʼязкова'];
        if (!form.slug.trim()) validationErrors.slug = ['Slug обовʼязковий'];
        if (!form.description?.trim()) validationErrors.description = ['Опис не може бути порожнім'];

        if (!form.video && requireVideoFile && !initialVideoUrl) {
            validationErrors.video = ['Відеофайл обовʼязковий для завантаження'];
        }

        if (form.privacyId === 0) validationErrors.privacyId = ['Оберіть рівень приватності'];

        if (Object.keys(validationErrors).length) {
            setServerErrors(validationErrors);
            window.scrollTo({ top: 0, behavior: 'smooth' });
            return false;
        }

        return true;
    };

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (!validateClient()) return;

        try {
            await onSubmit(form);
        } catch (err: any) {
            if (err?.data?.errors) {
                setServerErrors(err.data.errors);
            }
        }
    };

    const inputBaseStyle = "w-full bg-white dark:bg-zinc-900/50 text-zinc-900 dark:text-zinc-100 placeholder-zinc-400 dark:placeholder-zinc-500 rounded-xl px-5 py-3.5 text-sm focus:outline-none focus:ring-2 focus:ring-[#ff2a6d]/80 focus:border-transparent transition-all duration-200 border border-zinc-300 dark:border-zinc-800 hover:border-zinc-400 dark:hover:border-zinc-700 shadow-xs";

    const cardContainerStyle = "bg-white dark:bg-zinc-900/60 border border-zinc-200 dark:border-zinc-800 rounded-2xl shadow-xs backdrop-blur-md overflow-hidden";

    return (
        <div className="relative min-h-screen bg-theme-bg text-theme-zinc-100 p-6 lg:p-10 flex flex-col justify-between transition-colors duration-300">
            {isLoading && <LoadingOverlay />}

            <input ref={imageInputRef} type="file" name="image" accept="image/*" onChange={handleFileChange} className="hidden" />
            <input ref={videoInputRef} type="file" name="video" accept="video/*" onChange={handleFileChange} className="hidden" />

            <form onSubmit={handleSubmit} className="flex-1 flex flex-col justify-between max-w-[1400px] w-full mx-auto">
                <div>
                    <div className="flex items-center justify-between pb-6 border-b border-zinc-200 dark:border-zinc-800">
                        <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-zinc-900 dark:text-zinc-100">{title}</h1>
                    </div>

                    <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start mt-6">
                        <div className="lg:col-span-7 space-y-6">
                            <div className="space-y-2">
                                <label className="text-xs font-semibold text-zinc-700 dark:text-zinc-300">
                                    Назва відео *
                                </label>
                                <input
                                    type="text"
                                    name="title"
                                    value={form.title}
                                    onChange={handleChange}
                                    placeholder="Введіть назву відео"
                                    className={`${inputBaseStyle} ${errors.title ? '!border-red-500' : ''}`}
                                />
                                {errors.title && <p className="text-red-500 text-xs mt-1">{errors.title[0]}</p>}
                            </div>

                            <div className="space-y-2">
                                <label className="text-xs font-semibold text-zinc-700 dark:text-zinc-300">Slug *</label>
                                <input
                                    type="text"
                                    name="slug"
                                    value={form.slug}
                                    onChange={handleChange}
                                    placeholder="video-slug"
                                    className={`${inputBaseStyle} ${errors.slug ? '!border-red-500' : ''}`}
                                />
                                {errors.slug && <p className="text-red-500 text-xs mt-1">{errors.slug[0]}</p>}
                            </div>

                            <div className="space-y-2">
                                <label className="text-xs font-semibold text-zinc-700 dark:text-zinc-300">Опис *</label>
                                <textarea
                                    name="description"
                                    rows={4}
                                    value={form.description}
                                    onChange={handleChange}
                                    placeholder="Розкажіть глядачам про ваше відео."
                                    className={`${inputBaseStyle} rounded-2xl p-5 resize-none ${errors.description ? '!border-red-500' : ''}`}
                                />
                                {errors.description && <p className="text-red-500 text-xs mt-1">{errors.description[0]}</p>}
                            </div>

                            <div className="space-y-2 pt-2">
                                <h3 className="text-xs font-semibold text-zinc-700 dark:text-zinc-300">Обкладинка відео</h3>
                                <div
                                    onClick={() => imageInputRef.current?.click()}
                                    className={`w-full max-w-sm aspect-video rounded-2xl border-2 border-dashed ${errors.image ? 'border-red-500' : 'border-zinc-300 dark:border-zinc-700 hover:border-[#ff2a6d] dark:hover:border-[#ff2a6d]'
                                    } bg-zinc-50 dark:bg-zinc-900/40 hover:bg-zinc-100 dark:hover:bg-zinc-800/60 flex flex-col items-center justify-center text-center cursor-pointer transition-all group relative overflow-hidden`}
                                >
                                    {imagePreview ? (
                                        <>
                                            <img src={imagePreview} alt="Cover Preview" className="w-full h-full object-cover" />
                                            <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex flex-col items-center justify-center gap-2 backdrop-blur-xs">
                                                <ImagePlus className="text-white" size={24} />
                                                <span className="text-xs bg-[#ff2a6d] text-white px-3 py-1 rounded-full font-medium">Змінити обкладинку</span>
                                            </div>
                                        </>
                                    ) : (
                                        <>
                                            <ImagePlus className="text-zinc-500 dark:text-zinc-400 mb-2 group-hover:text-[#ff2a6d] group-hover:scale-110 transition-all" size={28} />
                                            <p className="text-xs text-zinc-800 dark:text-zinc-200 font-semibold group-hover:text-zinc-900 dark:group-hover:text-zinc-100 transition-colors">
                                                Натисніть, щоб завантажити фото
                                            </p>
                                            <p className="text-[10px] text-zinc-500 dark:text-zinc-400 mt-1">Формат: 16:9 (PNG, JPG)</p>
                                        </>
                                    )}
                                </div>
                                {errors.image && <p className="text-red-500 text-xs mt-1">{errors.image[0]}</p>}
                            </div>
                        </div>

                        <div className="lg:col-span-5 space-y-6">
                            <div className={`${cardContainerStyle} p-4 space-y-4`}>
                                <div className="aspect-video w-full rounded-xl overflow-hidden bg-zinc-50 dark:bg-zinc-950 flex items-center justify-center relative border border-zinc-200 dark:border-zinc-800">
                                    {videoPreview ? (
                                        <MoviePlayer src={videoPreview} />
                                    ) : initialVideoUrl ? (
                                        <MoviePlayer videoName={initialVideoUrl} />
                                    ) : (
                                        <div
                                            onClick={() => videoInputRef.current?.click()}
                                            className="group flex flex-col items-center gap-3 text-zinc-600 dark:text-zinc-400 cursor-pointer hover:text-zinc-900 dark:hover:text-zinc-200 transition-colors w-full h-full justify-center p-4 text-center"
                                        >
                                            <div className="w-16 h-16 rounded-full bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 flex items-center justify-center shadow-md group-hover:scale-105 transition-transform">
                                                <Upload size={28} className="text-[#ff2a6d]" />
                                            </div>
                                            <div className="space-y-1">
                                                <p className="text-sm font-semibold text-zinc-800 dark:text-zinc-200">Відео не вибрано</p>
                                                <p className="text-xs text-zinc-500 dark:text-zinc-400">Натисніть, щоб обрати відеофайл</p>
                                            </div>
                                        </div>
                                    )}
                                </div>

                                <div className="px-1">
                                    <div className="flex items-center justify-between mt-2">
                                        <div className="overflow-hidden">
                                            <p className="text-[10px] text-zinc-500 dark:text-zinc-400 uppercase font-semibold">Назва файлу</p>
                                            <p className="text-sm font-semibold text-zinc-900 dark:text-zinc-100 truncate w-48">
                                                {form.video instanceof File ? form.video.name : (form.title || 'Немає відео')}
                                            </p>
                                        </div>
                                        <button
                                            type="button"
                                            onClick={() => videoInputRef.current?.click()}
                                            className="flex items-center gap-1.5 bg-[#ff2a6d] hover:bg-[#e0245e] text-white text-xs px-4 py-2.5 rounded-xl transition-colors font-medium shrink-0 cursor-pointer shadow-md shadow-[#ff2a6d]/20"
                                        >
                                            <Upload size={14} />
                                            {form.video || initialVideoUrl ? 'Змінити відео' : 'Завантажити'}
                                        </button>
                                    </div>
                                </div>

                                {errors.video && (
                                    <div className="mt-2 p-3 bg-red-500/10 border border-red-500/20 rounded-xl flex items-center gap-2">
                                        <AlertCircle className="text-red-500 shrink-0" size={16} />
                                        <p className="text-red-500 text-xs font-medium">{errors.video[0]}</p>
                                    </div>
                                )}
                            </div>

                            <div className={`${cardContainerStyle} p-6 space-y-4`}>
                                <div>
                                    <h3 className="text-sm font-semibold text-zinc-900 dark:text-zinc-100">Приватність *</h3>
                                    <p className="text-xs text-zinc-500 dark:text-zinc-400">Виберіть, хто зможе бачити це відео.</p>
                                </div>

                                <div className="space-y-3">
                                    {privaciesData?.map((privacy) => {
                                        const isSelected = form.privacyId === privacy.id;
                                        return (
                                            <label
                                                key={privacy.id}
                                                onClick={() => {
                                                    setForm(prev => ({ ...prev, privacyId: privacy.id }));
                                                    clearError('privacyId');
                                                }}
                                                className={`flex items-start gap-3 p-3.5 rounded-xl cursor-pointer border transition-all ${isSelected
                                                    ? 'bg-[#ff2a6d]/10 border-[#ff2a6d] shadow-md shadow-[#ff2a6d]/10'
                                                    : 'bg-zinc-50 dark:bg-zinc-900/40 border-zinc-300 dark:border-zinc-800 hover:border-zinc-400 dark:hover:border-zinc-700 hover:bg-zinc-100 dark:hover:bg-zinc-800/60'
                                                }`}
                                            >
                                                <div className="pt-0.5">
                                                    <div className={`w-4 h-4 rounded-full border flex items-center justify-center transition-colors ${isSelected ? 'border-[#ff2a6d]' : 'border-zinc-400 dark:border-zinc-600'}`}>
                                                        {isSelected && <div className="w-2 h-2 rounded-full bg-[#ff2a6d]" />}
                                                    </div>
                                                </div>
                                                <div>
                                                    <p className={`text-xs font-semibold ${isSelected ? 'text-[#ff2a6d]' : 'text-zinc-800 dark:text-zinc-200'}`}>
                                                        {privacy.name}
                                                    </p>
                                                </div>
                                            </label>
                                        );
                                    })}
                                </div>
                                {errors.privacyId && <p className="text-red-500 text-xs">{errors.privacyId[0]}</p>}
                            </div>
                        </div>
                    </div>
                </div>

                <div className="pt-6 mt-6 border-t border-zinc-200 dark:border-zinc-800 flex items-center justify-end">
                    <button
                        type="submit"
                        disabled={isLoading}
                        className="bg-[#ff2a6d] hover:bg-[#e0245e] text-white font-semibold px-8 py-3 rounded-full text-sm transition-all shadow-lg shadow-[#ff2a6d]/20 disabled:opacity-50 cursor-pointer flex items-center gap-2"
                    >
                        {submitButtonText}
                    </button>
                </div>
            </form>
        </div>
    );
}
