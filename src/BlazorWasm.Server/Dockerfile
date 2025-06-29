# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["src/BlazorWasm.Server/BlazorWasm.Server.csproj", "BlazorWasm.Server/"]
COPY ["src/BlazorWasm.Client/BlazorWasm.Client.csproj", "BlazorWasm.Client/"]
COPY ["src/BlazorWasm.Shared/BlazorWasm.Shared.csproj", "BlazorWasm.Shared/"]

RUN dotnet restore "BlazorWasm.Server/BlazorWasm.Server.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/BlazorWasm.Server"
RUN dotnet publish "BlazorWasm.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorWasm.Server.dll"]
