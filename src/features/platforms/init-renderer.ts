export const initializeRendererPlatforms = async () => {
  const modules: Record<string, { register?: () => void }> = import.meta.glob('./**/init-renderer.ts', { eager: true });

  Object.values(modules).forEach((mod) => {
    mod.register?.();
  });
};
