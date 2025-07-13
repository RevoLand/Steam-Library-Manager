export function hasFlag(mode: number, flag: number): boolean {
  return (mode & flag) === flag;
}
