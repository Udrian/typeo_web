# typeo_web

The TypeO homepage, hosted on [typeo.typedeaf.com](https://typeo.typedeaf.com). An ASP.NET Core
(Razor Pages) site with a landing page, about, download, getting started, a module guide and a
generated API documentation browser for every TypeO module version.

## Projects

| Project | Description |
| --- | --- |
| [src/TypeO.Web](src/TypeO.Web) | The website. Serves the pages and the documentation browser. |
| [src/TypeO.Web.Docs](src/TypeO.Web.Docs) | Shared models for the generated documentation JSON and the module registry. |
| [src/TypeO.Web.DocGen](src/TypeO.Web.DocGen) | Command line tool that generates the documentation data. |

## Running locally

```sh
dotnet run --project src/TypeO.Web
```

## Module documentation

The documentation browser is driven by two things:

- **`src/TypeO.Web/modules.json`** — the registry of modules shown on the site. To add a new module
  in the future, add an entry here (name, GitHub owner/repo and a description); nothing else is needed.
- **`src/TypeO.Web/wwwroot/docs-data/{Module}/{version}.json`** — generated API documentation, one
  file per module version, committed to this repository.

### How documentation is generated

`TypeO.Web.DocGen update` lists the release tags (`x.y.z`) of every module repository and, for each
tag that has no documentation yet:

1. clones the repository at that tag (with the `typer` submodule),
2. runs `typer generate` to create the project files from `typeo.json`,
3. builds the non-test projects with `GenerateDocumentationFile`,
4. merges the compiled assemblies with the XML documentation into a JSON file.

```sh
# All modules, all missing versions
dotnet run --project src/TypeO.Web.DocGen -- update \
  --modules src/TypeO.Web/modules.json \
  --output src/TypeO.Web/wwwroot/docs-data

# From an already generated and built local checkout
dotnet run --project src/TypeO.Web.DocGen -- extract \
  --repo ../typed --output src/TypeO.Web/wwwroot/docs-data
```

Prerequisites for `update`: .NET SDK 10, git, Python 3 with `requests` installed.

### Automatic updates

The [update-docs workflow](.github/workflows/update-docs.yml) runs the generator and commits new
documentation. It triggers on:

- `repository_dispatch` (`module-release`) — sent by a module repository when a version tag is
  pushed. Copy [docs/notify-website-on-tag.yml](docs/notify-website-on-tag.yml) into a module
  repository to enable this.
- a daily schedule — fallback that picks up any tag without a dispatch,
- manual `workflow_dispatch` — optionally limited to a single module.

## Downloads

The download page links to the TypeDater release assets on
[Udrian/typedater](https://github.com/Udrian/typedater/releases). Expected asset names
(configured in `appsettings.json`): `TypeDater-win-x64.zip`, `TypeDater-win-x86.zip`,
`TypeDater-linux-x64.tar.gz`, downloaded via `releases/latest/download/`.

## Deploying

### Docker / Dockge

The repository ships a [Dockerfile](Dockerfile) and a Dockge-ready [compose.yaml](compose.yaml).
The [publish-image workflow](.github/workflows/publish-image.yml) builds and pushes
`ghcr.io/udrian/typeo-web:latest` on every push — including the automatic documentation
commits — so deploying an update is a pull + restart of the stack.

1. In Dockge, create a stack (e.g. `typeo-web`) and paste the contents of [compose.yaml](compose.yaml).
2. If the GHCR package is private, run `docker login ghcr.io` on the server first
   (or make the package public in the GitHub package settings).
3. Point the reverse proxy for `typeo.typedeaf.com` at port `8080` of the host; the container
   honors `X-Forwarded-*` headers (`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`).
4. When new documentation or site changes land, pull + restart the stack from the Dockge UI.

Alternatively the stack can build straight from the repository without a registry — see the
commented `build:` line in [compose.yaml](compose.yaml).

### Bare .NET

```sh
dotnet publish src/TypeO.Web -c Release
```

The publish output is a regular ASP.NET Core app (including `modules.json` and
`wwwroot/docs-data`) that can be hosted behind any reverse proxy on typeo.typedeaf.com.
