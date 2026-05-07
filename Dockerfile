# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["ServicesApp.csproj", "./"]
RUN dotnet restore "ServicesApp.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "ServicesApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ServicesApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy the published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Ensure the database file can be written to by the app user if using a non-root user
# For simplicity in this template, we stay as root, but for production consider:
# USER app

ENTRYPOINT ["dotnet", "ServicesApp.dll"]
