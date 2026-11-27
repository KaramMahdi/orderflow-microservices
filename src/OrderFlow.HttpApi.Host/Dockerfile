# OrderFlow.HttpApi.Host image.
# Build context: the repository root (docker-compose.yml sets it).
#   docker build -f src/OrderFlow.HttpApi.Host/Dockerfile .

ARG DOTNET_SDK_IMAGE
ARG DOTNET_ASPNET_IMAGE

# ---------------------------------------------------------------
# Build stage
# ---------------------------------------------------------------
FROM ${DOTNET_SDK_IMAGE} AS build
WORKDIR /src

# Central build settings first: restore needs them, and they are cached with it.
# .editorconfig carries the analyzer settings that the Release build enforces.
COPY Directory.Build.props Directory.Packages.props nuget.config global.json .editorconfig ./

# Then only the project files of the host and every project it references,
# so the restore layer stays cached until a project file changes.
COPY src/Domain.Shared.Layer/*.csproj src/Domain.Shared.Layer/
COPY src/Shared.AspNetCore/*.csproj src/Shared.AspNetCore/
COPY src/Shared.Infrastructure/*.csproj src/Shared.Infrastructure/
COPY src/OrderFlow.Domain.Shared/*.csproj src/OrderFlow.Domain.Shared/
COPY src/OrderFlow.Domain/*.csproj src/OrderFlow.Domain/
COPY src/OrderFlow.Application.Contracts/*.csproj src/OrderFlow.Application.Contracts/
COPY src/OrderFlow.Application/*.csproj src/OrderFlow.Application/
COPY src/OrderFlow.EntityFrameworkCore/*.csproj src/OrderFlow.EntityFrameworkCore/
COPY src/OrderFlow.HttpApi/*.csproj src/OrderFlow.HttpApi/
COPY src/Payments.Infrastructure/*.csproj src/Payments.Infrastructure/
COPY src/OrderFlow.HttpApi.Host/*.csproj src/OrderFlow.HttpApi.Host/
RUN dotnet restore src/OrderFlow.HttpApi.Host/OrderFlow.HttpApi.Host.csproj

# Now the sources, then publish without restoring again.
COPY src/ src/
RUN dotnet publish src/OrderFlow.HttpApi.Host/OrderFlow.HttpApi.Host.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

# ---------------------------------------------------------------
# Runtime stage
# ---------------------------------------------------------------
FROM ${DOTNET_ASPNET_IMAGE} AS final

# curl is used only by the container healthcheck in docker-compose.yml.
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
# Files stay owned by root, so the app user can read them but not change them.
COPY --from=build /app/publish .

# The .NET images ship a non-root user; APP_UID is its id.
USER $APP_UID

ENTRYPOINT ["dotnet", "OrderFlow.HttpApi.Host.dll"]
