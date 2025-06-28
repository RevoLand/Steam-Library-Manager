import FileTransferStat from '../models/FileTransferStat';

export function formatTransferStat(stat: FileTransferStat): string {
  const base = `${stat.file} (${Math.round(stat.sizeBytes / 1024)} KB)`;

  if (stat.skipped) {
    return `${base} skipped.`;
  }
  if (stat.error) {
    return `${base} failed: ${stat.error}`;
  }
  if (stat.verified === false) {
    return `${base} failed verification.`;
  }

  return `${base} transferred in ${stat.durationMs}ms`;
}
