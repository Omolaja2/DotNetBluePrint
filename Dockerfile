# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["DotNetBlueprint.csproj", "./"]
RUN dotnet restore

# Copy the rest of the files and build
COPY . .
RUN dotnet publish "DotNetBlueprint.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage (Using SDK because we need 'dotnet' CLI at runtime)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Render uses 10000 by default or environment variable PORT)
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "DotNetBlueprint.dll"]
