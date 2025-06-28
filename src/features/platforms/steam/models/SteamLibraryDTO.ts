import LibraryType from 'src/core/models/LibraryType';
import SteamAppDTO from 'src/features/platforms/steam/models/SteamAppDTO';

interface SteamLibraryDTO {
  apps: SteamAppDTO[];
  id: string;
  path: string;
  label: string;
  type: LibraryType;
}

export default SteamLibraryDTO;
