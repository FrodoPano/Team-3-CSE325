# ------------------------
# Build Stage
# ------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . ./

RUN dotnet publish PasswordAdmin/PasswordAdmin.csproj -c Release -o /app/publish

# ------------------------
# Runtime Stage
# ------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish ./

# Copy SQLite database from PasswordAdmin folder
COPY PasswordAdmin/app.db /app/app.db

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "PasswordAdmin.dll"]