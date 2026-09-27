import { Routes, Route } from 'react-router-dom';
import { useEffect } from 'react';

import UserHomePage from './pages/home/UserHomePage';
import VideoPage from './pages/video/VideoPage';
import SearchPage from './pages/video/SearchPage';
import CreateVideoPage from './pages/video/CreateVideoPage';
import EditVideoPage from './pages/video/EditVideoPage';
import NotFoundPage from './pages/error/NotFoundPage';

import AppLayout from './layouts/AppLayout';
import { initThemeSystem } from './themes';
import ScrollToTop from './components/layout/ScrollToTop';

function App() {
    useEffect(() => {
        const cleanup = initThemeSystem();
        return cleanup;
    }, []);

    return (
        <>
            <ScrollToTop />
            <Routes>
                <Route element={<AppLayout />}>
                    <Route path="/" element={<UserHomePage />} />
                    <Route path="/video/add" element={<CreateVideoPage />} />
                    <Route path="/video/edit/:id" element={<EditVideoPage />} />
                    <Route path="/video/:slug" element={<VideoPage />} />
                    <Route path="/search" element={<SearchPage />} />
                </Route>

                <Route path="*" element={<NotFoundPage />} />
            </Routes>
        </>
    );
}

export default App;
