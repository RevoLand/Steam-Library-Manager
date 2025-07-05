import { useDraggable } from '@dnd-kit/core';
import { formatBytes } from 'src/core/utils/bytes';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '../ui/dropdown-menu';

const DraggableGame = ({ app, library }: { app: SteamApp; library: SteamLibrary }) => {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: app.appId,
    data: { app, library },
  });

  return (
    <div
      ref={setNodeRef}
      {...attributes}
      className={`bg-white border rounded shadow-sm hover:shadow-md transition-shadow group z-50 relative
        ${isDragging ? 'opacity-50 scale-95 z-[999]' : ''}`}
      style={{ transform: transform ? `translate(${transform.x}px, ${transform.y}px)` : undefined }}
    >
      <div className='w-full overflow-hidden rounded-t cursor-move' {...listeners}>
        <img src={app.image} alt={app.name} className='object-cover size-full' />
      </div>
      <div
        className='p-2 text-center text-sm text-gray-800 font-medium truncate'
        title={`${app.name} • ${formatBytes(app.sizeOnDisk)}`}
      >
        {app.name}
      </div>
      <div className='absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity'>
        <DropdownMenu>
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
          <DropdownMenuContent align='start' className='z-[1000] bg-white'>
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
    </div>
  );
};

export default DraggableGame;
