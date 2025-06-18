import { Outlet } from 'react-router-dom';
import useProfiles from 'src/renderer/hooks/useProfile';
import StatusBar from './StatusBar';
import Topbar from './Topbar';

export default function Layout() {
  const { profile } = useProfiles();

  if (!profile) {
    return null;
  }

  return (
    <div className='flex flex-col h-screen w-screen bg-gray-100 text-gray-900'>
      <Topbar />
      <div className='flex flex-1 overflow-hidden'>
        <Outlet />
      </div>
      <StatusBar />
    </div>
  );
}
