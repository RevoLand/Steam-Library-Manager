import fs from 'node:fs';
import path from 'node:path';
import pLimit from 'p-limit';
import { TransferAbortError } from 'src/core/models/errors/TransferAbortError';
import { FileToTransfer } from 'src/core/models/FilePattern';
import { FileTransferStat } from 'src/core/models/FileTransferStat';
import { TransferMode } from 'src/core/models/TransferMode';
import { TransferOptions } from 'src/core/models/TransferOptions';
import { hasMode } from 'src/core/utils/bitwise';
import { hashFile } from 'src/core/utils/hash';
import { appFilesFinders } from 'src/features/platforms';
import { SteamApp } from 'src/features/platforms/steam/models/SteamApp';
import { SteamLibrary } from 'src/features/platforms/steam/models/SteamLibrary';
import { resolveTransferPatterns } from './transferResolver';

class AppMover {
  prepareFiles(app: SteamApp, library: SteamLibrary): FileToTransfer[] {
    const finder = appFilesFinders.find((f) => f.canHandle(library));

    if (!finder) {
      throw new Error(`No AppFilesFinder found for platform: ${library.type}`);
    }

    const patterns = finder.generatePatterns(app, library);

    return resolveTransferPatterns(patterns, library.path);
  }

  sortFiles(files: FileToTransfer[]): FileToTransfer[] {
    return [...files].sort((a, b) => {
      const dirCompare = a.relativePath.localeCompare(b.relativePath);

      if (dirCompare !== 0) {
        return dirCompare;
      }

      return b.size - a.size;
    });
  }

  async transferSingleFile(
    file: FileToTransfer,
    targetLibraryPath: string,
    options: TransferOptions
  ): Promise<FileTransferStat> {
    const startTime = performance.now();
    const stat: FileTransferStat = {
      file: file.relativePath,
      sizeBytes: fs.statSync(file.source).size,
      durationMs: 0,
      skipped: false,
      mode: hasMode(options.mode, TransferMode.MOVE) ? 'move' : 'copy',
      startTime,
    };

    const targetPath = path.join(targetLibraryPath, file.relativePath);
    const tempPath = targetPath + '.slm_temp';

    try {
      if (options.abortSignal?.()) {
        throw new TransferAbortError();
      }

      if (hasMode(options.mode, TransferMode.SKIP_EXISTING) && fs.existsSync(targetPath)) {
        stat.skipped = true;
        stat.endTime = performance.now();
        stat.durationMs = Math.round(stat.endTime - startTime);

        return stat;
      }

      if (hasMode(options.mode, TransferMode.DRY_RUN)) {
        console.log(`[DRY_RUN] Would transfer: ${file.source} → ${targetPath}`);

        stat.endTime = performance.now();
        stat.durationMs = Math.round(stat.endTime - startTime);

        return stat;
      }

      await fs.promises.mkdir(path.dirname(targetPath), { recursive: true });

      let usedTemp = false;

      if (hasMode(options.mode, TransferMode.MOVE)) {
        try {
          await fs.promises.rename(file.source, tempPath);
          usedTemp = true;
        } catch (e: any) {
          if (e.code === 'EXDEV') {
            await fs.promises.copyFile(file.source, tempPath, fs.constants.COPYFILE_FICLONE);
            await fs.promises.unlink(file.source);
            usedTemp = true;
          } else {
            throw e;
          }
        }
      } else {
        await fs.promises.copyFile(file.source, tempPath, fs.constants.COPYFILE_FICLONE);
        usedTemp = true;
      }

      if (usedTemp) {
        await fs.promises.rename(tempPath, targetPath);
      }

      stat.tempUsed = usedTemp;

      if (hasMode(options.mode, TransferMode.VERIFY)) {
        const [srcHash, destHash] = await Promise.all([hashFile(file.source), hashFile(targetPath)]);

        stat.verified = srcHash === destHash;

        if (!stat.verified) {
          throw new Error(`Hash mismatch: ${file.source}`);
        }
      }
    } catch (e: any) {
      stat.error = e.message || 'Unknown error';
      throw e;
    } finally {
      const endTime = performance.now();

      stat.endTime = endTime;
      stat.durationMs = Math.round(endTime - startTime);
      stat.throughput = stat.durationMs > 0 ? Math.round(stat.sizeBytes / 1024 / (stat.durationMs / 1000)) : undefined;
    }

    return stat;
  }

  async transfer(
    files: FileToTransfer[],
    targetLibraryPath: string,
    options: TransferOptions
  ): Promise<FileTransferStat[]> {
    const sortedFiles = this.sortFiles(files);
    const limit = pLimit(Math.max(1, options.concurrency || 1));
    const stats: FileTransferStat[] = [];

    await Promise.all(
      sortedFiles.map((file) =>
        limit(async () => {
          const stat = await this.transferSingleFile(file, targetLibraryPath, options);

          stats.push(stat);
          options.onProgress?.(stat);
        })
      )
    );

    return stats;
  }
}

export const appMover = new AppMover();
