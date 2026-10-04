import { User, Mail, Shield, LogOut, Edit2 } from 'lucide-react';
import { motion } from 'framer-motion';
import { useAppSelector } from "../../store/index";
import { APP_ENV } from "../../env/index";
import { logout } from "../../store/slices/authSlice";
import { useLogoutMutation } from "../../services/api/apiAccount";
import { useDispatch } from "react-redux";
import { useNavigate } from "react-router-dom";
import { Button } from "../../components/form/Button";

function ProfilePage() {
    const { user } = useAppSelector(state => state.auth);
    const fullName = user?.name || "Користувач";
    const nameParts = fullName.split(' ');
    const lastName = nameParts[0] || "";
    const firstName = nameParts.slice(1).join(' ') || nameParts[0] || "";

    const dispatch = useDispatch();
    const navigate = useNavigate();
    const [logoutApi] = useLogoutMutation();

    const logoutHandler = async () => {
        try {
            await logoutApi().unwrap();
        } catch (e) {
            console.error("Logout failed on server", e);
        }
        dispatch(logout());
        navigate('/');
    }

    if (!user) return null;

    return (
        <div className="min-h-screen bg-theme-bg text-theme-text p-6 md:p-12">
            <div className="max-w-4xl mx-auto">

                <section
                    className="relative mb-12 flex flex-col md:flex-row md:flex-wrap items-center gap-6 bg-zinc-100 dark:bg-zinc-900/20 p-8 rounded-[3rem] border border-zinc-200 dark:border-white/5 backdrop-blur-sm">
                    <div className="relative group shrink-0">
                        <div
                            className="w-32 h-32 rounded-[2.5rem] overflow-hidden border-2 border-red-600 shadow-2xl shadow-red-600/20">
                            <img
                                src={user.image ? `${APP_ENV.IMAGES_400_URL}${user.image}` : '/images/user/default.png'}
                                alt={user.name} className="w-full h-full object-cover" />
                        </div>
                    </div>

                    <div className="text-center md:text-left space-y-2 flex-1 min-w-0">
                        <h1 className="text-4xl font-black uppercase italic tracking-tighter text-theme-text truncate">
                            {firstName} <span className="text-red-600">{lastName}</span>
                        </h1>
                        <p className="text-zinc-500 font-medium flex items-center justify-center md:justify-start gap-2 truncate">
                            <Mail size={16} className="shrink-0" /> {user.email}
                        </p>
                        <div className="flex gap-2 pt-2 justify-center md:justify-start">
                            {user.roles.includes("Admin") ? (
                                <span
                                    className="px-3 py-1 bg-red-600/10 border border-red-600/20 rounded-full text-[10px] font-black uppercase text-red-500 tracking-widest">
                                    Адміністратор
                                </span>
                            ) : (
                                <span
                                    className="px-3 py-1 bg-red-600/10 border border-red-600/20 rounded-full text-[10px] font-black uppercase text-red-500 tracking-widest">
                                    Фрілансер / Креатор
                                </span>
                            )}
                        </div>
                    </div>

                    <Button
                        variant="inverse"
                        size="sm"
                        className="shrink-0 md:ml-auto gap-2"
                        icon={<Edit2 size={16} />}
                        onClick={() => navigate('/edit-account')}
                    >
                        Редагувати
                    </Button>
                </section>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

                    <motion.div whileHover={{ y: -5 }}
                        className="bg-zinc-100 dark:bg-zinc-900/40 p-8 rounded-[2.5rem] border border-zinc-200 dark:border-white/5 space-y-6">
                        <div className="flex items-center gap-3 mb-2">
                            <div className="p-2 bg-red-600/20 rounded-lg text-red-500">
                                <User size={20} />
                            </div>
                            <h2 className="text-lg font-black uppercase italic tracking-tight text-theme-text">Дані профілю</h2>
                        </div>

                        <div className="space-y-4">
                            <InfoRow label="Ім'я" value={firstName} />
                            <InfoRow label="Прізвище" value={lastName} />
                            <InfoRow label="Логін (Email)" value={user.email} />
                        </div>
                    </motion.div>

                    <div className="space-y-6">
                        <motion.div whileHover={{ y: -5 }}
                            className="bg-zinc-100 dark:bg-zinc-900/40 p-8 rounded-[2.5rem] border border-zinc-200 dark:border-white/5">
                            <div className="flex items-center gap-3 mb-4">
                                <div className="p-2 bg-red-600/20 rounded-lg text-red-500">
                                    <Shield size={20} />
                                </div>
                                <h2 className="text-lg font-black uppercase italic tracking-tight text-theme-text">Статус аккаунту</h2>
                            </div>
                            <p className="text-xs text-zinc-400">Аккаунт підтверджено та авторизовано у системі.</p>
                        </motion.div>

                        <Button
                            variant="danger"
                            fullWidth
                            className="p-6 rounded-[2rem] tracking-widest gap-3 group"
                            icon={<LogOut size={20} className="group-hover:-translate-x-1 transition-transform" />}
                            onClick={() => logoutHandler()}
                        >
                            Вийти з системи
                        </Button>
                    </div>

                </div>
            </div>
        </div>
    );
}

function InfoRow({ label, value }: { label: string, value: string }) {
    return (
        <div className="border-b border-zinc-200 dark:border-white/5 pb-3">
            <p className="text-[10px] font-black uppercase text-zinc-400 dark:text-zinc-600 tracking-widest mb-1">{label}</p>
            <p className="text-theme-text font-bold">{value}</p>
        </div>
    );
}

export default ProfilePage;
