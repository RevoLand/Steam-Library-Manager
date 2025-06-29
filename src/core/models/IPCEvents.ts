import SteamLibraryDTO from 'src/features/platforms/steam/models/SteamLibraryDTO';
import { TaskManagerStatus } from '../services/taskManager';
import TransferTask from './TransferTask';

export const IPCEventDefinitions = {
  'task-update': {} as TransferTask,
  'tasks-update': {} as TransferTask[],
  'taskmanager-status-update': {} as TaskManagerStatus,
  'update-library': {} as SteamLibraryDTO,
  'update-libraries': {} as SteamLibraryDTO[],
};

export type IPCEventChannel = keyof typeof IPCEventDefinitions;

export type IPCEventPayloads = {
  [K in IPCEventChannel]: (typeof IPCEventDefinitions)[K];
};
