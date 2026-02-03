FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EventTickets.sln .
COPY EventTickets.API/EventTickets.API.csproj EventTickets.API/
COPY EventTickets.Core/EventTickets.Core.csproj EventTickets.Core/
COPY EventTickets.Infrastructure/EventTickets.Infrastructure.csproj EventTickets.Infrastructure/

RUN dotnet restore

COPY . .
RUN dotnet publish EventTickets.API/EventTickets.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "EventTickets.API.dll"]
