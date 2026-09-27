import { Home, Plus } from 'lucide-react';
import BaseSidebar, { type SidebarSection } from './BaseSidebar';

interface SidebarProps {
    isOpen: boolean;
    toggleSidebar: () => void;
}

function Sidebar({ isOpen, toggleSidebar }: SidebarProps) {
    const sections: SidebarSection[] = [
        {
            items: [
                { name: 'Головна', path: '/', end: true, icon: <Home size={18} /> },
                { name: 'Додати відео', path: '/video/add', icon: <Plus size={18} /> },
            ]
        },
    ];

    return (
        <BaseSidebar
            isOpen={isOpen}
            toggleSidebar={toggleSidebar}
            isCollapsible={true}
            sections={sections}
        />
    );
}

export default Sidebar;
