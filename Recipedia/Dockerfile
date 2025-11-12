# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and project files (adjusted path)
COPY Recipedia/Recipedia.sln ./
COPY Recipedia/Recipedia/Recipedia.csproj ./Recipedia/

# Restore dependencies
RUN dotnet restore "Recipedia/Recipedia.csproj"

# Copy the rest of the app
COPY Recipedia/Recipedia/ ./Recipedia/

# Build and publish
RUN dotnet publish "Recipedia/Recipedia.csproj" -c Release -o /app/publish

# Use Render's dynamic port
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080
ENTRYPOINT ["dotnet", "Recipedia.dll"]
