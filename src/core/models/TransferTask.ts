import { SteamApp } from 'src/features/platforms/steam/models/SteamApp';
import { SteamLibrary } from 'src/features/platforms/steam/models/SteamLibrary';
import { FileToTransfer } from './FilePattern';
import { FileTransferStat } from './FileTransferStat';
import { TransferMode } from './TransferMode';

export interface TransferTask {
  id?: string;
  app: SteamApp;
  sourceLibrary: SteamLibrary;
  targetLibrary: SteamLibrary;
  files?: FileToTransfer[];
  transferLog?: FileTransferStat[];
  mode: TransferMode;
  createdAt?: Date;
  status?: 'pending' | 'in-progress' | 'done' | 'error' | 'aborted';

  totalBytes?: number;
  transferredBytes?: number;
  skippedBytes?: number;
  errorCount?: number;
  verifiedCount?: number;
}
