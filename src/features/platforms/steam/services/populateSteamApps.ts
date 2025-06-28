import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import AcfFile from 'src/features/platforms/steam/models/AcfFile';
import SteamApp from 'src/features/platforms/steam/models/SteamApp';
import { parse } from 'vdf-parser';
import SteamLibrary from '../models/SteamLibrary';

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
    const vdfParser: AcfFile = parse(readFileSync(acfFile).toString());

    if (!vdfParser?.AppState) {
      return;
    }

    apps.push(
      new SteamApp({
        appId: vdfParser.AppState.appid,
        installPath: vdfParser.AppState.installdir,
        libraryId: library.id,
        name: vdfParser.AppState.name,
        stateFlags: vdfParser.AppState.StateFlags,
      })
    );
  });

  library.setApps(apps);
};
