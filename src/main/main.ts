import { app, BrowserWindow } from 'electron';
import installExtension, { REACT_DEVELOPER_TOOLS } from 'electron-devtools-installer';
import started from 'electron-squirrel-startup';
import path from 'node:path';
import { initDatabase } from 'src/core/db/initDB';
import { libraryManager } from 'src/features/libraries/services/LibraryManager';
import { initializePlatforms } from 'src/features/platforms/init';
import { bridgeAllEventSources } from '../preload/mapEventRendererBridge';
import { registerIPCHandlers } from './ipcHandlers';
import taskManager from './taskManager';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (started) {
  app.quit();
}

const createWindow = () => {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    minHeight: 600,
    minWidth: 800,
    width: 800,
    height: 600,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
    },
    center: true,
  });

  // and load the index.html of the app.
  if (MAIN_WINDOW_VITE_DEV_SERVER_URL) {
    mainWindow.loadURL(MAIN_WINDOW_VITE_DEV_SERVER_URL);
  } else {
    mainWindow.loadFile(path.join(__dirname, '../renderer/index.html'));
  }

  if (!app.isPackaged) {
    mainWindow.webContents.openDevTools({ mode: 'detach' });
  } else {
    mainWindow.webContents.on('before-input-event', (event, input) => {
      const isDevShortcut = input.key.toLowerCase() === 'i' && input.control && input.shift;

      if (isDevShortcut || input.key === 'F12') {
        event.preventDefault();
      }
    });
  }

  bridgeAllEventSources([taskManager], mainWindow);
};

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and import them here.

app.whenReady().then(async () => {
  installExtension([REACT_DEVELOPER_TOOLS]);

  registerIPCHandlers();

  initDatabase();

  await initializePlatforms();

  libraryManager.loadLibraries().then(() => libraryManager.populateAllApps());

  createWindow();
});
