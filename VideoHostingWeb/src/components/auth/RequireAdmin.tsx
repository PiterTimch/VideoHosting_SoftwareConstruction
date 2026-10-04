import RequireRole from './RequireRole';

const RequireAdmin = () => <RequireRole allowedRoles={['Admin']} />;

export default RequireAdmin;
