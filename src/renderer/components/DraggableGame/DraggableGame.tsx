import { useDraggable } from '@dnd-kit/core';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

const DraggableGame = ({ app, library }: { app: SteamApp; library: SteamLibrary }) => {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: app.appId,
    data: { app, library },
  });

  return (
    <div
      ref={setNodeRef}
      {...listeners}
      {...attributes}
      className={`bg-white border rounded shadow-sm hover:shadow-md transition-shadow cursor-move group z-50
        ${isDragging ? 'opacity-50 scale-95 z-[999]' : ''}`}
      style={{ transform: transform ? `translate(${transform.x}px, ${transform.y}px)` : undefined }}
    >
      <div className='w-full overflow-hidden rounded-t'>
        <img
          src={app.image}
          alt={app.name}
          className='object-cover w-full h-full aspect-[460/215] group-hover:scale-[1.02] transition-transform'
        />
      </div>
      <div className='p-2 text-center text-sm text-gray-800 font-medium truncate'>{app.name}</div>
    </div>
  );
};

export default DraggableGame;
