import { useDroppable } from '@dnd-kit/core';
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

  return (
    <li
      key={library.id}
      ref={setNodeRef}
      onClick={onClick}
      className={`px-3 py-2 rounded cursor-pointer transition-all select-none overflow-hidden wrap-anywhere
                    ${isSelected ? 'bg-blue-100 text-blue-800 font-semibold' : ''}
                    ${!isSelected && isOver ? 'bg-green-100 text-green-800' : 'hover:bg-gray-200 text-gray-700'}`}
    >
      {library.label && <strong>{library.label}</strong>}
      <div>{library.path}</div>
      {isOver && !isSelected && <div className='text-xs text-green-600 mt-1 animate-pulse'>Bırakmak için uygun</div>}
    </li>
  );
};

export default DroppableLibrary;
