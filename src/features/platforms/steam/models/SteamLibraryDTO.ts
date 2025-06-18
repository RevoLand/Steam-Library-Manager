import { SteamAppDTO } from 'src/features/platforms/steam/models/SteamAppDTO';

export interface SteamLibraryDTO {
  apps: SteamAppDTO[];
  id: string;
  path: string;
  type: 'steam' | 'slm';
}
