import { memo } from 'react';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import useLibraries from 'src/renderer/hooks/useLibraries';
import DroppableLibrary from '../DroppableLibrary';
import LibraryCreationDialog from '../LibraryCreationDialog';

interface Props {
  onSelect: (lib: SteamLibrary) => () => void;
}

const LibraryList = ({ onSelect }: Props) => {
  const { libraries, selectedLibrary } = useLibraries();

  return (
    <aside className='shrink-0 w-72 border-r border-gray-200 bg-gray-50 p-4'>
      <h2 className='text-lg font-semibold mb-4 text-gray-700'>Kütüphaneler</h2>
      <ul className='space-y-1'>
        {libraries.map((library) => (
          <DroppableLibrary
            library={library}
            key={library.id}
            isSelected={selectedLibrary?.id === library.id}
            onClick={onSelect(library)}
          />
        ))}
      </ul>
      <LibraryCreationDialog />
    </aside>
  );
};

export default memo(LibraryList);
