#!/bin/bash

echo "Starting EF Core Migrations for AirlineTicket Modules..."

# Export connection string for design-time DbContext factories
CONN="$1"
if [[ "$CONN" == postgres://* ]]; then
  # Convert postgres://user:pass@host:port/db to Host=host;Port=port;Database=db;Username=user;Password=pass
  # Handles passwords containing '@'
  WITHOUT_SCHEME="${CONN#postgres://}"
  USERPASS="${WITHOUT_SCHEME%@*}"
  HOSTPORTDB="${WITHOUT_SCHEME##*@}"
  USER="${USERPASS%%:*}"
  PASS="${USERPASS#*:}"
  HOSTPORT="${HOSTPORTDB%%/*}"
  DB="${HOSTPORTDB#*/}"
  HOST="${HOSTPORT%%:*}"
  PORT="${HOSTPORT##*:}"
  export ConnectionStrings__DefaultConnection="Host=$HOST;Port=$PORT;Database=$DB;Username=$USER;Password=$PASS;"
  PSQL_CONN="host=$HOST port=$PORT dbname=$DB user=$USER password=$PASS"
else
  export ConnectionStrings__DefaultConnection="$CONN"
  PSQL_CONN="$CONN"
fi

# Update Flights Database
echo "Updating Flights database..."
dotnet ef database update --project src/Modules/v1/Flights/AirlineTicket.Modules.Flights.Infrastructure --startup-project src/Api/AirlineTicket.Api -c FlightDbContext

# Update Bookings Database
echo "Updating Bookings database..."
dotnet ef database update --project src/Modules/v1/Bookings/AirlineTicket.Modules.Bookings.Infrastructure --startup-project src/Api/AirlineTicket.Api -c BookingDbContext

# Update Users Database
echo "Updating Users database..."
dotnet ef database update --project src/Modules/v1/Users/AirlineTicket.Modules.Users.Infrastructure --startup-project src/Api/AirlineTicket.Api -c UserDbContext

# Update Promotions Database
echo "Updating Promotions database..."
dotnet ef database update --project src/Modules/v1/Promotions/AirlineTicket.Modules.Promotions.Infrastructure --startup-project src/Api/AirlineTicket.Api -c PromotionDbContext

# Update Interactions Database
echo "Updating Interactions database..."
dotnet ef database update --project src/Modules/v1/Interactions/AirlineTicket.Modules.Interactions.Infrastructure --startup-project src/Api/AirlineTicket.Api -c InteractionDbContext

# Update CMS Database
echo "Updating CMS database..."
dotnet ef database update --project src/Modules/v1/CMS/AirlineTicket.Modules.CMS.Infrastructure --startup-project src/Api/AirlineTicket.Api -c CMSDbContext

# Update Notifications Database
echo "Updating Notifications database..."
dotnet ef database update --project src/Modules/v1/Notifications/AirlineTicket.Modules.Notifications.Infrastructure --startup-project src/Api/AirlineTicket.Api -c NotificationDbContext

# Update Logs Database
echo "Updating Logs database..."
dotnet ef database update --project src/Modules/v1/Logs/AirlineTicket.Modules.Logs.Infrastructure --startup-project src/Api/AirlineTicket.Api -c LogsDbContext

echo "EF Core Migrations completed successfully!"

# Optional: Seed Core Data if connection string is provided
if [ -n "$CONN" ]; then
  echo "Seeding Core Data..."
  ./database/scripts/run_seed_core.sh "$PSQL_CONN"
else
  echo "To seed core data automatically, run: ./run_ef.sh <connection_string>"
fi