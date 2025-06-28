import { existsSync } from 'node:fs';
import ProfileManager from 'src/core/config/profile/ProfileManager';
import { hashText } from 'src/core/utils/hash';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

export const findAllSLMLibraries = async (): Promise<SteamLibrary[]> => {
  const slmLibraries = (ProfileManager.getSetting('slmLibraries') as Record<string, string>) ?? {};

  const validEntries = Object.entries(slmLibraries).filter(([path]) => existsSync(path));
  const validMap = Object.fromEntries(validEntries);

  ProfileManager.setSetting('slmLibraries', validMap);

  const promises = validEntries.map(async ([path, label]) => {
    const id = await hashText(path);

    return new SteamLibrary(id, path, label, 'slm');
  });

  return Promise.all(promises);
};
