# Builds the TypeO homepage (TypeO.Web) into a self-contained runtime image.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so dependency layers are cached between builds.
COPY typeo_web.sln ./
COPY src/TypeO.Web/TypeO.Web.csproj src/TypeO.Web/
COPY src/TypeO.Web.Docs/TypeO.Web.Docs.csproj src/TypeO.Web.Docs/
COPY src/TypeO.Web.DocGen/TypeO.Web.DocGen.csproj src/TypeO.Web.DocGen/
RUN dotnet restore typeo_web.sln

COPY . .
RUN dotnet publish src/TypeO.Web -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The site runs behind a reverse proxy that terminates TLS for typeo.typedeaf.com;
# honor its X-Forwarded-* headers so the app knows the original scheme/host.
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

EXPOSE 8080
ENTRYPOINT ["dotnet", "TypeO.Web.dll"]
