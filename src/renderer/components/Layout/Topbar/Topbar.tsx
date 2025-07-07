import { NavLink } from 'react-router-dom';

const Topbar = () => {
  return (
    <header className='h-12 w-full px-6 flex items-center justify-between border-b border-gray-300 bg-white shadow-sm'>
      <nav className='flex gap-6 text-sm font-medium text-gray-600'>
        <NavLink
          to='/'
          className={({ isActive }) => (isActive ? 'text-blue-600 font-semibold' : 'hover:text-gray-900')}
        >
          Dashboard
        </NavLink>
        <NavLink
          to='/libraries'
          className={({ isActive }) => (isActive ? 'text-blue-600 font-semibold' : 'hover:text-gray-900')}
        >
          Libraries
        </NavLink>
        <NavLink
          to='/tasks'
          className={({ isActive }) => (isActive ? 'text-blue-600 font-semibold' : 'hover:text-gray-900')}
        >
          Görevler
        </NavLink>
        <NavLink
          to='/settings'
          className={({ isActive }) => (isActive ? 'text-blue-600 font-semibold' : 'hover:text-gray-900')}
        >
          Ayarlar
        </NavLink>
      </nav>

      <div className='flex items-center gap-4 text-sm text-gray-600'>
        <button className='hover:text-gray-900'>Profil</button>
        <button className='hover:text-gray-900'>Çıkış</button>
      </div>
    </header>
  );
};

export default Topbar;
