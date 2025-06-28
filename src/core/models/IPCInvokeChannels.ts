import { IPCInvokeChannel } from './IPCInvoker';

const IPCInvokeChannels = [
  'get-libraries',
  'create-library',
  'get-tasks',
  'get-task-manager-status',
  'get-profiles',
  'get-profile',
  'get-active-profile',
  'transfer-task',
  'remove-task',
  'clear-completed-tasks',
  'start-task-manager',
  'pause-task-manager',
  'abort-task',
  'select-directory',
] as const as readonly IPCInvokeChannel[];

export default IPCInvokeChannels;
