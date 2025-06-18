import { SteamLibrary } from 'src/features/platforms/steam/models/SteamLibrary';

export interface LibraryCreator {
  create(path: string): Promise<SteamLibrary>;
}
