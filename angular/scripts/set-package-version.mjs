import assert from 'node:assert/strict';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const packageVersion = process.env.PACKAGE_VERSION;
assert.ok(packageVersion, 'PACKAGE_VERSION must contain the intended release version.');
assert.match(packageVersion, /^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$/, 'PACKAGE_VERSION must be an explicit semantic version.');
assert.ok(process.env.npm_execpath, 'Run this script through npm run versionService.');

execFileSync(process.execPath, [
  process.env.npm_execpath,
  '--prefix', fileURLToPath(new URL('../projects/remootio-angular/', import.meta.url)),
  'version', packageVersion,
  '--no-git-tag-version', '--allow-same-version'
], {
  cwd: fileURLToPath(new URL('../projects/remootio-angular/', import.meta.url)),
  stdio: 'inherit'
});
