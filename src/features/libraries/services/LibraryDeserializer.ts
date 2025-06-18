import { SteamLibrary } from 'src/features/platforms/steam/models/SteamLibrary';
import type { SteamLibraryDTO } from '../../platforms/steam/models/SteamLibraryDTO';

type ResolverFn = (dto: SteamLibraryDTO) => SteamLibrary;

const registry = new Map<string, ResolverFn>();

export const LibraryDeserializer = {
  register(type: string, resolver: ResolverFn) {
    registry.set(type, resolver);
  },

  fromDTO(dto: SteamLibraryDTO): SteamLibrary {
    const fn = registry.get(dto.type);

    if (!fn) {
      throw new Error(`Unknown library type: ${dto.type}`);
    }

    return fn(dto);
  },
};
