# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies
COPY backend/src/GudangPro.Domain/GudangPro.Domain.csproj backend/src/GudangPro.Domain/
COPY backend/src/GudangPro.Infrastructure/GudangPro.Infrastructure.csproj backend/src/GudangPro.Infrastructure/
COPY backend/src/GudangPro.Application/GudangPro.Application.csproj backend/src/GudangPro.Application/
COPY backend/src/GudangPro.Api/GudangPro.Api.csproj backend/src/GudangPro.Api/

RUN dotnet restore backend/src/GudangPro.Api/GudangPro.Api.csproj

# Copy full source and publish
COPY backend/src/ backend/src/
RUN dotnet publish backend/src/GudangPro.Api/GudangPro.Api.csproj -c Release -o /out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5000

ENTRYPOINT ["dotnet", "GudangPro.Api.dll"]
