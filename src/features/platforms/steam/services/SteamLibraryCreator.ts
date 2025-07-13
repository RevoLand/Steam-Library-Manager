/* eslint-disable camelcase */
/* eslint-disable quotes */
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join, normalize } from 'node:path';
import TransferOperation from 'src/core/models/TransferOperation';
import { hashText } from 'src/core/utils/hash';
import { appMover } from 'src/features/apps/services/appMover';
import LibraryCreator from 'src/features/libraries/models/LibraryCreator';
import { parse, stringify } from 'vdf-parser';
import LibraryFolders from '../models/LibraryFolders';
import SteamLibrary from '../models/SteamLibrary';
import { getSteamInstallPath } from '../utils/steam';
import { escapeVdfPath } from '../utils/vdf';

export default class SteamLibraryCreator implements LibraryCreator {
  async create(path: string, label = ''): Promise<SteamLibrary> {
    const steamPath = getSteamInstallPath();

    if (!steamPath) {
      throw new Error("Steam path couldn't not be found");
    }

    const vdfPath = join(steamPath, 'steamapps', 'libraryfolders.vdf');

    if (!existsSync(vdfPath)) {
      throw new Error("Steam path couldn't not be found");
    }

    const content = parse<LibraryFolders>(readFileSync(vdfPath).toString());

    mkdirSync(join(path, 'steamapps'), { recursive: true });

    await appMover.performTransfer(join(steamPath, 'Steam.dll'), join(path, 'Steam.dll'), {
      operation: TransferOperation.COPY,
    });

    const newLibraryKey = Object.keys(content.libraryfolders).length.toString();

    content.libraryfolders[newLibraryKey] = {
      path: escapeVdfPath(normalize(path)),
      label,
      contentid: '',
      totalsize: '0',
      update_clean_bytes_tally: '',
      time_last_update_verified: '',
      apps: {},
    };

    const newVdfContent = stringify(content, { pretty: true, indent: '\t' });

    writeFileSync(vdfPath, newVdfContent);

    const libraryId = await hashText(path);

    return new SteamLibrary(libraryId, path, label, 'steam');
  }
}
