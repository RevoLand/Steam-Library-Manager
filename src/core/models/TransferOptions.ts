import { FileTransferStat } from './FileTransferStat';
import { TransferMode } from './TransferMode';

export type TransferOptions = {
  mode: TransferMode;
  concurrency?: number;
  abortSignal?: () => boolean;
  onProgress?: (stat: FileTransferStat) => void;
};
