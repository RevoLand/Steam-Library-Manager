import { libraryDeserializer } from 'src/features/libraries/services/libraryDeserializer';
import SteamLibrary from '../models/SteamLibrary';

export const register = async () => {
  libraryDeserializer.register('steam', SteamLibrary.fromDTO);
};
