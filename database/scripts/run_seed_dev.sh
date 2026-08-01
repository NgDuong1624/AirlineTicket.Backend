#!/bin/bash
# Run DEVELOPMENT seed files only (sample data for local dev/demo)
# NOTE: Requires CORE seeds first -> run ./run_seed_core.sh <conn> before this
# Usage: ./run_seed_dev.sh <connection_string>
# Example: ./run_seed_dev.sh "postgres://user:pass@localhost:5432/airline_ticket"

CONN="$1"
if [ -z "$CONN" ]; then
  echo "Connection string required"
  echo "Usage: $0 <connection_string>"
  exit 1
fi

# Parse connection string if it's a URL to handle passwords with '@'
if [[ "$CONN" == postgres://* ]]; then
  WITHOUT_SCHEME="${CONN#postgres://}"
  USERPASS="${WITHOUT_SCHEME%@*}"
  HOSTPORTDB="${WITHOUT_SCHEME##*@}"
  USER="${USERPASS%%:*}"
  PASS="${USERPASS#*:}"
  HOSTPORT="${HOSTPORTDB%%/*}"
  DB="${HOSTPORTDB#*/}"
  HOST="${HOSTPORT%%:*}"
  PORT="${HOSTPORT##*:}"
  PSQL_CONN="host=$HOST port=$PORT dbname=$DB user=$USER password=$PASS"
else
  PSQL_CONN="$CONN"
fi

DEV_FILES=(
  "users/01_sample_users.sql"
  "users/02_airline_users.sql"
  "flights/01_airplanes.sql"
  "flights/02_routes.sql"
  "flights/03_flights.sql"
  "flights/04_flight_seats.sql"
  "flights/05_bulk_flights.sql"
  "bookings/01_bookings.sql"
  "bookings/02_passengers.sql"
  "bookings/03_tickets.sql"
  "bookings/04_payments.sql"
  "bookings/05_bulk_bookings.sql"
  "promotions/01_coupons.sql"
  "promotions/02_campaigns.sql"
  "cms/01_categories.sql"
  "cms/02_articles.sql"
  "interactions/01_reviews.sql"
)

run_sql() {
  local file="$1"
  echo "Running $file"
  if command -v psql >/dev/null 2>&1; then
    psql "$CONN" -f "$file" || { echo "Failed $file"; exit 1; }
  elif command -v docker >/dev/null 2>&1 && docker ps | grep -q local-postgres; then
    # Fallback to docker if psql is not installed locally
    cat "$file" | docker exec -i local-postgres psql "$CONN" || { echo "Failed $file"; exit 1; }
  else
    echo "Error: psql is not installed and local-postgres docker container is not running."
    exit 1
  fi
}

for f in "${DEV_FILES[@]}"; do
  run_sql "$(dirname "$0")/../development/$f"
done

echo "Development seeds applied successfully"
