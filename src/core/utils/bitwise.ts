export function hasMode(mode: number, flag: number): boolean {
  return (mode & flag) === flag;
}
