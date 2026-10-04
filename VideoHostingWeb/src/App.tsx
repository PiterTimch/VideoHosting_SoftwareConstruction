import { Routes, Route } from 'react-router-dom';
import { useEffect, useState } from 'react';

import UserHomePage from './pages/home/UserHomePage';
import VideoPage from './pages/video/VideoPage';
import SearchPage from './pages/video/SearchPage';
import CreateVideoPage from './pages/video/CreateVideoPage';
import EditVideoPage from './pages/video/EditVideoPage';
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import ProfilePage from './pages/profile/ProfilePage';
import EditProfilePage from './pages/profile/EditProfilePage';
import NotFoundPage from './pages/error/NotFoundPage';

import AppLayout from './layouts/AppLayout';
import RequireLogin from './components/auth/RequireLogin';
import RequireAuthor from './components/auth/RequireAuthor';
import { initThemeSystem } from './themes';
import ScrollToTop from './components/layout/ScrollToTop';
import { useRefreshTokenMutation } from './services/api/apiAccount';

function App() {
    const [refreshToken] = useRefreshTokenMutation();
    const [isCheckingAuth, setIsCheckingAuth] = useState(true);

    useEffect(() => {
        const cleanup = initThemeSystem();
        return cleanup;
    }, []);

    useEffect(() => {
        const checkAuth = async () => {
            try {
                await refreshToken().unwrap();
            } catch (e) {
            } finally {
                setIsCheckingAuth(false);
            }
        };
        checkAuth();
    }, [refreshToken]);

    if (isCheckingAuth) {
        return (
            <div className="flex min-h-screen items-center justify-center bg-theme-bg text-zinc-100 font-sans">
                <div className="relative">
                    <div className="w-12 h-12 border-4 border-zinc-800 rounded-full" />
                    <div className="absolute top-0 left-0 w-12 h-12 border-4 border-red-600 border-t-transparent rounded-full animate-spin" />
                </div>
            </div>
        );
    }

    return (
        <>
            <ScrollToTop />
            <Routes>
                <Route element={<AppLayout />}>
                    <Route path="/" element={<UserHomePage />} />
                    <Route path="/video/:slug" element={<VideoPage />} />
                    <Route path="/search" element={<SearchPage />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />

                    <Route element={<RequireLogin />}>
                        <Route path="/account" element={<ProfilePage />} />
                        <Route path="/edit-account" element={<EditProfilePage />} />
                    </Route>

                    <Route element={<RequireAuthor />}>
                        <Route path="/video/add" element={<CreateVideoPage />} />
                        <Route path="/video/edit/:id" element={<EditVideoPage />} />
                    </Route>
                </Route>

                <Route path="*" element={<NotFoundPage />} />
            </Routes>
        </>
    );
}

export default App;
