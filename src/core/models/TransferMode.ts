enum TransferMode {
  NONE = 0,
  COPY = 1 << 0, // 1
  MOVE = 1 << 1, // 2
  DRY_RUN = 1 << 2, // 4
  SKIP_EXISTING = 1 << 3, // 8
  VERIFY = 1 << 4, // 16
}

export default TransferMode;
