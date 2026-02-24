FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY src/MeilisearchSyncService.csproj ./
RUN dotnet restore

COPY src/ ./
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app ./

ENV DOTNET_ENVIRONMENT=Production
ENV COMPlus_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "MeilisearchSyncService.dll"]
