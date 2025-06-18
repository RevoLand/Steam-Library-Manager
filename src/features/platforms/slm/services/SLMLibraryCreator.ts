import { ProfileManager } from 'src/core/config/profile/ProfileManager';
import { hashText } from 'src/core/utils/hash';
import { LibraryCreator } from 'src/features/libraries/models/LibraryCreator';
import { SteamLibrary } from '../../steam/models/SteamLibrary';

export default class SLMLibraryCreator implements LibraryCreator {
  async create(path: string): Promise<SteamLibrary> {
    const slmLibraries = ProfileManager.getSetting<string[]>('slmLibraries') ?? [];

    slmLibraries.push(path);
    ProfileManager.setSetting('slmLibraries', slmLibraries);

    const id = await hashText(path);

    return new SteamLibrary(id, path, 'slm');
  }
}
