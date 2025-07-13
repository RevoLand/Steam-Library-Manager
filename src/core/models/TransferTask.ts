import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import BaseTask from './BaseTask';
import { FileToTransfer } from './FilePattern';
import FileTransferStat from './FileTransferStat';
import TaskType from './TaskType';
import TransferFlags from './TransferFlags';
import TransferMethod from './TransferMethod';
import TransferOperation from './TransferOperation';

interface TransferTask extends BaseTask {
  type: TaskType.TRANSFER;
  targetLibrary: SteamLibrary;
  files?: FileToTransfer[];
  transferLog?: FileTransferStat[];
  operation: TransferOperation;
  flags?: TransferFlags;
  method?: TransferMethod;
  totalBytes?: number;
  transferredBytes?: number;
  skippedBytes?: number;
  errorCount?: number;
  verifiedCount?: number;
}

export default TransferTask;
