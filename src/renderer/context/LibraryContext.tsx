import { createContext, PropsWithChildren, useCallback, useEffect, useMemo, useState } from 'react';
import { LibraryDeserializer } from 'src/features/libraries/services/LibraryDeserializer';
import { LibraryType, SteamLibrary } from 'src/features/platforms/steam/models/SteamLibrary';

interface LibraryContextValue {
  create: (path: string, type: LibraryType) => Promise<SteamLibrary>;
  libraries: SteamLibrary[];
  refreshLibraries: () => Promise<void>;
}

export const LibraryContext = createContext<LibraryContextValue>(undefined);

LibraryContext.displayName = 'LibraryContext';

export default function LibraryProvider(props: PropsWithChildren) {
  const [libraries, setLibraries] = useState<SteamLibrary[]>();

  const refreshLibraries = useCallback(async () => {
    const libraryList = await window.api['get-libraries']();

    setLibraries(libraryList.map(LibraryDeserializer.fromDTO));
  }, []);

  useEffect(() => {
    refreshLibraries();
  }, []);

  const create = useCallback((path: string, type: LibraryType) => {
    return window.api['create-library'](path, type);
  }, []);

  const libraryContextValue = useMemo(
    () => ({
      create,
      libraries,
      refreshLibraries,
    }),
    [libraries]
  );

  return <LibraryContext.Provider value={libraryContextValue}>{props.children}</LibraryContext.Provider>;
}
