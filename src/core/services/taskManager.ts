import debounce from 'lodash/debounce';
import throttle from 'lodash/throttle';
import { nanoid } from 'nanoid';
import { EventEmitter } from 'node:events';
import { appMover } from 'src/features/apps/services/appMover';
import TransferMethod from '../models/TransferMethod';
import TransferTask from '../models/TransferTask';
import TransferAbortError from '../models/errors/TransferAbortError';
import { DebouncedFunc } from '../types/utils';

export type TaskManagerStatus = 'idle' | 'running' | 'processing' | 'paused' | 'aborted';

class TaskManager extends EventEmitter {
  private isActive = false;

  private isProcessing = false;

  private currentAbortFlag = false;

  private currentTask: TransferTask = null;

  private tasks: TransferTask[] = [];

  private taskDebouncers = new Map<string, DebouncedFunc<(task: TransferTask) => void>>();

  private throttledTaskEmitters = new Map<string, DebouncedFunc<(task: TransferTask) => void>>();

  private readonly emitThrottleDuration = 200;

  start() {
    this.isActive = true;
    this.emitTaskManagerStatusUpdate('running');
    this.processNext();

    if (this.currentTask?.status === 'paused') {
      this.currentTask.status = 'in-progress';

      this.emitTaskUpdateImmediate(this.currentTask);
    }
  }

  pause() {
    this.isActive = false;
    this.emitTaskManagerStatusUpdate('paused');

    if (this.currentTask) {
      this.currentTask.status = 'paused';

      this.emitTaskUpdateImmediate(this.currentTask);
    }
  }

  abort(options?: { includePending?: boolean }) {
    if (this.currentTask) {
      this.currentAbortFlag = true;

      this.currentTask.status = 'aborted';
      this.emitTaskUpdateImmediate(this.currentTask);

      this.emitTaskManagerStatusUpdate('aborted');
    }

    if (options?.includePending) {
      for (const task of this.tasks) {
        if (task.status === 'pending') {
          task.status = 'aborted';
          this.emitTaskUpdateImmediate(task);
        }
      }
    }
  }

  add(task: TransferTask): TransferTask {
    task.id ??= nanoid();
    task.status ??= 'pending';
    task.createdAt ??= new Date();
    task.transferLog ??= [];
    task.method ??= TransferMethod.system;

    this.tasks.push(task);

    this.emitTasksUpdate();

    if (this.isActive && !this.isProcessing) {
      this.processNext();
    }

    return task;
  }

  private async processNext() {
    if (!this.isActive || this.isProcessing) {
      return;
    }

    const task = this.tasks.find((t) => t.status === 'pending');

    if (!task) {
      return;
    }

    this.isProcessing = true;
    this.emitTaskManagerStatusUpdate('processing');

    try {
      await this.run(task.id);
    } finally {
      this.isProcessing = false;
      this.emitTaskManagerStatusUpdate('running');
      this.processNext();
    }
  }

  remove(taskId: string) {
    this.tasks = this.tasks.filter((task) => task.id !== taskId);

    this.emitTasksUpdate();
  }

  clearCompleted() {
    this.tasks = this.tasks.filter((task) => task.status !== 'done');

    this.emitTasksUpdate();
  }

  getAll(): TransferTask[] {
    return this.tasks;
  }

  emitTaskUpdate(task: TransferTask) {
    this.emit('task-update', task);
  }

  emitTaskUpdateImmediate(task: TransferTask) {
    this.cleanupDebounce(task.id);
    this.cleanupThrottle(task.id);
    this.emitTaskUpdate(task);
  }

  private cleanupDebounce(taskId: string) {
    this.taskDebouncers.get(taskId)?.cancel();
    this.taskDebouncers.delete(taskId);
  }

  private cleanupThrottle(taskId: string) {
    this.throttledTaskEmitters.get(taskId)?.cancel();
    this.throttledTaskEmitters.delete(taskId);
  }

  private emitTaskUpdateDebounced(task: TransferTask) {
    let debounced = this.taskDebouncers.get(task.id);

    if (!debounced) {
      debounced = debounce((latestTask: TransferTask) => {
        this.emitTaskUpdate(latestTask);
        this.taskDebouncers.delete(task.id);
      }, 100);

      this.taskDebouncers.set(task.id, debounced);
    }

    debounced(task);
  }

  private emitTaskUpdateThrottled(task: TransferTask) {
    let throttled = this.throttledTaskEmitters.get(task.id);

    if (!throttled) {
      throttled = throttle(
        (latestTask: TransferTask) => {
          this.emitTaskUpdate(latestTask);
        },
        this.emitThrottleDuration,
        { leading: true, trailing: true }
      );

      this.throttledTaskEmitters.set(task.id, throttled);
    }

    throttled(task);
  }

  emitTasksUpdate() {
    this.emit('tasks-update', this.tasks);
  }

  emitTaskManagerStatusUpdate(status: TaskManagerStatus) {
    this.emit('taskmanager-status-update', status);
  }

  getStatus(): TaskManagerStatus {
    if (this.currentAbortFlag) {
      return 'aborted';
    }
    if (!this.isActive) {
      return 'paused';
    }
    if (this.isProcessing) {
      return 'processing';
    }

    return 'running';
  }

  async run(taskId: string): Promise<void> {
    const task = this.tasks.find((t) => t.id === taskId);

    if (!task || task.status !== 'pending') {
      return;
    }

    this.currentAbortFlag = false;
    this.currentTask = task;

    task.transferredBytes = 0;
    task.skippedBytes = 0;
    task.errorCount = 0;
    task.verifiedCount = 0;
    task.status = 'in-progress';

    this.emitTaskUpdateImmediate(task);

    try {
      task.files = appMover.prepareFiles(task.app, task.sourceLibrary);
      task.totalBytes = task.files.reduce((total, file) => total + file.size, 0);

      await appMover.transfer(task.files, task.targetLibrary.path, {
        mode: task.mode,
        method: task.method,
        onProgress: (stat) => {
          task.transferLog.push(stat);

          if (stat.skipped) {
            task.skippedBytes += stat.sizeBytes;
          } else {
            task.transferredBytes += stat.sizeBytes;
          }

          if (stat.error) {
            task.errorCount += 1;
          }

          if (stat.verified) {
            task.verifiedCount += 1;
          }

          this.emitTaskUpdateThrottled(task);
          this.emitTaskUpdateDebounced(task);
        },
        abortSignal: () => this.currentAbortFlag,
        isPaused: () => !this.isActive,
      });

      task.status = 'done';
      this.emitTaskUpdateImmediate(task);
    } catch (e) {
      if (e instanceof TransferAbortError) {
        task.status = 'aborted';
      } else {
        console.error(`[Transfer Error] ${e}`);
        task.status = 'error';
      }

      this.emitTaskUpdateImmediate(task);
    } finally {
      this.currentTask = null;

      if (!this.currentAbortFlag) {
        this.processNext();
      }
    }
  }
}

export default new TaskManager();
