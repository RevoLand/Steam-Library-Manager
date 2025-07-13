import FileTransferStat from './FileTransferStat';
import TransferFlags from './TransferFlags';
import TransferMethod from './TransferMethod';
import TransferOperation from './TransferOperation';

type TransferOptions = {
  operation: TransferOperation;
  flags?: TransferFlags;
  method?: TransferMethod;
  concurrency?: number;
  abortSignal?: () => boolean;
  onProgress?: (stat: FileTransferStat) => void;
  isPaused?: () => boolean;
};

export default TransferOptions;
