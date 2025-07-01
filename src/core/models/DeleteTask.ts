import BaseTask from './BaseTask';
import DeleteResult from './DeleteResult';
import TaskType from './TaskType';

interface DeleteTask extends BaseTask {
  type: TaskType.DELETE;
  deletedFiles: DeleteResult[];
}

export default DeleteTask;
