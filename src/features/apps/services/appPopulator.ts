import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

class AppPopulator {
  private populators: Map<string, (library: SteamLibrary) => Promise<void>> = new Map();

  register(platform: string, fn: (library: SteamLibrary) => Promise<void>) {
    this.populators.set(platform, fn);
  }

  async populate(library: SteamLibrary): Promise<void> {
    const fn = this.populators.get(library.type);

    if (!fn) {
      throw new Error(`No populator registered for platform: ${library.type}`);
    }

    return fn(library);
  }
}

export const appPopulator = new AppPopulator();
