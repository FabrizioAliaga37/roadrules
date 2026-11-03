# Stage 1: build with the SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file first so restore is cached when only code changes
COPY RoadRules/RoadRules.csproj RoadRules/
RUN dotnet restore RoadRules/RoadRules.csproj

COPY RoadRules/ RoadRules/
RUN dotnet publish RoadRules/RoadRules.csproj -c Release -o /app/publish --no-restore

# Stage 2: run on the smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RoadRules.dll"]
