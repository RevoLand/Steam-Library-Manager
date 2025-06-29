import { DndContext, DragEndEvent } from '@dnd-kit/core';
import { useState } from 'react';
import TransferMode from 'src/core/models/TransferMode';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import DraggableGame from '../components/DraggableGame';
import DroppableLibrary from '../components/DroppableLibrary';
import LibraryCreationDialog from '../components/LibraryCreationDialog';
import useLibraries from '../hooks/useLibraries';
import useTasks from '../hooks/useTasks';

const Index = () => {
  const [selectedLibrary, setSelectedLibrary] = useState<SteamLibrary>();
  const { libraries } = useLibraries();
  const { addTask } = useTasks();

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (over && selectedLibrary && over.id !== selectedLibrary.id) {
      const { app } = active.data.current as { app: SteamApp; library: SteamLibrary };
      const droppedLibrary = over.data.current as SteamLibrary;

      addTask(app, droppedLibrary, TransferMode.COPY | TransferMode.VERIFY);
    }
  };

  return (
    <DndContext onDragEnd={handleDragEnd}>
      <div className='flex size-full'>
        <aside className='w-72 border-r border-gray-200 bg-gray-50 p-4'>
          <h2 className='text-lg font-semibold mb-4 text-gray-700'>Kütüphaneler</h2>
          <ul className='space-y-1'>
            {libraries.map((library) => (
              <DroppableLibrary
                library={library}
                key={library.id}
                isSelected={selectedLibrary?.id === library.id}
                onClick={() => setSelectedLibrary(library)}
              />
            ))}
          </ul>
          <LibraryCreationDialog />
        </aside>

        {selectedLibrary && (
          <section className='flex-1 p-6 bg-white'>
            <h2 className='text-xl font-semibold mb-6 text-gray-800'>
              Seçilen Kütüphane: <span className='text-blue-600'>{selectedLibrary.path}</span>
            </h2>
            <div className='grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4'>
              {selectedLibrary.apps.map((app) => (
                <DraggableGame key={app.appId} app={app} library={selectedLibrary} />
              ))}
            </div>
          </section>
        )}
      </div>
    </DndContext>
  );
};

export default Index;
