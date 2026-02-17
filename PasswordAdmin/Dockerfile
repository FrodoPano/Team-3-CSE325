# ------------------------
# Build Stage
# ------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all files and restore dependencies
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ------------------------
# Runtime Stage
# ------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published files
COPY --from=build /app/publish ./

# Copy SQLite database file into container
COPY ../app.db /app/app.db

# Set port for Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "PasswordAdmin.dll"]
