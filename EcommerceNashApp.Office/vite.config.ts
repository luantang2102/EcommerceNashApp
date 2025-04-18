import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import mkcert from 'vite-plugin-mkcert';
import tailwindcss from '@tailwindcss/vite'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin(), mkcert(), tailwindcss()],
    server: {
        port: 3000,
    }
})
