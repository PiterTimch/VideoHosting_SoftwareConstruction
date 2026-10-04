import React, { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Camera, Eye, EyeOff, ArrowLeft, Video } from 'lucide-react';
import { motion } from 'framer-motion';
import { useDispatch } from 'react-redux';
import { loginSuccess } from "../../store/slices/authSlice";
import { useRegisterMutation } from "../../services/api/apiAccount";
import LoadingOverlay from "../../components/ui/loading/LoadingOverlay";
import type { IRegister } from "../../types/Account/IRegister";

function RegisterPage() {
    const navigate = useNavigate();
    const dispatch = useDispatch();
    const fileInputRef = useRef<HTMLInputElement>(null);

    const [register, { isLoading }] = useRegisterMutation();

    const [formData, setFormData] = useState({
        firstName: '',
        lastName: '',
        email: '',
        password: '',
    });

    const [showPassword, setShowPassword] = useState(false);
    const [errorMessage, setErrorMessage] = useState("");
    const [imageFile, setImageFile] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string | null>(null);

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setImageFile(file);
        setImagePreview(URL.createObjectURL(file));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const reg: IRegister = {
            firstName: formData.firstName,
            lastName: formData.lastName,
            email: formData.email,
            password: formData.password,
        };

        if (imageFile) {
            reg.imageFile = imageFile;
        }

        try {
            const result = await register(reg).unwrap();
            dispatch(loginSuccess(result.token));
            navigate('/');
            setErrorMessage("");
        } catch (error: any) {
            setErrorMessage(error?.data?.message || 'Помилка реєстрації');
        }
    };

    return (
        <div className="min-h-screen bg-[rgb(var(--color-bg))] text-[rgb(var(--color-zinc-50))] flex items-center justify-center p-4 relative overflow-hidden py-10 transition-colors duration-300">
            {isLoading && <LoadingOverlay />}

            <div className="absolute top-1/4 left-1/2 -translate-x-[110%] -translate-y-1/2 w-[30rem] h-[30rem] bg-[#ff2a6d]/25 rounded-full blur-[120px] pointer-events-none" />
            <div className="absolute bottom-1/4 left-1/2 translate-x-[10%] translate-y-1/2 w-[30rem] h-[30rem] bg-[#3b82f6]/25 rounded-full blur-[120px] pointer-events-none" />

            <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ duration: 0.3 }}
                className="w-full max-w-md bg-[rgb(var(--color-zinc-950)/0.8)] backdrop-blur-xl border border-[rgb(var(--color-zinc-700))] rounded-3xl p-8 shadow-2xl z-10 flex flex-col items-center relative transition-colors duration-300"
            >
                <button
                    type="button"
                    onClick={() => navigate('/login')}
                    className="absolute top-6 left-6 text-[rgb(var(--color-zinc-400))] hover:text-[rgb(var(--color-zinc-50))] flex items-center gap-1.5 text-xs transition-colors cursor-pointer"
                >
                    <ArrowLeft size={16} />
                    <span>Вхід</span>
                </button>

                <div className="flex items-center gap-2 mb-4 mt-2">
                    <Video className="text-[#ff2a6d] h-8 w-8" />
                    <span className="text-2xl font-bold tracking-tight text-[#ff2a6d]">VideoHosting</span>
                </div>

                <h1 className="text-2xl font-bold text-center mb-1 text-[rgb(var(--color-zinc-50))]">Створення акаунта</h1>
                <p className="text-[rgb(var(--color-zinc-400))] text-xs text-center mb-6">
                    Приєднуйтесь до платформи вже сьогодні!
                </p>

                <form onSubmit={handleSubmit} className="w-full space-y-4">
                    <div className="flex flex-col items-center mb-2">
                        <div
                            onClick={() => fileInputRef.current?.click()}
                            className="relative w-20 h-20 rounded-full bg-[rgb(var(--color-zinc-800))] border-2 border-dashed border-[rgb(var(--color-zinc-600))] flex items-center justify-center cursor-pointer overflow-hidden group hover:border-[#ff2a6d] transition-all"
                        >
                            {imagePreview ? (
                                <img src={imagePreview} alt="Preview" className="w-full h-full object-cover" />
                            ) : (
                                <Camera size={24} className="text-[rgb(var(--color-zinc-400))] group-hover:text-[#ff2a6d] transition-colors" />
                            )}
                        </div>
                        <input
                            type="file"
                            ref={fileInputRef}
                            onChange={handleImageChange}
                            className="hidden"
                            accept="image/*"
                        />
                        <span className="text-[11px] text-[rgb(var(--color-zinc-400))] mt-1.5 font-medium">
                            Фото профілю
                        </span>
                    </div>

                    <div className="grid grid-cols-2 gap-3">
                        <div className="flex flex-col gap-1">
                            <label className="text-xs font-semibold text-[rgb(var(--color-zinc-200))]">Ім'я</label>
                            <input
                                type="text"
                                required
                                placeholder="Іван"
                                value={formData.firstName}
                                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                                className="w-full bg-[rgb(var(--color-zinc-800))] text-[rgb(var(--color-zinc-50))] placeholder-[rgb(var(--color-zinc-400))] px-3.5 py-2.5 rounded-full text-sm outline-none border border-[rgb(var(--color-zinc-700))] focus:border-[#ff2a6d] focus:ring-2 focus:ring-[#ff2a6d] transition-all"
                            />
                        </div>
                        <div className="flex flex-col gap-1">
                            <label className="text-xs font-semibold text-[rgb(var(--color-zinc-200))]">Прізвище</label>
                            <input
                                type="text"
                                required
                                placeholder="Іваненко"
                                value={formData.lastName}
                                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                                className="w-full bg-[rgb(var(--color-zinc-800))] text-[rgb(var(--color-zinc-50))] placeholder-[rgb(var(--color-zinc-400))] px-3.5 py-2.5 rounded-full text-sm outline-none border border-[rgb(var(--color-zinc-700))] focus:border-[#ff2a6d] focus:ring-2 focus:ring-[#ff2a6d] transition-all"
                            />
                        </div>
                    </div>

                    <div className="flex flex-col gap-1">
                        <label className="text-xs font-semibold text-[rgb(var(--color-zinc-200))]">Електронна пошта</label>
                        <input
                            type="email"
                            required
                            placeholder="Введіть ваш email"
                            value={formData.email}
                            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                            className="w-full bg-[rgb(var(--color-zinc-800))] text-[rgb(var(--color-zinc-50))] placeholder-[rgb(var(--color-zinc-400))] px-4 py-2.5 rounded-full text-sm outline-none border border-[rgb(var(--color-zinc-700))] focus:border-[#ff2a6d] focus:ring-2 focus:ring-[#ff2a6d] transition-all"
                        />
                    </div>

                    <div className="flex flex-col gap-1">
                        <label className="text-xs font-semibold text-[rgb(var(--color-zinc-200))]">Пароль</label>
                        <div className="relative flex items-center">
                            <input
                                type={showPassword ? "text" : "password"}
                                required
                                placeholder="Створіть пароль"
                                value={formData.password}
                                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                                className="w-full bg-[rgb(var(--color-zinc-800))] text-[rgb(var(--color-zinc-50))] placeholder-[rgb(var(--color-zinc-400))] pl-4 pr-11 py-2.5 rounded-full text-sm outline-none border border-[rgb(var(--color-zinc-700))] focus:border-[#ff2a6d] focus:ring-2 focus:ring-[#ff2a6d] transition-all"
                            />
                            <button
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                className="absolute right-4 text-[rgb(var(--color-zinc-400))] hover:text-[rgb(var(--color-zinc-50))] transition-colors"
                            >
                                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                            </button>
                        </div>
                    </div>

                    {errorMessage && (
                        <p className="text-red-500 text-xs font-medium text-center pt-1">{errorMessage}</p>
                    )}

                    <button
                        type="submit"
                        className="w-full bg-[rgb(var(--color-zinc-100))] hover:bg-[rgb(var(--color-zinc-50))] text-[rgb(var(--color-zinc-950))] font-semibold py-2.5 rounded-full text-sm transition-all shadow-md mt-4 cursor-pointer"
                    >
                        Зареєструватися
                    </button>
                </form>

                <p className="text-xs text-[rgb(var(--color-zinc-400))] mt-6">
                    Вже є акаунт?{' '}
                    <button
                        type="button"
                        onClick={() => navigate('/login')}
                        className="text-[#ff2a6d] hover:underline font-medium cursor-pointer"
                    >
                        Увійти
                    </button>
                </p>
            </motion.div>
        </div>
    );
}

export default RegisterPage;
