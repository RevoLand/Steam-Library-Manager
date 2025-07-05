type AcfFile = {
  appstate: {
    appid: number;
    name: string;
    stateflags: number;
    installdir: string;
    sizeondisk: number;
  };
};

export default AcfFile;
