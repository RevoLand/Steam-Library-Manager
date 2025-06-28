import FileTransferStat from './FileTransferStat';
import TransferMode from './TransferMode';
import TransferStrategy from './TransferStrategy';

type TransferOptions = {
  mode: TransferMode;
  method?: TransferStrategy;
  concurrency?: number;
  abortSignal?: () => boolean;
  onProgress?: (stat: FileTransferStat) => void;
};

export default TransferOptions;
