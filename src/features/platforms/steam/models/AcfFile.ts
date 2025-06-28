type AcfFile = {
  AppState: {
    appid: number;
    name: string;
    StateFlags: number;
    installdir: string;
    SizeOnDisk: number;
  };
};

export default AcfFile;
