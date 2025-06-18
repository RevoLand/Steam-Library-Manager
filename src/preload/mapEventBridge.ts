import { ipcRenderer } from 'electron';
import { IPCEventChannel, IPCEventDefinitions, IPCEventPayloads } from 'src/core/models/IPCEvents';

type IPCEventBridge = {
  [K in IPCEventChannel]: (callback: (data: IPCEventPayloads[K]) => void) => void;
};

export const mapEventBridge = (): IPCEventBridge => {
  const channels = Object.keys(IPCEventDefinitions) as IPCEventChannel[];

  return channels.reduce((acc, channel) => {
    acc[channel] = (callback: any) => {
      ipcRenderer.on(channel, (_, data) => callback(data));
    };

    return acc;
  }, {} as Partial<IPCEventBridge>) as IPCEventBridge;
};
