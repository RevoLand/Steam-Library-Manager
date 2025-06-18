import type { IPCInvokeHandlers } from 'src/core/models/IPCTypes';
import { IPCEventChannel } from './core/models/IPCEvents';

declare global {
  interface Window {
    api: IPCInvokeHandlers;
    events: {
      [K in IPCEventChannel]: (callback: (data: IPCEventPayloads[K]) => void) => void;
    };
  }

  interface ImportMeta {
    glob(
      pattern: string,
      options?: {
        eager?: boolean;
        import?: string;
      }
    ): Record<string, unknown>;
  }
}
