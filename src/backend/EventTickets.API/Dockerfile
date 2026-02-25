FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EventTickets.API/EventTickets.API.csproj", "EventTickets.API/"]
COPY ["EventTickets.Core/EventTickets.Core.csproj", "EventTickets.Core/"]
COPY ["EventTickets.Infrastructure/EventTickets.Infrastructure.csproj", "EventTickets.Infrastructure/"]
RUN dotnet restore "EventTickets.API/EventTickets.API.csproj"
COPY . .
WORKDIR "/src/EventTickets.API"
RUN dotnet build "EventTickets.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventTickets.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventTickets.API.dll"]
