import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const expectedVersion = process.env.PACKAGE_VERSION;
assert.ok(expectedVersion, 'PACKAGE_VERSION must contain the GitVersion semVer output.');

const packageFiles = [
  '../projects/remootio-angular/package.json',
  '../projects/remootio-angular/package-lock.json',
  '../dist/remootio-angular/package.json'
];

for (const packageFile of packageFiles) {
  const manifest = JSON.parse(readFileSync(new URL(packageFile, import.meta.url), 'utf8'));
  assert.equal(manifest.name, 'remootio-angular', `Unexpected package name in ${packageFile}`);
  assert.equal(manifest.version, expectedVersion, `Version mismatch in ${packageFile}`);
  if (manifest.packages) {
    assert.equal(manifest.packages[''].version, expectedVersion, 'Lockfile root version mismatch');
  }
}

console.log(`Verified remootio-angular@${expectedVersion} in source, lockfile and publish directory.`);
