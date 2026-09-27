import React, { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import PageTransition from '../components/layout/PageTransition';

interface BaseLayoutProps {
    sidebar: React.ReactNode | ((props: { isOpen: boolean; toggleSidebar: () => void }) => React.ReactNode);
    header?: React.ReactNode;
    footer?: React.ReactNode;
    className?: string;
    mainClassName?: string;
    contentClassName?: string;
    usePageTransition?: boolean;
}

export function BaseLayout({
    sidebar,
    header,
    footer,
    className = "flex flex-col md:flex-row min-h-screen bg-theme-bg text-zinc-100 antialiased font-sans",
    mainClassName = "flex-1 flex flex-col min-w-0 bg-theme-bg pb-16 md:pb-0",
    contentClassName = "px-2 py-1 flex-grow",
    usePageTransition = true,
}: BaseLayoutProps) {
    const [isOpen, setIsOpen] = useState(true);
    const toggleSidebar = () => setIsOpen(!isOpen);
    const location = useLocation();

    const renderedSidebar = typeof sidebar === 'function'
        ? sidebar({ isOpen, toggleSidebar })
        : sidebar;

    return (
        <div className={className}>
            {renderedSidebar}

            <main className={mainClassName}>
                {header}

                <div className={contentClassName}>
                    {usePageTransition ? (
                        <PageTransition key={location.pathname}>
                            <Outlet />
                        </PageTransition>
                    ) : (
                        <Outlet />
                    )}
                </div>

                {footer}
            </main>
        </div>
    );
}

export default BaseLayout;
