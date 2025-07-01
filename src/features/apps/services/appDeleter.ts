import fs from 'node:fs/promises';
import pLimit from 'p-limit';
import { rimraf } from 'rimraf';
import DeleteResult from 'src/core/models/DeleteResult';
import TransferAbortError from 'src/core/models/errors/TransferAbortError';
import { waitWhile } from 'src/core/utils/async';
import { appFilesFinders } from 'src/features/platforms';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

interface DeleteOptions {
  concurrency?: number;
  onProgress?: (file: DeleteResult) => void;
  isPaused?: () => boolean;
  abortSignal?: () => boolean;
}

class AppDeleter {
  prepareFiles(app: SteamApp, library: SteamLibrary): string[] {
    const finder = appFilesFinders.find((f) => f.canHandle(library));

    if (!finder) {
      throw new Error(`No AppFilesFinder found for platform: ${library.type}`);
    }

    const patterns = finder.generateTransferPatterns(app, library);

    return patterns.map((pattern) => pattern.path);
  }

  async delete(app: SteamApp, library: SteamLibrary, options: DeleteOptions = {}): Promise<DeleteResult[]> {
    const files = this.prepareFiles(app, library);
    const results: DeleteResult[] = [];
    const limit = pLimit(Math.max(1, options.concurrency || 1));

    await Promise.all(
      files.map((file) =>
        limit(async () => {
          await waitWhile(() => options.isPaused?.());

          if (options.abortSignal?.()) {
            throw new TransferAbortError();
          }

          try {
            const stat = await fs.stat(file);

            if (stat.isDirectory()) {
              await rimraf(file, { maxRetries: 2 });
            } else {
              await fs.unlink(file);
            }

            results.push({ path: file, deleted: true });
            options.onProgress?.({ path: file, deleted: true });
          } catch (e) {
            console.warn(`[Delete] Could not remove ${file}: ${e.message}`);
            results.push({ path: file, deleted: false, error: e.message });
          }
        })
      )
    );

    return results;
  }
}

export const appDeleter = new AppDeleter();
