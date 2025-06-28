import { TransferPattern } from 'src/core/models/FilePattern';
import SteamApp from '../steam/models/SteamApp';
import SteamLibrary from '../steam/models/SteamLibrary';

interface AppFilesFinder {
  canHandle(library: SteamLibrary): boolean;
  generatePatterns(app: SteamApp, library: SteamLibrary): TransferPattern[];
}

export default AppFilesFinder;
