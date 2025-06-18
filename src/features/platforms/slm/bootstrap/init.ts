import { appPopulator } from 'src/features/apps/services/appPopulator';
import { LibraryDeserializer } from 'src/features/libraries/services/LibraryDeserializer';
import { LibraryLocator } from 'src/features/libraries/services/LibraryLocator';
import { SteamLibrary } from '../../steam/models/SteamLibrary';
import { populateSteamApps } from '../../steam/services/populateSteamApps';
import { findAllSLMLibraries } from '../services/libraryFinder';

export const register = async () => {
  LibraryLocator.register('slm', findAllSLMLibraries);
  LibraryDeserializer.register('slm', SteamLibrary.fromDTO);
  appPopulator.register('slm', populateSteamApps);
};
