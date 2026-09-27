import React, { useState } from 'react';
import { Link, NavLink, useLocation } from 'react-router-dom';
import { Menu, ChevronDown, ChevronUp, Video } from 'lucide-react';
import { Button } from '../form/Button';

export interface SidebarItem {
    name: string;
    path?: string;
    icon: React.ReactNode;
    onClick?: () => void;
    end?: boolean;
    isActive?: boolean | ((pathname: string) => boolean);
    subItems?: { name: string; path: string }[];
}

export interface SidebarSection {
    title?: string;
    items: SidebarItem[];
}

interface BaseSidebarProps {
    isOpen?: boolean;
    toggleSidebar?: () => void;
    isCollapsible?: boolean;
    sections: SidebarSection[];
    bottomItems?: SidebarItem[];
    headerContent?: React.ReactNode;
}

function SidebarItemRow({
    item,
    isOpen,
    isCollapsible,
    getNavLinkClasses,
    renderActiveIndicator,
    checkActive,
}: {
    item: SidebarItem;
    isOpen: boolean;
    isCollapsible: boolean;
    getNavLinkClasses: (isActive: boolean) => string;
    renderActiveIndicator: (isActive: boolean) => React.ReactNode;
    checkActive: (item: SidebarItem) => boolean | undefined;
}) {
    const location = useLocation();
    const hasSubItems = !!(item.subItems && item.subItems.length > 0);

    const hasActiveSubItem = hasSubItems && item.subItems?.some(sub => {
        const searchParams = new URLSearchParams(location.search);
        const subParams = new URLSearchParams(sub.path.includes('?') ? sub.path.split('?')[1] : '');
        const pathMatches = location.pathname === (sub.path.includes('?') ? sub.path.split('?')[0] : sub.path);
        if (!pathMatches) return false;

        let queryMatches = true;
        subParams.forEach((val, key) => {
            if (searchParams.get(key) !== val) {
                queryMatches = false;
            }
        });
        return queryMatches;
    });

    const isItemActive = (checkActive(item) || false) || !!hasActiveSubItem;

    const [isExpanded, setIsExpanded] = useState(() => hasSubItems && isItemActive);

    if (hasSubItems) {
        return (
            <div className="space-y-1">
                <div
                    className={`${getNavLinkClasses(isItemActive)} w-full justify-between cursor-pointer select-none ${(!isCollapsible || isOpen) ? 'pr-4' : ''}`}
                    onClick={() => setIsExpanded(!isExpanded)}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                            setIsExpanded(!isExpanded);
                        }
                    }}
                >
                    {renderActiveIndicator(isItemActive)}

                    <div className={`flex items-center w-full ${(!isCollapsible || isOpen) ? '' : 'justify-center'}`}>
                        <span className={`min-w-[24px] flex items-center justify-center ${isItemActive ? 'text-rose-500' : 'text-zinc-400'}`}>
                            {item.icon}
                        </span>
                        {(!isCollapsible || isOpen) && (
                            <span className="ml-4 font-medium text-left flex-1">
                                {item.name}
                            </span>
                        )}
                    </div>
                    {(!isCollapsible || isOpen) && (
                        <span className="text-zinc-500 hover:text-zinc-300 transition-colors ml-2 shrink-0">
                            {isExpanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
                        </span>
                    )}
                </div>

                {isExpanded && (!isCollapsible || isOpen) && (
                    <div className="pl-6 space-y-1 border-l border-zinc-800/80 transition-all duration-350 overflow-hidden box-border max-w-full">
                        {item.subItems!.map((sub, idx) => {
                            const searchParams = new URLSearchParams(location.search);
                            const subParams = new URLSearchParams(sub.path.includes('?') ? sub.path.split('?')[1] : '');
                            const isSubActive = location.pathname === (sub.path.includes('?') ? sub.path.split('?')[0] : sub.path) &&
                                Array.from(subParams.keys()).every(key => searchParams.get(key) === subParams.get(key));

                            return (
                                <NavLink
                                    key={idx}
                                    to={sub.path}
                                    className={`
                                        flex items-center py-2 px-3 rounded-lg text-xs transition-all duration-200 min-w-0
                                        ${isSubActive
                                            ? 'text-rose-500 font-bold bg-zinc-900/60'
                                            : 'text-zinc-500 hover:text-zinc-300 hover:bg-zinc-900/30'}
                                    `}
                                >
                                    <span className="truncate flex-1 text-left">{sub.name}</span>
                                </NavLink>
                            );
                        })}
                    </div>
                )}
            </div>
        );
    }

    if (item.onClick) {
        return (
            <div
                className={`${getNavLinkClasses(isItemActive)} w-full justify-start cursor-pointer select-none`}
                onClick={item.onClick}
                role="button"
                tabIndex={0}
                onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        item.onClick?.();
                    }
                }}
            >
                {renderActiveIndicator(isItemActive)}
                <span className={`min-w-[24px] flex items-center justify-center ${isItemActive ? 'text-rose-500' : 'text-zinc-400'}`}>
                    {item.icon}
                </span>
                {(!isCollapsible || isOpen) && (
                    <span className="ml-4 text-left">
                        {item.name}
                    </span>
                )}
            </div>
        );
    }

    return (
        <NavLink
            to={item.path || '#'}
            end={item.end}
            className={getNavLinkClasses(isItemActive)}
        >
            {renderActiveIndicator(isItemActive)}
            <span className={`min-w-[24px] flex items-center justify-center ${isItemActive ? 'text-rose-500' : 'text-zinc-400'}`}>
                {item.icon}
            </span>
            {(!isCollapsible || isOpen) && (
                <span className="transition-all duration-300 ml-4">
                    {item.name}
                </span>
            )}
        </NavLink>
    );
}

export function BaseSidebar({
    isOpen = true,
    toggleSidebar,
    isCollapsible = true,
    sections,
    bottomItems = [],
    headerContent,
}: BaseSidebarProps) {
    const location = useLocation();

    const getNavLinkClasses = (isActive: boolean) => {
        return `
            flex items-center p-2.5 rounded-xl transition-all duration-200 relative group text-sm
            ${isOpen ? 'px-3 mx-1' : 'justify-center mx-1'}
            ${isActive
                ? 'bg-gradient-to-r from-zinc-950 via-rose-600/20 to-pink-500/50 text-zinc-50 font-bold'
                : 'text-zinc-400 hover:bg-zinc-900/40 hover:text-zinc-200'
            }
        `;
    };

    const renderActiveIndicator = (isActive: boolean) => {
        if (!isActive) return null;
        return (
            <span
                className="absolute right-0 top-2 bottom-2 w-1 bg-gradient-to-b from-rose-500 to-pink-500 rounded-l-full shadow-[0_0_12px_rgba(244,63,94,0.6)]" />
        );
    };

    const checkActive = (item: SidebarItem): boolean | undefined => {
        if (item.isActive !== undefined) {
            return typeof item.isActive === 'function' ? item.isActive(location.pathname) : item.isActive;
        }
        if (!item.path) return false;
        return item.end ? location.pathname === item.path : location.pathname.startsWith(item.path);
    };

    const mobileItems = [
        ...sections.flatMap(s => s.items),
        ...bottomItems
    ].filter(item => item.path || item.onClick);

    return (
        <>
            <div className="md:hidden fixed bottom-0 left-0 right-0 h-16 bg-zinc-950/95 backdrop-blur-md border-t border-zinc-800/60 flex items-center justify-around px-2 z-[50] select-none">
                {mobileItems.map((item, idx) => {
                    const isActive = checkActive(item) || false;
                    const content = (
                        <span className={`flex items-center justify-center p-3 rounded-2xl transition-all duration-200 ${isActive
                            ? 'text-rose-500 bg-rose-500/10'
                            : 'text-zinc-400 hover:text-zinc-200'
                            }`}>
                            {React.isValidElement(item.icon)
                                ? React.cloneElement(item.icon as React.ReactElement<any>, { size: 22 })
                                : item.icon
                            }
                        </span>
                    );

                    if (item.onClick) {
                        return (
                            <button
                                key={idx}
                                onClick={item.onClick}
                                className="flex-grow flex justify-center focus:outline-none"
                            >
                                {content}
                            </button>
                        );
                    }

                    return (
                        <NavLink
                            key={idx}
                            to={item.path || '#'}
                            end={item.end}
                            className="flex-grow flex justify-center"
                        >
                            {content}
                        </NavLink>
                    );
                })}
            </div>

            <aside
                className={`hidden md:flex bg-zinc-950 p-3 transition-all duration-300 flex-col justify-between select-none z-[50] sticky top-0 h-screen shrink-0
                    ${isCollapsible
                        ? (isOpen ? 'w-60 lg:w-64' : 'w-18')
                        : 'w-64 border-r border-zinc-800'
                    }`}
            >
                <div className="flex flex-col flex-1 overflow-y-auto overflow-x-hidden no-scrollbar">
                    <div className={`h-14 flex items-center mb-4 gap-3 shrink-0 transition-all duration-300 
                        ${isCollapsible
                            ? (isOpen ? 'px-2 justify-start' : 'justify-center')
                            : 'p-6 mb-4'
                        }`}
                    >
                        {isCollapsible && toggleSidebar && (
                            <Button
                                variant="icon"
                                onClick={toggleSidebar}
                                className="text-zinc-400 hover:text-white transition-colors shrink-0"
                            >
                                <Menu size={24} />
                            </Button>
                        )}

                        {(!isCollapsible || isOpen) && (
                            <Link to="/"
                                className="flex items-center active:scale-[0.98] cursor-pointer animate-fadeIn min-w-0 overflow-hidden">
                                <div className="h-9 w-9 rounded-xl bg-gradient-to-tr from-rose-600 to-pink-500 flex items-center justify-center text-white shadow-md shrink-0">
                                    <Video size={20} />
                                </div>

                                <h2
                                    className="text-[#ff2a6d] ml-2 text-lg lg:text-xl font-black tracking-tight whitespace-nowrap bg-clip-text truncate">
                                    VideoHosting
                                </h2>
                            </Link>
                        )}
                    </div>

                    {headerContent && (
                        <>
                            {headerContent}
                            <div className="h-[1px] w-full bg-gradient-to-r from-zinc-800/80 to-transparent mb-3 mx-1" />
                        </>
                    )}

                    <nav className="flex-1 space-y-4">
                        {sections.map((section, sectionIdx) => (
                            <div key={sectionIdx} className="space-y-1">
                                {section.title && (!isCollapsible || isOpen) && (
                                    <p className="px-3 mb-1 text-[9px] font-black uppercase tracking-[0.2em] text-zinc-600">
                                        {section.title}
                                    </p>
                                )}

                                <div className="space-y-1">
                                    {section.items.map((item, itemIdx) => (
                                        <SidebarItemRow
                                            key={itemIdx}
                                            item={item}
                                            isOpen={isOpen}
                                            isCollapsible={isCollapsible}
                                            getNavLinkClasses={getNavLinkClasses}
                                            renderActiveIndicator={renderActiveIndicator}
                                            checkActive={checkActive}
                                        />
                                    ))}
                                </div>

                                {sectionIdx < sections.length - 1 && (
                                    <hr className="border-zinc-500/60 my-3 mx-1" />
                                )}
                            </div>
                        ))}
                    </nav>
                </div>

                {bottomItems.length > 0 && (
                    <div className="pt-2 border-t border-zinc-900/60">
                        {bottomItems.map((item, idx) => (
                            <SidebarItemRow
                                key={idx}
                                item={item}
                                isOpen={isOpen}
                                isCollapsible={isCollapsible}
                                getNavLinkClasses={getNavLinkClasses}
                                renderActiveIndicator={renderActiveIndicator}
                                checkActive={checkActive}
                            />
                        ))}
                    </div>
                )}
            </aside>
        </>
    );
}

export default BaseSidebar;
