# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY src/GudangPro.Domain/GudangPro.Domain.csproj src/GudangPro.Domain/
COPY src/GudangPro.Infrastructure/GudangPro.Infrastructure.csproj src/GudangPro.Infrastructure/
COPY src/GudangPro.Application/GudangPro.Application.csproj src/GudangPro.Application/
COPY src/GudangPro.Api/GudangPro.Api.csproj src/GudangPro.Api/

RUN dotnet restore src/GudangPro.Api/GudangPro.Api.csproj

# Copy full source and publish
COPY src/ src/
RUN dotnet publish src/GudangPro.Api/GudangPro.Api.csproj -c Release -o /out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5000

ENTRYPOINT ["dotnet", "GudangPro.Api.dll"]
