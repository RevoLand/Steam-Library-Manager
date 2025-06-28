import { execSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { homedir, platform } from 'node:os';
import { join } from 'node:path';

export const getSteamInstallPath = (): string | null => {
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
