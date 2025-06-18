import { LibraryDeserializer } from 'src/features/libraries/services/LibraryDeserializer';
import { SteamLibrary } from '../models/SteamLibrary';

export const register = async () => {
  LibraryDeserializer.register('steam', SteamLibrary.fromDTO);
};
