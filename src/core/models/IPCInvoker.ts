import ProfileManager from 'src/core/config/profile/ProfileManager';
import TransferMode from 'src/core/models/TransferMode';
import { libraryManager } from 'src/features/libraries/services/libraryManager';
import taskManager from '../services/taskManager';
import { isTransferTask } from '../utils/task';
import LibraryType from './LibraryType';
import TaskType from './TaskType';

export const IPCInvokeDefinitions = {
  'get-libraries': {
    handler: async () => {
      return libraryManager.getLibraries().map((library) => library.toDTO());
    },
  },
  'create-library': {
    handler: async (path: string, label: string, type: LibraryType) => {
      return libraryManager.createLibrary(path, label, type);
    },
  },
  'get-tasks': {
    handler: async () => {
      return taskManager.getAll();
    },
  },
  'get-task-manager-status': {
    handler: async () => {
      return taskManager.getStatus();
    },
  },
  'get-profiles': {
    handler: async () => {
      return ProfileManager.getProfiles();
    },
  },
  'get-profile': {
    handler: async (profileId: string) => {
      return ProfileManager.getProfile(profileId);
    },
  },
  'get-active-profile': {
    handler: async () => {
      return ProfileManager.getActiveProfile();
    },
  },
  'transfer-task': {
    handler: async (appId: number, libraryId: string, targetLibraryId: string, transferMode: TransferMode) => {
      const sourceLibrary = libraryManager.getLibrary(libraryId);
      const targetLibrary = libraryManager.getLibrary(targetLibraryId);

      if (!sourceLibrary) {
        throw new Error(`Library not found: ${libraryId}`);
      }
      if (!targetLibrary) {
        throw new Error(`Library not found: ${targetLibraryId}`);
      }

      const app = sourceLibrary.apps.find((a) => a.appId === appId);

      if (!app) {
        throw new Error(`App not found: ${appId}`);
      }

      const existingTask = taskManager
        .getAll()
        .find(
          (_task) =>
            isTransferTask(_task) &&
            _task.app.appId === appId &&
            _task.sourceLibrary.id === sourceLibrary.id &&
            _task.targetLibrary.id === targetLibrary.id &&
            ['pending', 'error'].includes(_task.status)
        );

      if (existingTask) {
        taskManager.run(existingTask.id);

        return existingTask;
      }

      return taskManager.add({
        app,
        type: TaskType.TRANSFER,
        sourceLibrary,
        targetLibrary,
        mode: transferMode,
      });
    },
  },
  'remove-task': {
    handler: async (taskId: string) => {
      taskManager.remove(taskId);
    },
  },
  'clear-completed-tasks': {
    handler: async () => {
      taskManager.clearCompleted();
    },
  },
  'start-task-manager': {
    handler: async () => {
      taskManager.start();
    },
  },
  'pause-task-manager': {
    handler: async () => {
      taskManager.pause();
    },
  },
  'abort-task': {
    handler: async () => {
      taskManager.abort();
    },
  },
  'select-directory': {
    handler: async () => {
      const { dialog } = await import('electron');
      const result = await dialog.showOpenDialog({ properties: ['openDirectory'] });

      return result.canceled || result.filePaths.length === 0 ? null : result.filePaths[0];
    },
  },
} as const;

export type IPCInvokeChannel = keyof typeof IPCInvokeDefinitions;

export type IPCInvokeHandlers = {
  [K in IPCInvokeChannel]: (typeof IPCInvokeDefinitions)[K]['handler'];
};
