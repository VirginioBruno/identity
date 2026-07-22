FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY identity.sln ./
COPY src/identity.api/identity.api.csproj src/identity.api/
RUN dotnet restore src/identity.api/identity.api.csproj

COPY src/identity.api/ src/identity.api/
RUN dotnet publish src/identity.api/identity.api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "identity.api.dll"]
