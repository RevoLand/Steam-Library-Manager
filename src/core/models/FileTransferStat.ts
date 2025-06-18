export interface FileTransferStat {
  file: string;
  sizeBytes: number;
  durationMs: number;
  mode: 'copy' | 'move';
  skipped: boolean;
  verified?: boolean;
  error?: string;

  startTime?: number;
  endTime?: number;
  throughput?: number;
  attempt?: number;
  tempUsed?: boolean;
}
