import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

interface LibraryCreator {
  create(path: string): Promise<SteamLibrary>;
}

export default LibraryCreator;
