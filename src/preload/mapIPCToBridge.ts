import { ipcRenderer } from 'electron';
import IPCInvokeChannels from 'src/core/models/IPCInvokeChannels';
import { IPCInvokeHandlers } from 'src/core/models/IPCInvoker';

export const mapIPCToBridge = (): IPCInvokeHandlers => {
  const api = {} as Partial<IPCInvokeHandlers>;

  for (const channel of IPCInvokeChannels) {
    api[channel] = ((...args: any[]) => ipcRenderer.invoke(channel, ...args)) as any;
  }

  return api as IPCInvokeHandlers;
};
