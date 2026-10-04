# Package publishing with GitHub OIDC

The release workflow publishes from pushes to main after the reusable build/test workflow succeeds. Only the two publishing jobs have id-token: write; the GitHub release job alone has contents: write. Pull requests cannot publish.

## Provider configuration

Configure these values exactly in both providers:

| Field | Value |
| --- | --- |
| Provider | GitHub Actions |
| Repository owner | erenken |
| Repository | gateMonitor |
| Workflow filename | dotnet-release.yml |
| Environment | Leave empty (the jobs do not specify an environment) |
| Policy/connection label | gateMonitor-release |

### NuGet

In the erenken NuGet account, create a Trusted Publishing policy owned by erenken. Allow Push new packages and package versions, with the exact package pattern myNOC.Remootio. Do not grant unlist/relist permissions. Allowing creation is needed for the first public release of this package.

The workflow uses NuGet/login v1.2.0 with user erenken, then passes its short-lived NUGET_API_KEY output to dotnet nuget push. This is an ephemeral credential, not a repository secret. NuGet symbols are included in snupkg format. Both build validation and the release packing job extract the symbol package and run `sourcelink test` on its .NET 8 and .NET 10 portable PDBs before any publication. GitHub Actions builds normalize source paths and record the GitHub repository URL and exact commit.

### npm

Open remootio-angular package Settings and add a GitHub Actions trusted publisher with the values above. Enable Allow npm publish to preserve automatic direct releases; leave Allow npm dist-tag disabled. Direct publishing does not require a separate human approval in npm. npm also supports staged publishing if manual approval is preferred in a future change.

The workflow explicitly installs npm 12.2.0, publishes the built angular/dist/remootio-angular directory with `--tag latest`, and requests provenance. It does not set NODE_AUTH_TOKEN. Keep the package repository URL set to `git+https://github.com/erenken/gateMonitor.git` so npm accepts the canonical Git URL and provenance matches the GitHub repository.

## Version stamping and pre-publish validation

Both registries and the GitHub tag use GitVersion `semVer`. The `main` branch uses `mode: ContinuousDeployment` with `label: ''` to produce stable `major.minor.patch` versions. Before either publishing job starts, the version job rejects prerelease suffixes, build metadata, and malformed versions. GitHub releases are explicitly marked as stable. Work branches may still produce alpha versions for PR validation, but cannot publish. Successful release tags establish the baseline for the next stable version.

The npm publishing job performs these steps in order:

1. Install the Angular workspace with `npm ci`.
2. Run `npm run versionService` with `PACKAGE_VERSION` set to GitVersion `semVer`. The script invokes `npm version "$PACKAGE_VERSION" --no-git-tag-version --allow-same-version` in `angular/projects/remootio-angular`. This updates the library manifest and lockfile before building.
3. Build with `npm run buildService`; ng-packagr copies the stamped version into `angular/dist/remootio-angular/package.json`.
4. Run `npm run verifyServiceVersion` with the same `PACKAGE_VERSION`. This checks the source manifest, both lockfile version fields, and the built manifest. A missing or mismatched version fails the job.
5. Dry-run npm packaging, then publish the verified `dist/remootio-angular` directory with OIDC and provenance.

PR/reusable validation runs the same version check and packaging dry run. NuGet receives the same version through `-p:PackageVersion`. The checked-in npm version remains a development baseline; the workflow does not commit manifest updates or create an npm version tag. The final GitHub release job creates `v<semVer>` only after both publishing jobs succeed.

Manual `npm run deployService` uses the same stamp/build/verify sequence and requires an explicit semantic `PACKAGE_VERSION`; it fails before publishing if that value is absent. It does not derive a local GitVersion automatically. Local publishing requires your own npm authentication; trusted publishing is used by GitHub Actions. Prefer CI publishing.

## Building and verifying the Angular package

Install dependencies once at the Angular workspace root; a separate library install is not needed:

```bash
cd angular
npm ci
```

To build and inspect a package using the checked-in development version, run from `angular/`:

```bash
npm run buildService
npm pack ./dist/remootio-angular --dry-run
```

To reproduce CI version stamping, replace the sample version with the intended version and run from `angular/`. In Bash:

```bash
export PACKAGE_VERSION=1.0.0-alpha.2
npm run versionService
npm run buildService
npm run verifyServiceVersion
npm pack ./dist/remootio-angular --dry-run
```

In PowerShell:

```powershell
$env:PACKAGE_VERSION = "1.0.0-alpha.2"
npm.cmd run versionService
npm.cmd run buildService
npm.cmd run verifyServiceVersion
npm.cmd pack ./dist/remootio-angular --dry-run
```

Version stamping modifies the source library manifest and lockfile; review those changes before committing. Only `dist/remootio-angular` is published, never the private dashboard package. Local dry runs inspect package contents but do not publish or validate OIDC authentication. For workspace development and tests, see the [Angular dashboard README](../angular/README.md).

## Building and verifying the .NET package

Build and run all library and Blazor tests from the repository root:

```bash
dotnet build dotnet/GateMonitor.slnx --configuration Release -warnaserror
dotnet test dotnet/GateMonitor.slnx --configuration Release --no-build
```

### Source Link and symbols

Builds produce portable PDBs; NuGet packing produces a separate `.snupkg` containing symbols for both target frameworks. The main package contains the assemblies, README and license, not duplicate PDBs. `PublishRepositoryUrl` includes the GitHub repository URL in package metadata, and Source Link maps tracked source files to the exact source commit. The Source Link package keeps its build assets enabled while `PrivateAssets=all` prevents build tooling from becoming a consumer dependency.

GitHub Actions enables `ContinuousIntegrationBuild` for normalized source paths, extracts each symbol package and runs `sourcelink test` against both PDBs before publishing. That checks source downloads and document checksums, not just the presence of Source Link metadata. For a local CI-style build and verification, run from the repository root:

```bash
dotnet build dotnet/myNOC.Remootio/myNOC.Remootio.csproj --configuration Release -p:ContinuousIntegrationBuild=true -warnaserror
dotnet tool install sourcelink --version 3.1.1 --tool-path .pack/sourcelink
.pack/sourcelink/sourcelink test dotnet/myNOC.Remootio/bin/Release/net8.0/myNOC.Remootio.pdb
.pack/sourcelink/sourcelink test dotnet/myNOC.Remootio/bin/Release/net10.0/myNOC.Remootio.pdb
```

On Windows, use `.pack/sourcelink/sourcelink.exe`. Remote Source Link checks require network access and a commit available on GitHub. Locally edited tracked source may fail checksums until its commit is pushed. Generated/untracked sources are embedded. These local commands verify the build-output PDBs; CI also verifies the PDBs extracted from the packed `.snupkg`.

## Security and migration

Provider trust is scoped to the repository and workflow filename, not a branch. The committed workflow only triggers on main. Protect main and review workflow changes carefully. For provider-enforced branch restrictions, add a GitHub environment restricted to main, then configure the identical environment name in both provider policies and both publishing jobs.

Save the provider policies before merging the OIDC workflow changes. npm may require an interactive two-factor verification when saving its connection. Do not rename the workflow without updating provider policies.

Keep the existing NUGET_PUBLISH and NPM_TOKEN GitHub secrets until the first successful OIDC release, but they are no longer read by the new workflow. After success, remove those secrets and revoke their corresponding provider keys/tokens only after confirming they are not used by other repositories or manual publishing.

A local build or npm pack --dry-run cannot verify OIDC exchange. Final authentication and provenance validation require a GitHub-hosted workflow run. Failed releases can partially publish one registry before the other fails; retrying an already published npm version requires reconciliation before rerunning.

## References

- [NuGet trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
- [NuGet OIDC login action](https://github.com/NuGet/login)
- [npm trusted publishing](https://docs.npmjs.com/trusted-publishers/)
