# Use the base image for the runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApp1/WebApp1.csproj", "WebApp1/"]
COPY ["Business/Business.csproj", "Business/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "./WebApp1/WebApp1.csproj"
COPY . .
WORKDIR "/src/WebApp1"
RUN dotnet build "./WebApp1.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApp1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp1.dll"]