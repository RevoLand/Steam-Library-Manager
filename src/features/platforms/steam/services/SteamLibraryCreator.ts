import LibraryCreator from 'src/features/libraries/models/LibraryCreator';
import SteamLibrary from '../models/SteamLibrary';

export default class SteamLibraryCreator implements LibraryCreator {
  async create(path: string): Promise<SteamLibrary> {
    // vdf yaz
    // model oluştur
    return new SteamLibrary(path, 'steam');
  }
}
