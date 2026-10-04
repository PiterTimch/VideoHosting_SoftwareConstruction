import RequireRole from './RequireRole';

const RequireAuthor = () => <RequireRole allowedRoles={['Author', 'Admin', 'User']} />;

export default RequireAuthor;
