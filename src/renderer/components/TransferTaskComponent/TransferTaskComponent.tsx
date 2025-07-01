import { Dispatch, SetStateAction } from 'react';
import TransferTask from 'src/core/models/TransferTask';

function formatBytes(bytes: number): string {
  if (bytes === 0) {
    return '0 B';
  }
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

const TransferTaskComponent = ({
  task,
  isExpanded,
  setExpandedTaskId,
}: {
  task: TransferTask;
  isExpanded: boolean;
  setExpandedTaskId: Dispatch<SetStateAction<string>>;
}) => {
  const progress = task.totalBytes ? (task.transferredBytes + task.skippedBytes) / task.totalBytes : undefined;

  const lastFile = task.transferLog?.at(-1)?.file;

  return (
    <li key={task.id} className='p-4 rounded-xl shadow bg-white border border-gray-200'>
      <div className='flex items-center justify-between'>
        <div className='flex flex-col space-y-1'>
          <p className='text-sm font-semibold text-gray-700'>{task.app.name}</p>
          <p className='text-sm text-gray-500'>
            Durum: <span className='font-medium'>{task.status}</span>
          </p>
          <p className='text-sm text-gray-500'>
            Kaynak: <span className='text-gray-600'>{task.sourceLibrary.path}</span>
          </p>
          <p className='text-sm text-gray-500'>
            Hedef: <span className='text-gray-600'>{task.targetLibrary.path}</span>
          </p>
          {task.createdAt && (
            <p className='text-sm text-gray-400'>Oluşturulma: {new Date(task.createdAt).toLocaleString()}</p>
          )}

          {isExpanded && (
            <>
              {lastFile && <p className='text-sm text-gray-400 truncate max-w-xl'>Son dosya: {lastFile}</p>}
              <p className='text-sm text-gray-500'>Toplam Boyut: {formatBytes(task.totalBytes || 0)}</p>
              <p className='text-sm text-gray-500'>Aktarılan: {formatBytes(task.transferredBytes || 0)}</p>
              <p className='text-sm text-gray-500'>Atlanan: {formatBytes(task.skippedBytes || 0)}</p>
              <p className='text-sm text-gray-500'>Doğrulanan: {task.verifiedCount || 0} dosya</p>
              <p className='text-sm text-gray-500'>Hatalı: {task.errorCount || 0} dosya</p>
            </>
          )}

          <button
            className='mt-2 w-max text-sm text-blue-600 hover:underline'
            onClick={() => setExpandedTaskId(isExpanded ? null : task.id || null)}
          >
            {isExpanded ? 'Detayı Gizle' : 'Detayı Göster'}
          </button>
        </div>

        {typeof progress === 'number' && (
          <div className='w-40'>
            <div className='w-full bg-gray-200 h-3 rounded'>
              <div className='h-3 rounded bg-blue-500' style={{ width: `${Math.floor(progress * 100)}%` }}></div>
            </div>
            <p className='text-xs text-right text-gray-500 mt-1'>%{Math.floor(progress * 100)}</p>
            <p className='text-xs text-gray-400 text-right'>
              {formatBytes(task.transferredBytes || 0)} / {formatBytes(task.totalBytes || 0)}
            </p>
          </div>
        )}
      </div>
    </li>
  );
};

export default TransferTaskComponent;
