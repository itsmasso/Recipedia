# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Recipedia.sln", "./"]
COPY ["Recipedia/Recipedia.csproj", "Recipedia/"]

RUN dotnet restore "Recipedia/Recipedia.csproj"

COPY Recipedia/ Recipedia/
RUN dotnet publish "Recipedia/Recipedia.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Use Render's dynamic port
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080
ENTRYPOINT ["dotnet", "Recipedia.dll"]
