import { Home, Plus, User as UserIcon } from 'lucide-react';
import BaseSidebar, { type SidebarSection } from './BaseSidebar';
import { useAppSelector } from '../../store';

interface SidebarProps {
    isOpen: boolean;
    toggleSidebar: () => void;
}

function Sidebar({ isOpen, toggleSidebar }: SidebarProps) {
    const { user } = useAppSelector(state => state.auth);

    const sections: SidebarSection[] = [
        {
            items: [
                { name: 'Головна', path: '/', end: true, icon: <Home size={18} /> },
                { name: 'Додати відео', path: '/video/add', icon: <Plus size={18} /> },
                ...(user ? [{ name: 'Мій профіль / Портфоліо', path: '/account', icon: <UserIcon size={18} /> }] : []),
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
