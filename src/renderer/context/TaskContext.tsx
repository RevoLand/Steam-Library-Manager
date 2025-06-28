import { createContext, PropsWithChildren, useCallback, useEffect, useMemo, useState } from 'react';
import TransferMode from 'src/core/models/TransferMode';
import TransferTask from 'src/core/models/TransferTask';
import { TaskManagerStatus } from 'src/core/services/taskManager';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import SteamLibrary from 'src/features/platforms/steam/models/SteamLibrary';

interface TaskContextValue {
  abort: (taskId?: string) => void;
  addTask: (app: SteamApp, targetLibrary: SteamLibrary, transferMode: TransferMode) => Promise<TransferTask>;
  clearCompleted: () => void;
  initialized: boolean;
  pause: () => void;
  removeTask: (taskId: string) => void;
  start: () => void;
  status: TaskManagerStatus;
  tasks: TransferTask[];
}

export const TaskContext = createContext<TaskContextValue>(undefined);

TaskContext.displayName = 'TaskContext';

export default function TaskProvider(props: PropsWithChildren) {
  const [tasks, setTasks] = useState<TransferTask[]>([]);
  const [status, setStatus] = useState<TaskManagerStatus>('idle');
  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    const getMainTasks = () => {
      window.api['get-tasks']()
        .then((mainTasks) => setTasks(mainTasks))
        .then(() => setInitialized(true));
    };

    const getTaskManagerStatus = () => {
      window.api['get-task-manager-status']().then(setStatus);
    };

    const subscribeToSingleTaskUpdates = () => {
      window.events['task-update']((task: TransferTask) => {
        setTasks((prev) => {
          const existing = prev.find((t) => t.id === task.id);

          if (existing) {
            return prev.map((t) => (t.id === task.id ? { ...t, ...task } : t));
          }

          return [...prev, task];
        });
      });
    };

    const subscribeToMainTaskListUpdates = () => {
      window.events['tasks-update']((mainTasks: TransferTask[]) => {
        setTasks(mainTasks);
      });
    };

    const subscribeToTaskManagerStatusUpdates = () => {
      window.events['taskmanager-status-update']((newStatus: TaskManagerStatus) => {
        setStatus(newStatus);
      });
    };

    getMainTasks();
    getTaskManagerStatus();
    subscribeToSingleTaskUpdates();
    subscribeToMainTaskListUpdates();
    subscribeToTaskManagerStatusUpdates();
  }, []);

  const addTask = useCallback(async (app: SteamApp, targetLibrary: SteamLibrary, transferMode: TransferMode) => {
    return window.api['transfer-task'](app.appId, app.libraryId, targetLibrary.id, transferMode);
  }, []);

  const removeTask = useCallback((taskId: string) => {
    window.api['remove-task'](taskId);
  }, []);

  const clearCompleted = useCallback(() => {
    window.api['clear-completed-tasks']();
  }, []);

  const start = useCallback(() => {
    window.api['start-task-manager']();
  }, []);

  const pause = useCallback(() => {
    window.api['pause-task-manager']();
  }, []);

  const abort = useCallback((taskId?: string) => {
    window.api['abort-task'](taskId);
  }, []);

  const taskContextValue = useMemo(
    () => ({
      abort,
      addTask,
      clearCompleted,
      initialized,
      pause,
      removeTask,
      start,
      status,
      tasks,
    }),
    [status, tasks]
  );

  return <TaskContext.Provider value={taskContextValue}>{props.children}</TaskContext.Provider>;
}
