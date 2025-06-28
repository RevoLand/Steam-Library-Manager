import { appPopulator } from 'src/features/apps/services/appPopulator';
import { libraryDeserializer } from 'src/features/libraries/services/libraryDeserializer';
import { libraryLocator } from 'src/features/libraries/services/libraryLocator';
import SteamLibrary from '../models/SteamLibrary';
import { findAllSteamLibraries } from '../services/libraryFinder';
import { populateSteamApps } from '../services/populateSteamApps';

export const register = async () => {
  libraryLocator.register('steam', findAllSteamLibraries);
  libraryDeserializer.register('steam', SteamLibrary.fromDTO);
  appPopulator.register('steam', populateSteamApps);
};
