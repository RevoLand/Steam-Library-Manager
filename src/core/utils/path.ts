import fs from 'node:fs';
import path from 'node:path';

export const getRealPath = (p: string) => {
  try {
    return fs.realpathSync.native(p).toLowerCase();
  } catch {
    return path.resolve(p).toLowerCase();
  }
};

export const isNestedOrSame = (newPath: string, existingPath: string) => {
  const a = getRealPath(newPath);
  const b = getRealPath(existingPath);

  return a === b || a.startsWith(b + path.sep);
};

export const isConflictingPath = (newPath: string, allPaths: string[]) => {
  return allPaths.some((existing) => isNestedOrSame(newPath, existing));
};
