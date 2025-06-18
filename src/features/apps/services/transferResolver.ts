import { sync } from 'fast-glob';
import fs from 'node:fs';
import path from 'node:path';
import { FileToTransfer, TransferPattern } from 'src/core/models/FilePattern';

export function resolveTransferPatterns(patterns: TransferPattern[], libraryRoot: string): FileToTransfer[] {
  const result: FileToTransfer[] = [];

  for (const pattern of patterns) {
    if (pattern.type === 'file') {
      if (fs.existsSync(pattern.path)) {
        const rel = path.relative(libraryRoot, pattern.path);
        const stat = fs.statSync(pattern.path);

        result.push({
          source: pattern.path,
          relativePath: rel,
          size: stat.size,
        });
      }
    }

    if (pattern.type === 'directory') {
      const entries = sync(pattern.recursive ? '**/*' : pattern.pattern, {
        cwd: pattern.path,
        onlyFiles: true,
        dot: true,
        absolute: true,
      });

      for (const entry of entries) {
        const rel = path.relative(libraryRoot, entry);
        const stat = fs.statSync(entry);

        result.push({
          source: entry,
          relativePath: rel,
          size: stat.size,
        });
      }
    }
  }

  return result;
}
