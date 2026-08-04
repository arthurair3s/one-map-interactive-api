FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OnePieceMap.slnx ./
COPY src/OnePieceMap.Api.csproj src/OnePieceMap.Api.csproj
RUN dotnet restore src/OnePieceMap.Api.csproj

COPY src/ src/
RUN dotnet publish src/OnePieceMap.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Npgsql probes for Kerberos/GSSAPI support at connection time; without this lib
# it still connects fine, but logs a "Cannot load library" warning on every attempt.
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "OnePieceMap.Api.dll"]
