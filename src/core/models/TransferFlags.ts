enum TransferFlags {
  NONE = 0,
  VERIFY = 1 << 0,
  SKIP_EXISTING = 1 << 1,
  DRY_RUN = 1 << 2,
}

export default TransferFlags;
