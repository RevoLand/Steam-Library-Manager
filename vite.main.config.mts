import { defineConfig } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';

// https://vitejs.dev/config
export default defineConfig({
  plugins: [tsconfigPaths()],
  build: {
    minify: true,
    sourcemap: false,
    rollupOptions: {
      external: ['better-sqlite3'],
    },
  },
});
