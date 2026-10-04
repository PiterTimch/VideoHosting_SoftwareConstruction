import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAppSelector } from '../../store/index';

interface RequireRoleProps {
    allowedRoles?: string[];
    fallback?: string;
}

const RequireRole = ({ allowedRoles = [], fallback = '/' }: RequireRoleProps) => {
    const { user } = useAppSelector((state) => state.auth);
    const location = useLocation();

    if (!user) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (allowedRoles.length > 0 && !user.roles.some(role => allowedRoles.includes(role))) {
        return <Navigate to={fallback} replace />;
    }

    return <Outlet />;
};

export default RequireRole;
