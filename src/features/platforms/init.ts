export const initializePlatforms = async () => {
  const modules: Record<string, { register?: () => void }> = import.meta.glob('./**/init.ts', { eager: true });

  Object.values(modules).forEach((mod) => {
    mod.register?.();
  });
};
