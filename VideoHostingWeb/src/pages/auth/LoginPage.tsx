import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Eye, EyeOff, Video } from 'lucide-react';
import { motion } from 'framer-motion';
import { useLoginMutation } from "../../services/api/apiAccount";
import LoadingOverlay from "../../components/ui/loading/LoadingOverlay";
import type { ILogin } from "../../types/Account/ILogin";

function LoginPage() {
    const navigate = useNavigate();
    const [errorMessage, setErrorMessage] = useState("");
    const [showPassword, setShowPassword] = useState(false);
    const [rememberMe, setRememberMe] = useState(false);

    const [formData, setFormData] = useState<ILogin>({
        email: '',
        password: '',
    });

    const [login, { isLoading: isLoginLoading }] = useLoginMutation();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMessage("");

        try {
            await login(formData).unwrap();
            navigate('/');
        } catch (err: any) {
            setErrorMessage(err?.data?.message || 'Помилка авторизації');
        }
    };

    return (
        <div className="min-h-screen bg-[rgb(var(--color-bg))] text-[rgb(var(--color-zinc-50))] flex items-center justify-center p-4 relative overflow-hidden transition-colors duration-300">
            {isLoginLoading && <LoadingOverlay />}

            <div className="absolute top-1/4 left-1/2 -translate-x-[110%] -translate-y-1/2 w-[30rem] h-[30rem] bg-[#ff2a6d]/25 rounded-full blur-[120px] pointer-events-none" />
            <div className="absolute bottom-1/4 left-1/2 translate-x-[10%] translate-y-1/2 w-[30rem] h-[30rem] bg-[#3b82f6]/25 rounded-full blur-[120px] pointer-events-none" />

            <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ duration: 0.3 }}
                className="w-full max-w-md bg-[rgb(var(--color-zinc-950)/0.8)] backdrop-blur-xl border border-[rgb(var(--color-zinc-700))] rounded-3xl p-8 shadow-2xl z-10 flex flex-col items-center transition-colors duration-300"
            >
                <div className="flex items-center gap-2 mb-6">
                    <Video className="text-[#ff2a6d] h-8 w-8" />
                    <span className="text-2xl font-bold tracking-tight text-[#ff2a6d]">VideoHosting</span>
                </div>

                <h1 className="text-2xl font-bold text-center mb-1 text-[rgb(var(--color-zinc-50))]">З поверненням</h1>
                <p className="text-[rgb(var(--color-zinc-400))] text-xs text-center mb-6">
                    Раді бачити вас знову! Будь ласка, введіть свої дані.
                </p>

                <form onSubmit={handleSubmit} className="w-full space-y-4">
                    <div className="flex flex-col gap-1.5">
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

                    <div className="flex flex-col gap-1.5">
                        <label className="text-xs font-semibold text-[rgb(var(--color-zinc-200))]">Пароль</label>
                        <div className="relative flex items-center">
                            <input
                                type={showPassword ? "text" : "password"}
                                required
                                placeholder="Введіть ваш пароль"
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
                        <p className="text-red-500 text-xs font-medium text-center">{errorMessage}</p>
                    )}

                    <div className="flex items-center justify-between text-xs pt-1">
                        <label className="flex items-center gap-2 cursor-pointer select-none">
                            <input
                                type="checkbox"
                                checked={rememberMe}
                                onChange={(e) => setRememberMe(e.target.checked)}
                                className="w-4 h-4 rounded-full accent-[#ff2a6d] cursor-pointer"
                            />
                            <span className="text-[rgb(var(--color-zinc-300))] font-medium">Запам'ятати мене</span>
                        </label>
                    </div>

                    <button
                        type="submit"
                        className="w-full bg-[rgb(var(--color-zinc-100))] hover:bg-[rgb(var(--color-zinc-50))] text-[rgb(var(--color-zinc-950))] font-semibold py-2.5 rounded-full text-sm transition-all shadow-md mt-2 cursor-pointer"
                    >
                        Увійти
                    </button>
                </form>

                <p className="text-xs text-[rgb(var(--color-zinc-400))] mt-6">
                    Немає акаунту?{' '}
                    <button
                        type="button"
                        onClick={() => navigate('/register')}
                        className="text-[#ff2a6d] hover:underline font-medium cursor-pointer"
                    >
                        Зареєструватися
                    </button>
                </p>
            </motion.div>
        </div>
    );
}

export default LoginPage;
