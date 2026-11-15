@echo off
rem Generates API documentation for any module release tags that have no docs yet.
rem Requires: .NET 10 SDK, git and Python 3 with requests (pip install requests).
pushd "%~dp0"

dotnet run --project src\TypeO.Web.DocGen -- update ^
  --modules src\TypeO.Web\modules.json ^
  --output src\TypeO.Web\wwwroot\docs-data

popd
