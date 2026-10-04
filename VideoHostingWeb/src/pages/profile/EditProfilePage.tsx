import { User, Mail, Camera, ArrowRight } from 'lucide-react';
import { motion } from 'framer-motion';
import LoadingOverlay from "../../components/ui/loading/LoadingOverlay";
import { useAppDispatch, useAppSelector } from "../../store/index";
import React, { useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { APP_ENV } from "../../env/index";
import { useEditAccountMutation } from "../../services/api/apiAccount";
import { loginSuccess } from "../../store/slices/authSlice";
import { InputField } from "../../components/form/InputField";
import { Button } from "../../components/form/Button";
import { BackButton } from "../../components/ui/common/BackButton";
import type { IUserEditRequest } from "../../types/User/IUserEditRequest";

function EditProfilePage() {
    const { user } = useAppSelector(state => state.auth);
    const [edit, { isLoading }] = useEditAccountMutation();
    const fileInputRef = useRef<HTMLInputElement>(null);

    const navigate = useNavigate();
    const dispatch = useAppDispatch();

    const fullName = user?.name || "";
    const nameParts = fullName.split(" ");

    const [formData, setFormData] = useState({
        firstName: nameParts[1] || nameParts[0] || "",
        lastName: nameParts[0] || "",
        email: user?.email || "",
    });

    const [imageFile, setImageFile] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string | null>(
        user?.image ? APP_ENV.IMAGES_400_URL + user.image : null
    );

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setImageFile(file);
        setImagePreview(URL.createObjectURL(file));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const value: IUserEditRequest = {
            id: user?.id,
            firstName: formData.firstName,
            lastName: formData.lastName,
            email: formData.email
        };
        if (imageFile) {
            value.image = imageFile;
        }
        try {
            const res = await edit(value).unwrap();
            dispatch(loginSuccess(res.token));
            navigate('/account');
        } catch (error) {
            console.error(error);
        }
    };

    return (
        <div
            className="min-h-screen bg-theme-bg text-theme-text flex items-center justify-center relative overflow-hidden py-12">
            {isLoading && <LoadingOverlay />}

            <div className="absolute top-[-10%] right-[-10%] w-[40%] h-[40%] bg-red-600/20 blur-[120px] rounded-full" />

            <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="w-full max-w-xl z-10 px-6"
            >
                <BackButton
                    label="Назад до акаунту"
                    onClick={() => navigate('/account')}
                />

                <div className="mb-10 text-center">
                    <h1 className="text-4xl font-black uppercase italic text-theme-text">
                        Редагувати <span className="text-red-600">профіль</span>
                    </h1>
                </div>

                <form
                    onSubmit={handleSubmit}
                    className="space-y-6 bg-zinc-100 dark:bg-zinc-900/30 p-8 rounded-[2.5rem] border border-zinc-200 dark:border-white/5 backdrop-blur-md shadow-2xl"
                >
                    <div className="flex flex-col items-center">
                        <div
                            onClick={() => fileInputRef.current?.click()}
                            className="relative w-24 h-24 rounded-3xl bg-zinc-200 dark:bg-zinc-800 border-2 border-dashed border-zinc-300 dark:border-zinc-700 flex items-center justify-center cursor-pointer overflow-hidden group hover:border-red-600"
                        >
                            {imagePreview ? (
                                <img src={imagePreview} className="w-full h-full object-cover" alt="Preview" />
                            ) : (
                                <Camera size={32} className="text-zinc-500 group-hover:text-red-600" />
                            )}
                        </div>
                        <input
                            type="file"
                            ref={fileInputRef}
                            onChange={handleImageChange}
                            className="hidden"
                            accept="image/*"
                        />
                        <span className="text-[10px] text-zinc-500 mt-3 font-bold uppercase">
                            Фото профілю
                        </span>
                    </div>

                    <InputField
                        label="Ім'я"
                        required
                        value={formData.firstName}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                            setFormData({ ...formData, firstName: e.target.value })
                        }
                        icon={<User className="text-zinc-400 dark:text-zinc-600" size={20} />}
                        inputClassName="w-full bg-zinc-800 rounded-2xl py-3.5"
                        labelClassName="text-[10px] uppercase text-zinc-500 ml-1"
                    />

                    <InputField
                        label="Прізвище"
                        required
                        value={formData.lastName}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                            setFormData({ ...formData, lastName: e.target.value })
                        }
                        icon={<User className="text-zinc-400 dark:text-zinc-600" size={20} />}
                        inputClassName="w-full bg-zinc-800 rounded-2xl py-3.5"
                        labelClassName="text-[10px] uppercase text-zinc-500 ml-1"
                    />

                    <InputField
                        label="Email"
                        type="email"
                        required
                        value={formData.email}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                            setFormData({ ...formData, email: e.target.value })
                        }
                        icon={<Mail className="text-zinc-400 dark:text-zinc-600" size={20} />}
                        inputClassName="w-full bg-zinc-800 rounded-2xl py-3.5"
                        labelClassName="text-[10px] uppercase text-zinc-500 ml-1"
                    />

                    <Button
                        type="submit"
                        variant="primary"
                        size="xl"
                        fullWidth
                        iconRight={<ArrowRight />}
                    >
                        Підтвердити
                    </Button>
                </form>
            </motion.div>
        </div>
    );
}

export default EditProfilePage;
