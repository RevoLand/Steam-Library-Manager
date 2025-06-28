type LibraryFolderEntry = {
  path: string;
  label: string;
  contentid: string;
  apps: Record<string, number>;
  time_last_update_verified: string;
  totalsize: string;
  update_clean_bytes_tally: string;
};

type LibraryFolders = {
  libraryfolders: Record<string, LibraryFolderEntry>;
};

export default LibraryFolders;
