# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached layer as long as project files don't change)
COPY Bazaarche.slnx ./
COPY src/Bazaarche.Domain/Bazaarche.Domain.csproj src/Bazaarche.Domain/
COPY src/Bazaarche.Application/Bazaarche.Application.csproj src/Bazaarche.Application/
COPY src/Bazaarche.Infrastructure/Bazaarche.Infrastructure.csproj src/Bazaarche.Infrastructure/
COPY src/Bazaarche.Api/Bazaarche.Api.csproj src/Bazaarche.Api/
RUN dotnet restore src/Bazaarche.Api/Bazaarche.Api.csproj

COPY src/ src/
RUN dotnet publish src/Bazaarche.Api/Bazaarche.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# Uploads live in a volume; the folder must be writable by the non-root "app" user.
RUN mkdir -p /app/uploads && chown -R $APP_UID:$APP_UID /app/uploads
COPY --from=build /app/publish .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080 \
    Storage__UploadsPath=/app/uploads
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bazaarche.Api.dll"]
