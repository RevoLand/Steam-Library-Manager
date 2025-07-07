import { DndContext, DragEndEvent } from '@dnd-kit/core';
import { restrictToWindowEdges } from '@dnd-kit/modifiers';
import { useCallback, useTransition } from 'react';
import { VirtuosoGrid } from 'react-virtuoso';
import TransferMode from 'src/core/models/TransferMode';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import DraggableGame from '../components/DraggableGame';
import DroppableLibrary from '../components/DroppableLibrary';
import LibraryCreationDialog from '../components/LibraryCreationDialog';
import useLibraries from '../hooks/useLibraries';
import useTasks from '../hooks/useTasks';

const Index = () => {
  const { libraries, selectedLibrary, setSelectedLibrary } = useLibraries();
  const { addTask } = useTasks();
  const [isPending, startTransition] = useTransition();
  const useVirtuosoForAppListing = selectedLibrary && selectedLibrary.appCount >= 200;

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (over && selectedLibrary && over.id !== selectedLibrary.id) {
      const app = active.data.current as SteamApp;
      const droppedLibrary = over.data.current as SteamLibrary;

      addTask(app, droppedLibrary, TransferMode.COPY | TransferMode.VERIFY);
    }
  };

  const handleLibraryClick = useCallback(
    (lib: SteamLibrary) => () =>
      !isPending &&
      startTransition(() => {
        setSelectedLibrary(lib);
      }),
    []
  );

  return (
    <div className='flex size-full'>
      <DndContext autoScroll={false} onDragEnd={handleDragEnd} modifiers={[restrictToWindowEdges]}>
        <aside className='shrink-0 w-72 border-r border-gray-200 bg-gray-50 p-4'>
          <h2 className='text-lg font-semibold mb-4 text-gray-700'>Kütüphaneler</h2>
          <ul className='space-y-1'>
            {libraries.map((library) => (
              <DroppableLibrary
                library={library}
                key={library.id}
                isSelected={selectedLibrary?.id === library.id}
                onClick={handleLibraryClick(library)}
              />
            ))}
          </ul>
          <LibraryCreationDialog />
        </aside>
        {isPending && <div className='flex-1 flex items-center justify-center bg-white text-gray-500'>Loading...</div>}
        {!isPending && selectedLibrary && (
          <section className='flex flex-col flex-1 min-h-0 p-4 pr-0 bg-white'>
            <h2 className='text-xl font-semibold mb-6 text-gray-800'>
              Seçilen Kütüphane: <span className='text-blue-600'>{selectedLibrary.path}</span> (
              {selectedLibrary.appCount})
            </h2>

            {useVirtuosoForAppListing && (
              <VirtuosoGrid
                listClassName='grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-4 pr-1'
                data={selectedLibrary.apps}
                overscan={{
                  main: 400,
                  reverse: 400,
                }}
                itemContent={(_, app) => <DraggableGame key={app.appId} app={app} />}
              />
            )}
            {!useVirtuosoForAppListing && (
              <div className='flex-1 pr-1 overflow-auto will-change-transform'>
                <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-4'>
                  {selectedLibrary.apps.map((app) => (
                    <DraggableGame key={app.appId} app={app} />
                  ))}
                </div>
              </div>
            )}
          </section>
        )}
      </DndContext>
    </div>
  );
};

export default Index;
