import { ipcMain } from 'electron';
import { IPCInvokeChannel, IPCInvokeDefinitions } from 'src/core/models/IPCTypes';

export function registerIPCHandlers() {
  // eslint-disable-next-line @typescript-eslint/ban-types
  const entries = Object.entries(IPCInvokeDefinitions) as [IPCInvokeChannel, { handler: Function }][];

  for (const [channel, def] of entries) {
    ipcMain.handle(channel, async (_, ...args) => def.handler(...args));
  }
}
