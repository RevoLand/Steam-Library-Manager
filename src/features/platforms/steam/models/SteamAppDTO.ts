interface SteamAppDTO {
  appId: number;
  installPath: string;
  libraryId: string;
  name: string;
  stateFlags: number;
}

export default SteamAppDTO;
