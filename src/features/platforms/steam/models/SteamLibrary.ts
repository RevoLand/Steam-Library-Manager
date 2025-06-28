import LibraryType from 'src/core/models/LibraryType';
import SteamLibraryDTO from 'src/features/platforms/steam/models/SteamLibraryDTO';
import SteamApp from './SteamApp';

class SteamLibrary {
  public readonly id: string;

  public readonly path: string;

  public readonly type: LibraryType;

  public apps: SteamApp[] = [];

  constructor(id: string, path: string, type: LibraryType = 'steam') {
    this.id = id;
    this.path = path;
    this.type = type;
  }

  get appCount(): number {
    return this.apps.length;
  }

  public setApps(apps: SteamApp[]) {
    this.apps = apps;

    return this;
  }

  public toDTO(): SteamLibraryDTO {
    return {
      id: this.id,
      path: this.path,
      type: this.type,
      apps: this.apps.map((app) => app.toDTO()),
    };
  }

  static fromDTO(dto: SteamLibraryDTO): SteamLibrary {
    const lib = new SteamLibrary(dto.id, dto.path, dto.type);

    lib.setApps(dto.apps.map(SteamApp.fromDTO));

    return lib;
  }
}

export default SteamLibrary;
