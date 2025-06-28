import { TaskManagerStatus } from '../services/taskManager';
import TransferTask from './TransferTask';

export const IPCEventDefinitions = {
  'task-update': {} as TransferTask,
  'tasks-update': {} as TransferTask[],
  'taskmanager-status-update': {} as TaskManagerStatus,
};

export type IPCEventChannel = keyof typeof IPCEventDefinitions;

export type IPCEventPayloads = {
  [K in IPCEventChannel]: (typeof IPCEventDefinitions)[K];
};
