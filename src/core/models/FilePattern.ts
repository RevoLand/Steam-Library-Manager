export interface DirectoryPattern {
  type: 'directory';
  path: string;
  pattern: string;
  recursive: boolean;
}

export interface FileEntry {
  type: 'file';
  path: string;
}

export type TransferPattern = DirectoryPattern | FileEntry;

export interface FileToTransfer {
  source: string;
  relativePath: string;
  size: number;
}
