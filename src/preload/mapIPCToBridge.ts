import { ipcRenderer } from 'electron';
import { ipcInvokeChannels } from 'src/core/models/IPCInvokeChannel';
import { IPCInvokeHandlers } from 'src/core/models/IPCTypes';

export const mapIPCToBridge = (): IPCInvokeHandlers => {
  const api = {} as Partial<IPCInvokeHandlers>;

  for (const channel of ipcInvokeChannels) {
    api[channel] = ((...args: any[]) => ipcRenderer.invoke(channel, ...args)) as any;
  }

  return api as IPCInvokeHandlers;
};
