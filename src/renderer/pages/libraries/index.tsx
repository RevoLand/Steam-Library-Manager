import { DndContext, DragEndEvent, DragOverlay, DragStartEvent } from '@dnd-kit/core';
import { restrictToWindowEdges } from '@dnd-kit/modifiers';
import { useCallback, useState, useTransition } from 'react';
import TransferMode from 'src/core/models/TransferMode';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import AppList from 'src/renderer/components/apps/AppList';
import DraggableApp from 'src/renderer/components/apps/DraggableApp';
import LibraryList from 'src/renderer/components/libraries/LibraryList';
import useLibraries from 'src/renderer/hooks/useLibraries';
import useTasks from 'src/renderer/hooks/useTasks';

const Libraries = () => {
  const [draggingApp, setDraggingApp] = useState<SteamApp | null>(null);
  const { selectedLibrary, setSelectedLibrary } = useLibraries();
  const { addTask } = useTasks();
  const [isPending, startTransition] = useTransition();

  const handleDragStart = (event: DragStartEvent) => {
    setDraggingApp(event.active.data.current as SteamApp);
  };

  const handleDragEnd = ({ active, over }: DragEndEvent) => {
    setDraggingApp(null);

    if (over && selectedLibrary && over.id !== selectedLibrary.id) {
      const app = active.data.current as SteamApp;
      const droppedLibrary = over.data.current as SteamLibrary;

      addTask(app, droppedLibrary, TransferMode.COPY | TransferMode.VERIFY);
    }
  };

  const onLibrarySelect = useCallback(
    (lib: SteamLibrary) => () =>
      !isPending &&
      startTransition(() => {
        setSelectedLibrary(lib);
      }),
    [isPending]
  );

  return (
    <div className='flex size-full'>
      <DndContext
        autoScroll={false}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
        modifiers={[restrictToWindowEdges]}
      >
        <LibraryList onSelect={onLibrarySelect} />
        <AppList isPending={isPending} />

        <DragOverlay>{draggingApp && <DraggableApp app={draggingApp} isOverlay />}</DragOverlay>
      </DndContext>
    </div>
  );
};

export default Libraries;
