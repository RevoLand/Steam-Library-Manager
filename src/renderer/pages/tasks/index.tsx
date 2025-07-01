import { useState } from 'react';
import TaskType from 'src/core/models/TaskType';
import TransferTask from 'src/core/models/TransferTask';
import TransferTaskComponent from 'src/renderer/components/TransferTaskComponent';
import useTasks from 'src/renderer/hooks/useTasks';

const TasksPage = () => {
  const { tasks, status, start, pause, abort, clearCompleted } = useTasks();
  const [expandedTaskId, setExpandedTaskId] = useState<string | null>(null);

  return (
    <div className='flex flex-col w-full p-4 overflow-auto'>
      <div className='flex flex-row justify-between'>
        <h2 className='text-2xl font-bold mb-4'>Aktif Görevler</h2>
        <div className='flex flex-row gap-4'>
          <button onClick={() => start()}>Başlat</button>
          <button onClick={() => pause()}>Duraklat</button>
          <button onClick={() => abort()}>İptal et</button>
          <button onClick={() => clearCompleted()}>Tamamlananları Temizle</button>
        </div>
      </div>
      <p className='text-gray-500'>Task Manager Durumu: {status}</p>

      {tasks.length === 0 ? (
        <p className='text-gray-500'>Şu anda herhangi bir görev çalışmıyor.</p>
      ) : (
        <ul className='space-y-4'>
          {tasks.map((task) => {
            if (task.type === TaskType.TRANSFER) {
              return (
                <TransferTaskComponent
                  key={task.id}
                  task={task as TransferTask}
                  isExpanded={expandedTaskId === task.id}
                  setExpandedTaskId={setExpandedTaskId}
                />
              );
            }
          })}
        </ul>
      )}
    </div>
  );
};

export default TasksPage;
