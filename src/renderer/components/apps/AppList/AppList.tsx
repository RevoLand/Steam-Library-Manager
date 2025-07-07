import { memo } from 'react';
import { VirtuosoGrid } from 'react-virtuoso';
import useLibraries from 'src/renderer/hooks/useLibraries';
import { cn } from 'src/renderer/utils/style';
import DraggableApp from '../DraggableApp';

const GRID_CLASS = 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-4';

interface Props {
  isPending: boolean;
}

const AppList = ({ isPending }: Props) => {
  const { selectedLibrary } = useLibraries();
  const shouldVirtualize = Boolean(selectedLibrary?.appCount >= 200);

  if (isPending) {
    return <div className='flex-1 flex items-center justify-center bg-white text-gray-500'>Loading...</div>;
  }

  if (!selectedLibrary) {
    return null;
  }

  return (
    <section className='flex flex-col flex-1 min-h-0 p-4 pr-0 bg-white select-none'>
      <h2 className='text-xl font-semibold mb-6 text-gray-800'>
        Seçilen Kütüphane: <span className='text-blue-600'>{selectedLibrary.path}</span> ({selectedLibrary.appCount})
      </h2>

      {shouldVirtualize && (
        <VirtuosoGrid
          listClassName={cn(GRID_CLASS, 'pr-1')}
          data={selectedLibrary.apps}
          overscan={{
            main: 400,
            reverse: 400,
          }}
          itemContent={(_, app) => <DraggableApp app={app} />}
        />
      )}
      {!shouldVirtualize && (
        <div className='flex-1 pr-1 overflow-auto will-change-transform'>
          <div className={cn(GRID_CLASS)}>
            {selectedLibrary.apps.map((app) => (
              <DraggableApp key={app.appId} app={app} />
            ))}
          </div>
        </div>
      )}
    </section>
  );
};

export default memo(AppList);
