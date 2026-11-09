# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/TestKubeApi/TestKubeApi.csproj ./TestKubeApi/
COPY src/TestKubeLibrary/TestKubeLibrary.csproj ./TestKubeLibrary/
RUN dotnet restore ./TestKubeApi/TestKubeApi.csproj

COPY src/TestKubeApi/. ./TestKubeApi/
COPY src/TestKubeLibrary/. ./TestKubeLibrary/
WORKDIR /src/TestKubeApi
RUN dotnet publish -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "TestKubeApi.dll"]
