interface SteamAppDTO {
  appId: number;
  installPath: string;
  libraryId: string;
  name: string;
  stateFlags: number;
  sizeOnDisk: number;
}

export default SteamAppDTO;
