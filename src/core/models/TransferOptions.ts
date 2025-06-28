import FileTransferStat from './FileTransferStat';
import TransferMethod from './TransferMethod';
import TransferMode from './TransferMode';

type TransferOptions = {
  mode: TransferMode;
  method?: TransferMethod;
  concurrency?: number;
  abortSignal?: () => boolean;
  onProgress?: (stat: FileTransferStat) => void;
  isPaused?: () => boolean;
};

export default TransferOptions;
