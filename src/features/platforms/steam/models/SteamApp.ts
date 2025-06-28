import SteamAppDTO from './SteamAppDTO';
import SteamStateFlags from './SteamStateFlags';

class SteamApp {
  public readonly appId!: number;

  public readonly installPath!: string;

  public readonly libraryId!: string;

  public readonly name!: string;

  public stateFlags: number;

  constructor(data: { appId: number; name: string; installPath: string; stateFlags: number; libraryId: string }) {
    Object.assign(this, data);
  }

  public get image(): string {
    return `https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/${this.appId}/header.jpg`;
  }

  public isInstalled(): boolean {
    return (this.stateFlags & SteamStateFlags.FULLY_INSTALLED) !== 0;
  }

  public isRunning(): boolean {
    return (this.stateFlags & SteamStateFlags.APP_RUNNING) !== 0;
  }

  public isDownloading(): boolean {
    return (this.stateFlags & SteamStateFlags.DOWNLOADING) !== 0;
  }

  public isValidating(): boolean {
    return (this.stateFlags & SteamStateFlags.VALIDATING) !== 0;
  }

  public toDTO(): SteamAppDTO {
    return {
      appId: this.appId,
      name: this.name,
      installPath: this.installPath,
      stateFlags: this.stateFlags,
      libraryId: this.libraryId,
    };
  }

  public static fromDTO(dto: SteamAppDTO): SteamApp {
    return new SteamApp(dto);
  }
}

export default SteamApp;
