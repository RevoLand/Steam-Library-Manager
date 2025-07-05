import { useDroppable } from '@dnd-kit/core';
import clsx from 'clsx';
import { memo } from 'react';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

const DroppableLibrary = ({
  library,
  isSelected,
  onClick,
}: {
  library: SteamLibrary;
  isSelected: boolean;
  onClick: () => void;
}) => {
  const { setNodeRef, isOver } = useDroppable({
    id: library.id,
    data: library,
    disabled: isSelected,
  });
  const showDropOverlay = isOver && !isSelected;

  return (
    <li
      ref={setNodeRef}
      onClick={onClick}
      className={clsx('px-3 py-2 rounded cursor-pointer transition-all select-none overflow-hidden wrap-anywhere', {
        'bg-blue-100 text-blue-800 font-semibold': isSelected,
        'bg-green-100 text-green-800': showDropOverlay,
        'hover:bg-gray-200 text-gray-700': !isSelected && !showDropOverlay,
      })}
    >
      {library.label && <strong>{library.label}</strong>}
      <div>{library.path}</div>
      {showDropOverlay && <div className='text-xs text-green-600 mt-1'>Bırakmak için uygun</div>}
    </li>
  );
};

export default memo(DroppableLibrary);
