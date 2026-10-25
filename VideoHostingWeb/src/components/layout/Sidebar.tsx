import { Home, Plus, User as UserIcon, Shield, Clapperboard } from 'lucide-react';
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
                ...(user ? [{ name: 'Мій профіль', path: '/account', icon: <UserIcon size={18} /> }, { name: 'Моє портфоліо', path: `/portfolio/${user.id}`, icon: <Clapperboard size={18} /> }] : []),
                ...(user?.roles.some(role => role.toLowerCase() === 'admin') ? [{ name: 'Адмінпанель', path: '/admin', icon: <Shield size={18} /> }] : []),
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
