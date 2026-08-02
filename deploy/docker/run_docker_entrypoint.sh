#!/bin/bash
# Custom entrypoint to run migrations and seeds before starting the app
set -e

# If arguments are provided, execute them (e.g., to run SignalR instead of API)
if [ "$#" -gt 0 ]; then
  exec "$@"
fi

echo "=== Database Initialization ==="
# Run migrations and seed core data
dotnet AirlineTicket.Api.dll --migrate --seed-core

# If environment is Development, seed development data
if [ "$ASPNETCORE_ENVIRONMENT" = "Development" ]; then
  echo "Development environment detected. Seeding development data..."
  dotnet AirlineTicket.Api.dll --seed-dev
fi

echo "=== Starting Application ==="
exec dotnet AirlineTicket.Api.dll
