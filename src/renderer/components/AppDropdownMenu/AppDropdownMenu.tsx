import { memo } from 'react';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '../ui/dropdown-menu';

const AppDropdownMenu = ({ app }: { app: SteamApp }) => {
  return (
    <div className='absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity'>
      <DropdownMenu modal={false}>
        <DropdownMenuTrigger asChild>
          <button
            onClick={(e) => {
              e.stopPropagation();
              e.preventDefault();
            }}
            className='text-gray-500 hover:text-gray-800 bg-white rounded cursor-pointer'
          >
            ⋯
          </button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align='start' className='z-[100] bg-white'>
          <DropdownMenuItem
            className='hover:bg-gray-100 hover:font-semibold cursor-pointer px-2 py-1 rounded'
            onClick={() => console.log(`Sil: ${app.name}`)}
          >
            Sil
          </DropdownMenuItem>
          <DropdownMenuItem
            className='hover:bg-gray-100 hover:font-semibold cursor-pointer px-2 py-1 rounded'
            onClick={() => console.log(`Sıkıştır: ${app.name}`)}
          >
            Sıkıştır
          </DropdownMenuItem>
          <DropdownMenuItem
            className='hover:bg-gray-100 hover:font-semibold cursor-pointer px-2 py-1 rounded'
            onClick={() => console.log(`Detaylar: ${app.name}`)}
          >
            Detaylar
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
};

export default memo(AppDropdownMenu);
