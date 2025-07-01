import BaseTask from '../models/BaseTask';
import DeleteTask from '../models/DeleteTask';
import TaskType from '../models/TaskType';
import TransferTask from '../models/TransferTask';

export function isTransferTask(task: BaseTask): task is TransferTask {
  return task.type === TaskType.TRANSFER;
}

export function isDeleteTask(task: BaseTask): task is DeleteTask {
  return task.type === TaskType.DELETE;
}
