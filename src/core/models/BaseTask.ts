import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';
import TaskStatus from './TaskStatus';
import TaskType from './TaskType';

interface BaseTask {
  id?: string;
  type: TaskType;
  app: SteamApp;
  sourceLibrary: SteamLibrary;
  status?: TaskStatus;
  createdAt?: Date;
}

export default BaseTask;
