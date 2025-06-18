import useProfiles from 'src/renderer/hooks/useProfile';
import useTasks from 'src/renderer/hooks/useTasks';

const StatusBar = () => {
  const { profile } = useProfiles();
  const { tasks } = useTasks();

  return (
    <footer className='h-12 w-full px-6 flex items-center justify-between border-t border-gray-300 bg-white text-sm shadow-sm'>
      <div className='flex items-center gap-6 text-gray-600'>
        <span>
          👤 <span className='font-medium'>{profile.name}</span>
        </span>
        <span>
          🔄 Aktif Görev: <span className='font-medium'>{tasks.length}</span>
        </span>
      </div>

      <div className='flex items-center gap-6 text-gray-600'>
        <span>📦 Portal → E:\Games (%100)</span>
        <span>📦 Hades → C:\SSD (%42)</span>
      </div>
    </footer>
  );
};

export default StatusBar;
