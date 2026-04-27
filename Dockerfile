FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MilestoneTracker.API/MilestoneTracker.API.csproj", "MilestoneTracker.API/"]
COPY ["MilestoneTracker.Application/MilestoneTracker.Application.csproj", "MilestoneTracker.Application/"]
COPY ["MilestoneTracker.Domain/MilestoneTracker.Domain.csproj", "MilestoneTracker.Domain/"]
COPY ["MilestoneTracker.Infrastructure/MilestoneTracker.Infrastructure.csproj", "MilestoneTracker.Infrastructure/"]
RUN dotnet restore "MilestoneTracker.API/MilestoneTracker.API.csproj"

COPY . .
WORKDIR "/src/MilestoneTracker.API"
RUN dotnet build "MilestoneTracker.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MilestoneTracker.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MilestoneTracker.API.dll"]
