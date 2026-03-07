import { defineConfig } from './node_modules/vite/dist/node/index';
import { resolve } from 'path';

export default defineConfig({
    build: {
        outDir: 'wwwroot/js/passkey',
        emptyOutDir: true,
        rollupOptions: {
            input: {
                login: resolve(__dirname, 'Scripts/passkey/login.ts'),
                register: resolve(__dirname, 'Scripts/passkey/register.ts'),
            },
            output: {
                entryFileNames: '[name].js'
            }
        }
    }
});
