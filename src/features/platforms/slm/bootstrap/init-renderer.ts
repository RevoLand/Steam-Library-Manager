import { libraryDeserializer } from 'src/features/libraries/services/libraryDeserializer';
import SteamLibrary from '../../steam/models/SteamLibrary';

export const register = async () => {
  libraryDeserializer.register('slm', SteamLibrary.fromDTO);
};
