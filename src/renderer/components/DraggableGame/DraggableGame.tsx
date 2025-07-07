import { useDraggable } from '@dnd-kit/core';
import clsx from 'clsx';
import { memo } from 'react';
import { formatBytes } from 'src/core/utils/bytes';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import AppDropdownMenu from '../AppDropdownMenu';

interface Props {
  app: SteamApp;
  isOverlay?: boolean;
}

const DraggableGame = ({ app, isOverlay = false }: Props) => {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: app.appId,
    data: app,
  });

  return (
    <div
      ref={!isOverlay ? setNodeRef : undefined}
      className={clsx(
        'bg-white border rounded shadow-sm hover:shadow-md transition-shadow group relative',
        'will-change-transform contain-[layout_style]',
        {
          'opacity-50': isDragging || isOverlay,
          'scale-95 z-50': isOverlay,
        }
      )}
      style={{
        transform: isOverlay && transform ? `translate(${transform.x}px, ${transform.y}px)` : undefined,
        pointerEvents: isOverlay ? 'none' : undefined,
      }}
    >
      <div
        className='w-full aspect-[460/215] overflow-hidden rounded-t cursor-move'
        {...(!isOverlay ? { ...attributes, ...listeners } : {})}
      >
        <img loading='lazy' src={app.image} alt={app.name} className='object-cover size-full' />
      </div>
      <div
        className='p-2 text-center text-sm text-gray-800 font-medium truncate'
        title={`${app.name} • ${formatBytes(app.sizeOnDisk)}`}
      >
        {app.name}
      </div>

      {!isDragging && !isOverlay && <AppDropdownMenu app={app} />}
    </div>
  );
};

export default memo(DraggableGame);
