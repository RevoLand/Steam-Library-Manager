import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import type SteamLibraryDTO from 'src/features/platforms/steam/models/SteamLibraryDTO';

type LibraryResolver = (dto: SteamLibraryDTO) => SteamLibrary;

class LibraryDeserializer {
  private registry = new Map<string, LibraryResolver>();

  register(type: string, resolver: LibraryResolver): void {
    this.registry.set(type, resolver);
  }

  fromDTO(dto: SteamLibraryDTO): SteamLibrary {
    const resolver = this.registry.get(dto.type);

    if (!resolver) {
      throw new Error(`Unknown library type: ${dto.type}`);
    }

    return resolver(dto);
  }

  clear(): void {
    this.registry.clear();
  }
}

export const libraryDeserializer = new LibraryDeserializer();
