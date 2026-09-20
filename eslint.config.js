import js from '@eslint/js';
import tseslint from 'typescript-eslint';
import globals from 'globals';
import prettier from 'eslint-config-prettier';

export default tseslint.config(
  {
    ignores: [
      '**/dist/**',
      '**/dist-types/**',
      '**/coverage/**',
      '**/node_modules/**',
      '**/*.tsbuildinfo',
    ],
  },
  js.configs.recommended,
  ...tseslint.configs.recommended,
  { files: ['client/**/*.ts'], languageOptions: { globals: globals.browser } },
  {
    files: ['server/**/*.ts', '*.js', '*.cjs', '*.ts'],
    languageOptions: { globals: globals.node },
  },
  prettier,
);
