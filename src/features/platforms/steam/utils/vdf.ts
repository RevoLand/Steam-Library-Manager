import { platform } from 'node:os';

export const getCaseInsensitivePath = (obj: any, path: string[]): any => {
  return path.reduce((acc, key) => {
    if (!acc || typeof acc !== 'object') {
      return undefined;
    }

    const realKey = Object.keys(acc).find((k) => k.toLowerCase() === key.toLowerCase());

    return realKey ? acc[realKey] : undefined;
  }, obj);
};

export function escapeVdfPath(path: string): string {
  if (platform() === 'win32') {
    return path.replace(/\\/g, '\\\\');
  }

  return path;
}
