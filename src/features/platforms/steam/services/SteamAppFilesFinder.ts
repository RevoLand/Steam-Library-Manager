import path from 'node:path';
import { TransferPattern } from 'src/core/models/FilePattern';
import AppFilesFinder from '../../interfaces/AppFilesFinder';
import SteamApp from '../models/SteamApp';
import SteamLibrary from '../models/SteamLibrary';

function generateSteamAppPatterns(appId: number, installPath: string, libraryPath: string): TransferPattern[] {
  return [
    {
      type: 'directory',
      path: path.join(libraryPath, 'common', installPath),
      pattern: '*',
      recursive: true,
    },
    {
      type: 'directory',
      path: path.join(libraryPath, 'downloading', installPath),
      pattern: '*',
      recursive: true,
    },
    {
      type: 'directory',
      path: path.join(libraryPath, 'workshop', 'content', appId.toString()),
      pattern: '*',
      recursive: true,
    },
    {
      type: 'directory',
      path: path.join(libraryPath, 'downloading'),
      pattern: `*${appId}*.patch`,
      recursive: false,
    },
    {
      type: 'file',
      path: path.join(libraryPath, `appmanifest_${appId}.acf`),
    },
    {
      type: 'file',
      path: path.join(libraryPath, 'workshop', `appworkshop_${appId}.acf`),
    },
  ];
}

class SteamAppFilesFinder implements AppFilesFinder {
  canHandle(library: SteamLibrary): boolean {
    return library.type === 'steam';
  }

  generatePatterns(app: SteamApp, library: SteamLibrary): TransferPattern[] {
    return generateSteamAppPatterns(app.appId, app.installPath, library.path);
  }
}

export default SteamAppFilesFinder;
