import { appPopulator } from 'src/features/apps/services/appPopulator';
import { LibraryDeserializer } from 'src/features/libraries/services/LibraryDeserializer';
import { LibraryLocator } from 'src/features/libraries/services/LibraryLocator';
import { SteamLibrary } from '../models/SteamLibrary';
import { populateSteamApps } from '../services/populateSteamApps';
import { findAllSteamLibraries } from '../services/libraryFinder';

export const register = async () => {
  LibraryLocator.register('steam', findAllSteamLibraries);
  LibraryDeserializer.register('steam', SteamLibrary.fromDTO);
  appPopulator.register('steam', populateSteamApps);
};
