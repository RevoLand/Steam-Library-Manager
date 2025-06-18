import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';
import { defineConfig } from 'vite';
import pages from 'vite-plugin-pages';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
  root: resolve(__dirname, 'src/renderer'),
  plugins: [
    tsconfigPaths(),
    react(),
    pages({
      dirs: ['pages'],
    }),
    tailwindcss(),
  ],
  build: {
    minify: true,
    sourcemap: false,
    outDir: '../../.vite/renderer',
    emptyOutDir: true,
  },
});
