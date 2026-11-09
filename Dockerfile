# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/PrototypeApi/PrototypeApi.csproj ./PrototypeApi/
COPY src/PrototypeLibrary/PrototypeLibrary.csproj ./PrototypeLibrary/
RUN dotnet restore ./PrototypeApi/PrototypeApi.csproj

COPY src/PrototypeApi/. ./PrototypeApi/
COPY src/PrototypeLibrary/. ./PrototypeLibrary/
WORKDIR /src/PrototypeApi
RUN dotnet publish -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "PrototypeApi.dll"]
