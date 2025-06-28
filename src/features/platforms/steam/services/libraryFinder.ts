import { execSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';
import { homedir, platform } from 'node:os';
import { join } from 'node:path';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import { hashText } from 'src/core/utils/hash';

const getSteamInstallPath = (): string | null => {
  const currentPlatform = platform();

  if (currentPlatform === 'win32') {
    try {
      const stdout = execSync('reg query "HKCU\\Software\\Valve\\Steam" /v SteamPath', { encoding: 'utf-8' });
      const match = stdout.match(/SteamPath\s+REG_SZ\s+(.+)/);

      return match ? match[1].trim().toLowerCase() : null;
    } catch {
      return null;
    }
  }

  if (currentPlatform === 'darwin') {
    const steamPath = join(homedir(), 'Library', 'Application Support', 'Steam');

    return existsSync(steamPath) ? steamPath : null;
  }

  // TODO: Linux için de eklenebilir
  return null;
};

export const findAllSteamLibraries = async (): Promise<SteamLibrary[]> => {
  const libraries: Set<string> = new Set();

  const steamPath = getSteamInstallPath();

  if (!steamPath) {
    return [];
  }

  const steamapps = join(steamPath, 'steamapps');
  const vdfPath = join(steamapps, 'libraryfolders.vdf');

  libraries.add(steamapps.toLowerCase());

  if (existsSync(vdfPath)) {
    const content = readFileSync(vdfPath, 'utf-8');

    const matches = [...content.matchAll(/"path"\s+"(.+?)"/g)];

    matches.forEach((match) => {
      const folderPath = join(match[1], 'steamapps').toLowerCase();

      libraries.add(folderPath);
    });
  }

  const promises = Array.from(libraries).map(async (path) => {
    const id = await hashText(path);

    return new SteamLibrary(id, path);
  });

  return Promise.all(promises);
};
