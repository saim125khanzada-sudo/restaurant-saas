# Multi-stage Dockerfile for RestaurantSaaS.Api
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files for caching restore layer
COPY RestaurantSaaS.sln ./
COPY src/Backend/RestaurantSaaS.SharedKernel/RestaurantSaaS.SharedKernel.csproj ./src/Backend/RestaurantSaaS.SharedKernel/
COPY src/Backend/RestaurantSaaS.Domain/RestaurantSaaS.Domain.csproj ./src/Backend/RestaurantSaaS.Domain/
COPY src/Backend/RestaurantSaaS.Application/RestaurantSaaS.Application.csproj ./src/Backend/RestaurantSaaS.Application/
COPY src/Backend/RestaurantSaaS.Infrastructure/RestaurantSaaS.Infrastructure.csproj ./src/Backend/RestaurantSaaS.Infrastructure/
COPY src/Backend/RestaurantSaaS.Api/RestaurantSaaS.Api.csproj ./src/Backend/RestaurantSaaS.Api/
COPY tests/RestaurantSaaS.UnitTests/RestaurantSaaS.UnitTests.csproj ./tests/RestaurantSaaS.UnitTests/
COPY tests/RestaurantSaaS.ArchitectureTests/RestaurantSaaS.ArchitectureTests.csproj ./tests/RestaurantSaaS.ArchitectureTests/
COPY tests/RestaurantSaaS.MultiTenancyTests/RestaurantSaaS.MultiTenancyTests.csproj ./tests/RestaurantSaaS.MultiTenancyTests/

RUN dotnet restore

# Copy full source and publish release build
COPY src/ ./src/
COPY tests/ ./tests/
WORKDIR /app/src/Backend/RestaurantSaaS.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=15s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "RestaurantSaaS.Api.dll"]
