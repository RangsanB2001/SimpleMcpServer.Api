# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SimpleMcpServer.Api/SimpleMcpServer.Api.csproj", "src/SimpleMcpServer.Api/"]
COPY ["src/SimpleMcpServer.Application/SimpleMcpServer.Application.csproj", "src/SimpleMcpServer.Application/"]
COPY ["src/SimpleMcpServer.Domain/SimpleMcpServer.Domain.csproj", "src/SimpleMcpServer.Domain/"]
COPY ["src/SimpleMcpServer.Infrastructure/SimpleMcpServer.Infrastructure.csproj", "src/SimpleMcpServer.Infrastructure/"]
RUN dotnet restore "./src/SimpleMcpServer.Api/SimpleMcpServer.Api.csproj"
COPY . .
WORKDIR "/src/src/SimpleMcpServer.Api"
RUN dotnet build "./SimpleMcpServer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SimpleMcpServer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleMcpServer.Api.dll"]