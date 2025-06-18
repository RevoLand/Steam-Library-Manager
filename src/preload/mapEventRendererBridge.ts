import { BrowserWindow } from 'electron';
import { IPCEventChannel, IPCEventDefinitions, IPCEventPayloads } from 'src/core/models/IPCEvents';

const registeredSources = new WeakSet<NodeJS.EventEmitter>();

export function bridgeEventsToRenderer<T extends NodeJS.EventEmitter>(source: T, targetWindow: BrowserWindow) {
  if (registeredSources.has(source)) {
    return;
  }

  const channels = Object.keys(IPCEventDefinitions) as IPCEventChannel[];

  for (const channel of channels) {
    source.on(channel, (payload: IPCEventPayloads[typeof channel]) => {
      targetWindow.webContents.send(channel, payload);
    });
  }

  registeredSources.add(source);
}

export function bridgeAllEventSources(sources: NodeJS.EventEmitter[], targetWindow: BrowserWindow) {
  sources.forEach((source) => bridgeEventsToRenderer(source, targetWindow));
}
