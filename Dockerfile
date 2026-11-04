# Stage 1: build with the SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files first so restore is cached when only code changes.
# RoadRules references RoadRules.Client (WebAssembly) and RoadRules.Shared (DTOs).
COPY RoadRules/RoadRules.csproj RoadRules/
COPY RoadRules.Client/RoadRules.Client.csproj RoadRules.Client/
COPY RoadRules.Shared/RoadRules.Shared.csproj RoadRules.Shared/
RUN dotnet restore RoadRules/RoadRules.csproj

COPY RoadRules/ RoadRules/
COPY RoadRules.Client/ RoadRules.Client/
COPY RoadRules.Shared/ RoadRules.Shared/
RUN dotnet publish RoadRules/RoadRules.csproj -c Release -o /app/publish --no-restore

# Stage 2: run on the smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RoadRules.dll"]
