import { LibraryDeserializer } from 'src/features/libraries/services/LibraryDeserializer';
import { SteamLibrary } from '../../steam/models/SteamLibrary';

export const register = async () => {
  LibraryDeserializer.register('slm', SteamLibrary.fromDTO);
};
