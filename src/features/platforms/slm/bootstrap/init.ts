import { appPopulator } from 'src/features/apps/services/appPopulator';
import { libraryDeserializer } from 'src/features/libraries/services/libraryDeserializer';
import { libraryLocator } from 'src/features/libraries/services/libraryLocator';
import SteamLibrary from '../../steam/models/SteamLibrary';
import { populateSteamApps } from '../../steam/services/populateSteamApps';
import { findAllSLMLibraries } from '../services/libraryFinder';

export const register = async () => {
  libraryLocator.register('slm', findAllSLMLibraries);
  libraryDeserializer.register('slm', SteamLibrary.fromDTO);
  appPopulator.register('slm', populateSteamApps);
};
