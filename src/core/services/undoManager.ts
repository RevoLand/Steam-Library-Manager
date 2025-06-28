import fs from 'node:fs';
import path from 'node:path';
import TransferTask from 'src/core/models/TransferTask';

class UndoManager {
  async undoTask(task: TransferTask): Promise<void> {
    const reversedLog = [...(task.transferLog || [])].reverse();

    for (const stat of reversedLog) {
      if (stat.skipped || stat.error) {
        continue;
      }

      const sourcePath = path.join(task.sourceLibrary.path, stat.file);
      const targetPath = path.join(task.targetLibrary.path, stat.file);

      try {
        if (stat.mode === 'copy') {
          await fs.promises.unlink(targetPath);
        }

        if (stat.mode === 'move') {
          await fs.promises.mkdir(path.dirname(sourcePath), { recursive: true });
          await fs.promises.rename(targetPath, sourcePath);
        }
      } catch (e) {
        console.warn(`[Undo Error] ${stat.file}: ${e.message}`);
      }
    }
  }
}

export const undoManager = new UndoManager();
