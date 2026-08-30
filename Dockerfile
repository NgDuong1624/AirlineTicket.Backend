FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Install Kerberos dependencies for Npgsql/Postgres
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all source code
COPY . .

# Restore and Build
RUN dotnet restore "src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj"
RUN dotnet build "src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj" -c Release -o /app/build/api
RUN dotnet restore "src/Realtime/AirlineTicket.SignalR/AirlineTicket.SignalR.csproj"
RUN dotnet build "src/Realtime/AirlineTicket.SignalR/AirlineTicket.SignalR.csproj" -c Release -o /app/build/signalr
RUN dotnet restore "src/Workers/AirlineTicket.Worker/AirlineTicket.Worker.csproj"
RUN dotnet build "src/Workers/AirlineTicket.Worker/AirlineTicket.Worker.csproj" -c Release -o /app/build/worker

FROM build AS publish
RUN dotnet publish "src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj" -c Release -o /app/publish/api
RUN dotnet publish "src/Realtime/AirlineTicket.SignalR/AirlineTicket.SignalR.csproj" -c Release -o /app/publish/signalr
RUN dotnet publish "src/Workers/AirlineTicket.Worker/AirlineTicket.Worker.csproj" -c Release -o /app/publish/worker

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish/api .
COPY --from=publish /app/publish/signalr ./signalr/
COPY --from=publish /app/publish/worker ./worker/
COPY database/ ./database/

# Set entrypoint to a custom script that runs migrations and seeds
COPY deploy/docker/run_docker_entrypoint.sh .
RUN chmod +x ./run_docker_entrypoint.sh
ENTRYPOINT ["./run_docker_entrypoint.sh"]
