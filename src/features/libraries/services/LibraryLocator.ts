import { SteamLibrary } from 'src/features/platforms/steam/models/SteamLibrary';

type DiscoveryFn = () => Promise<SteamLibrary[]>;

const registry = new Map<string, DiscoveryFn>();

export const LibraryLocator = {
  register(type: string, fn: DiscoveryFn) {
    registry.set(type, fn);
  },

  async findAll(): Promise<SteamLibrary[]> {
    const results = await Promise.all(Array.from(registry.values()).map((fn) => fn()));

    return results.flat();
  },

  async findByType(type: string): Promise<SteamLibrary[]> {
    const fn = registry.get(type);

    if (!fn) {
      throw new Error(`No finder registered for type: ${type}`);
    }

    return fn();
  },
};
