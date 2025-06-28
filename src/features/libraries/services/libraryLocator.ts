import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

export type DiscoveryFn = () => Promise<SteamLibrary[]>;

class LibraryLocator {
  private registry = new Map<string, DiscoveryFn>();

  register(type: string, fn: DiscoveryFn): void {
    this.registry.set(type, fn);
  }

  async findAll(): Promise<SteamLibrary[]> {
    const results = await Promise.all([...this.registry.values()].map((fn) => fn()));

    return results.flat();
  }

  async findByType(type: string): Promise<SteamLibrary[]> {
    const fn = this.registry.get(type);

    if (!fn) {
      throw new Error(`No finder registered for type: ${type}`);
    }

    return fn();
  }

  clear(): void {
    this.registry.clear();
  }
}

export const libraryLocator = new LibraryLocator();
