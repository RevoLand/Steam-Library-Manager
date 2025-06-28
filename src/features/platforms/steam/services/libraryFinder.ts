import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { hashText } from 'src/core/utils/hash';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import { parse } from 'vdf-parser';
import LibraryFolders from '../models/LibraryFolders';
import { getSteamInstallPath } from '../utils/steam';

export const findAllSteamLibraries = async (): Promise<SteamLibrary[]> => {
  const libraries: Map<string, string> = new Map();

  const steamPath = getSteamInstallPath();

  if (!steamPath) {
    return [];
  }

  const steamAppsPath = join(steamPath, 'steamapps');
  const vdfPath = join(steamAppsPath, 'libraryfolders.vdf');

  if (existsSync(vdfPath)) {
    const content = parse<LibraryFolders>(readFileSync(vdfPath).toString());

    Object.keys(content.libraryfolders).forEach((match) => {
      const library = content.libraryfolders[match];
      const folderPath = join(library.path, 'steamapps');

      libraries.set(folderPath, library.label);
    });
  }

  const promises = Array.from(libraries).map(async ([path, label]) => {
    const libraryId = await hashText(path);

    return new SteamLibrary(libraryId, path, label, 'steam');
  });

  return Promise.all(promises);
};
