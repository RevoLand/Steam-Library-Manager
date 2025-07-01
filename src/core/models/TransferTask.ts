import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import BaseTask from './BaseTask';
import { FileToTransfer } from './FilePattern';
import FileTransferStat from './FileTransferStat';
import TaskType from './TaskType';
import TransferMethod from './TransferMethod';
import TransferMode from './TransferMode';

interface TransferTask extends BaseTask {
  type: TaskType.TRANSFER;
  targetLibrary: SteamLibrary;
  files?: FileToTransfer[];
  transferLog?: FileTransferStat[];
  mode: TransferMode;
  method?: TransferMethod;
  totalBytes?: number;
  transferredBytes?: number;
  skippedBytes?: number;
  errorCount?: number;
  verifiedCount?: number;
}

export default TransferTask;
