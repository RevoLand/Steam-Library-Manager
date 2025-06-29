import { EventEmitter } from 'node:events';
import LibraryType from 'src/core/models/LibraryType';
import { isConflictingPath } from 'src/core/utils/path';
import SLMLibraryCreator from 'src/features/platforms/slm/services/SLMLibraryCreator';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import SteamLibraryCreator from 'src/features/platforms/steam/services/SteamLibraryCreator';
import { appPopulator } from '../../apps/services/appPopulator';
import LibraryCreator from '../models/LibraryCreator';
import { libraryLocator } from './libraryLocator';

class LibraryManager extends EventEmitter {
  private libraries: SteamLibrary[] = [];

  private libraryLoader: Promise<SteamLibrary[]> | null = null;

  private libraryAppLoader: Promise<void[]> | null = null;

  private creators: Record<LibraryType, LibraryCreator> = {
    steam: new SteamLibraryCreator(),
    slm: new SLMLibraryCreator(),
  };

  public async loadLibraries(): Promise<void> {
    if (!this.libraryLoader) {
      this.libraryLoader = libraryLocator.findAll();
    }

    try {
      this.libraries = await this.libraryLoader;
    } finally {
      this.libraryLoader = null;
      this.emitLibraryUpdates();
    }
  }

  public get isLoading(): boolean {
    return !!this.libraryLoader;
  }

  public getLibraries(): SteamLibrary[] {
    return this.libraries;
  }

  public getLibrary(id: string): SteamLibrary | undefined {
    return this.libraries.find((lib) => lib.id === id);
  }

  emitLibraryUpdate(library: SteamLibrary): void {
    this.emit('update-library', library.toDTO());
  }

  emitLibraryUpdates(): void {
    this.emit(
      'update-libraries',
      this.libraries.map((lib) => lib.toDTO())
    );
  }

  public async createLibrary(path: string, label: string, type: LibraryType): Promise<SteamLibrary> {
    const allLibraryPaths = this.getLibraries().map((lib) => lib.path);

    if (isConflictingPath(path, allLibraryPaths)) {
      throw new Error('Mevcut bir kütüphanenin alt klasöründe yeni kütüphane oluşturulamaz.');
    }

    const library = await this.creators[type].create(path, label);

    if (!this.libraries.find((lib) => lib.id === library.id)) {
      await appPopulator.populate(library);
      this.libraries.push(library);
    }

    this.emitLibraryUpdate(library);

    return library;
  }

  public async populateApps(libraryId: string): Promise<void> {
    const lib = this.getLibrary(libraryId);

    if (!lib) {
      throw new Error(`Library not found: ${libraryId}`);
    }

    if (!lib.apps.length) {
      await appPopulator.populate(lib);
    }
  }

  public async populateAllApps(): Promise<void> {
    if (!this.libraryAppLoader) {
      this.libraryAppLoader = Promise.all(this.libraries.map((lib) => appPopulator.populate(lib)));
    }

    try {
      await this.libraryAppLoader;
    } finally {
      this.libraryAppLoader = null;
    }
  }

  public clear() {
    this.libraries = [];

    this.emitLibraryUpdates();
  }
}

export const libraryManager = new LibraryManager();
