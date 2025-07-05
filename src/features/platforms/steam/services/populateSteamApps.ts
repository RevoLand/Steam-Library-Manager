import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import AcfFile from 'src/features/platforms/steam/models/AcfFile';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import { parse } from 'vdf-parser';
import SteamLibrary from '../models/SteamLibrary';
import { toLowerCaseKeysDeep } from '../utils/vdf';

export const populateSteamApps = async (library: SteamLibrary): Promise<void> => {
  if (!existsSync(library.path)) {
    return;
  }

  const appAcfFiles = readdirSync(library.path, {
    withFileTypes: true,
  })
    .filter((entry) => entry.isFile() && entry.name.endsWith('.acf'))
    .map((entry) => join(library.path, entry.name));

  const apps: SteamApp[] = [];

  appAcfFiles.forEach((acfFile) => {
    const vdfParser: AcfFile = toLowerCaseKeysDeep(parse(readFileSync(acfFile).toString()));

    if (!vdfParser.appstate) {
      return;
    }

    apps.push(
      new SteamApp({
        appId: vdfParser.appstate.appid,
        installPath: vdfParser.appstate.installdir,
        libraryId: library.id,
        name: vdfParser.appstate.name,
        stateFlags: vdfParser.appstate.stateflags,
        sizeOnDisk: vdfParser.appstate.sizeondisk,
      })
    );
  });

  library.setApps(apps);
};
