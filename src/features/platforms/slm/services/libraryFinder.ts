import { existsSync } from 'node:fs';
import { ProfileManager } from 'src/core/config/profile/ProfileManager';
import { SteamLibrary } from 'src/features/platforms/steam/models/SteamLibrary';
import { hashText } from 'src/core/utils/hash';

export const findAllSLMLibraries = async (): Promise<SteamLibrary[]> => {
  const libraries: Set<string> = new Set();

  const savedLibraries = ProfileManager.getSetting<string[]>('slmLibraries') ?? [];

  for (const lib of savedLibraries) {
    if (existsSync(lib)) {
      libraries.add(lib);
    }
  }

  ProfileManager.setSetting('slmLibraries', Array.from(libraries));

  const promises = Array.from(libraries).map(async (path) => {
    const id = await hashText(path);

    return new SteamLibrary(id, path, 'slm');
  });

  return Promise.all(promises);
};
