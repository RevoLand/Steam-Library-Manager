// See the Electron documentation for details on how to use preload scripts:
// https://www.electronjs.org/docs/latest/tutorial/process-model#preload-scripts

import { contextBridge } from 'electron';
import { mapEventBridge } from './mapEventBridge';
import { mapIPCToBridge } from './mapIPCToBridge';

contextBridge.exposeInMainWorld('api', mapIPCToBridge());
contextBridge.exposeInMainWorld('events', mapEventBridge());
