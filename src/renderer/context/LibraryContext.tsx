import { createContext, PropsWithChildren, useCallback, useEffect, useMemo, useState } from 'react';
import LibraryType from 'src/core/models/LibraryType';
import { libraryDeserializer } from 'src/features/libraries/services/libraryDeserializer';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

interface LibraryContextValue {
  create: (path: string, label: string, type: LibraryType) => Promise<SteamLibrary>;
  libraries: SteamLibrary[];
  refreshLibraries: () => Promise<void>;
}

export const LibraryContext = createContext<LibraryContextValue>(undefined);

LibraryContext.displayName = 'LibraryContext';

export default function LibraryProvider(props: PropsWithChildren) {
  const [libraries, setLibraries] = useState<SteamLibrary[]>([]);

  const refreshLibraries = useCallback(async () => {
    const libraryList = await window.api['get-libraries']();

    setLibraries(libraryList.map((library) => libraryDeserializer.fromDTO(library)));
  }, []);

  useEffect(() => {
    const subscribeToLibraryUpdate = () => {
      window.events['update-library']((updatedLibrary) => {
        setLibraries((prev) => {
          const existing = prev.find((t) => t.id === updatedLibrary.id);

          if (existing) {
            return prev.map((t) => (t.id === updatedLibrary.id ? libraryDeserializer.fromDTO(updatedLibrary) : t));
          }

          return [...prev, libraryDeserializer.fromDTO(updatedLibrary)];
        });
      });
    };

    const subscribeToLibraryUpdates = () => {
      window.events['update-libraries']((updatedLibraries) => {
        setLibraries(updatedLibraries.map((library) => libraryDeserializer.fromDTO(library)));
      });
    };

    refreshLibraries();
    subscribeToLibraryUpdate();
    subscribeToLibraryUpdates();
  }, []);

  const create = useCallback((path: string, label: string, type: LibraryType) => {
    return window.api['create-library'](path, label, type);
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
