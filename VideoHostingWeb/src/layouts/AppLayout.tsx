import BaseLayout from './BaseLayout';
import Sidebar from '../components/layout/Sidebar';
import Header from '../components/layout/Header';
import Footer from '../components/layout/Footer';

function AppLayout() {
    return (
        <BaseLayout
            sidebar={({ isOpen, toggleSidebar }) => (
                <Sidebar isOpen={isOpen} toggleSidebar={toggleSidebar} />
            )}
            header={<Header />}
            footer={<Footer />}
        />
    );
}

export default AppLayout;
