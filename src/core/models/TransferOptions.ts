import { FileTransferStat } from './FileTransferStat';
import { TransferMode } from './TransferMode';

export enum TransferStrategy {
  system = 'system',
  stream = 'stream',
}

export type TransferOptions = {
  mode: TransferMode;
  method?: TransferStrategy;
  concurrency?: number;
  abortSignal?: () => boolean;
  onProgress?: (stat: FileTransferStat) => void;
};
