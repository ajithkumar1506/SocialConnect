# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/SocialConnect.API/SocialConnect.API.csproj", "src/SocialConnect.API/"]
COPY ["src/SocialConnect.Application/SocialConnect.Application.csproj", "src/SocialConnect.Application/"]
COPY ["src/SocialConnect.Domain/SocialConnect.Domain.csproj", "src/SocialConnect.Domain/"]
COPY ["src/SocialConnect.Infrastructure/SocialConnect.Infrastructure.csproj", "src/SocialConnect.Infrastructure/"]
RUN dotnet restore "./src/SocialConnect.API/SocialConnect.API.csproj"
COPY . .
WORKDIR "/src/src/SocialConnect.API"
RUN dotnet build "./SocialConnect.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SocialConnect.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialConnect.API.dll"]
