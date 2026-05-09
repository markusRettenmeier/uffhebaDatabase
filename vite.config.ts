import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
  build: {
    outDir: 'wwwroot/js',
    emptyOutDir: false,
    minify: 'esbuild',

    rollupOptions: {
      input: {
        // global
        shared: resolve(__dirname, 'Scripts/shared.ts'),
        api: resolve(__dirname, 'Scripts/api.ts'),
        translationService: resolve(__dirname, 'Scripts/TranslationService.ts'),
        fancyboxInit: resolve(__dirname, 'Scripts/fancyboxInit.ts'),
        helperFunctions: resolve(__dirname, 'Scripts/helperFunctions.ts'),
        global: resolve(__dirname, 'Scripts/global.ts'),
        types: resolve(__dirname, 'Scripts/types.ts'),

        // searchByColumn
        searchByColumn: resolve(__dirname, 'Scripts/searchByColumn/searchByColumn.ts'),
        searchByColumnaddElements: resolve(__dirname, 'Scripts/searchByColumn/addElements.ts'),
        searchByColumncolumns: resolve(__dirname, 'Scripts/searchByColumn/columns.ts'),
        searchByColumncreateElements: resolve(__dirname, 'Scripts/searchByColumn/createElements.ts'),
        searchByColumnsessionStorage: resolve(__dirname, 'Scripts/searchByColumn/sessionStorage.ts'),
        searchByColumnvariables: resolve(__dirname, 'Scripts/searchByColumn/variables.ts'),

        // pages
        pagescollectionItem: resolve(__dirname, 'Scripts/pages/collectionItem.ts'),
        pagesconceptualRelationship: resolve(__dirname, 'Scripts/pages/conceptualRelationship.ts'),
        pagesparticipant: resolve(__dirname, 'Scripts/pages/participant.ts'),
        pagesplace: resolve(__dirname, 'Scripts/pages/place.ts'),
        // passkey
        pageslogin: resolve(__dirname, 'Scripts/pages/login.ts'),
        pagesregister: resolve(__dirname, 'Scripts/pages/register.ts'),
      },

      output: {
        entryFileNames: (chunkInfo) => {
          const name = chunkInfo.name;

          if (name.startsWith('searchByColumn')) {
            if (name === 'searchByColumn')
              return 'searchByColumn/[name].min.js';
            else
              return 'searchByColumn/' + name.substring(14) + '.min.js';
          }

          if (name.startsWith('pages')) {
            return 'pages/' + name.substring(5) + '.min.js';
          }

          return '[name].min.js';
        },

        chunkFileNames: 'chunks/[name]-[hash].js',
      }
    }
  }
});