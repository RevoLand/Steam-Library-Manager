import AppFilesFinder from './interfaces/AppFilesFinder';
import SteamAppFilesFinder from './steam/services/SteamAppFilesFinder';

export const appFilesFinders: AppFilesFinder[] = [new SteamAppFilesFinder()];
