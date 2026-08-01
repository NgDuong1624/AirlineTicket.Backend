#!/bin/bash
# Run CORE seed files only (required reference data for any environment)
# Usage: ./run_seed_core.sh <connection_string>
# Example: ./run_seed_core.sh "postgres://user:pass@localhost:5432/airline_ticket"

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

CORE_FILES=(
  "users/01_roles.sql"
  "users/02_permissions.sql"
  "users/03_role_permissions.sql"
  "users/04_system_admin.sql"
  "flights/01_airlines.sql"
  "flights/02_airports.sql"
  "flights/03_aircraft_models.sql"
  "flights/04_seat_templates.sql"
  "notifications/01_notification_templates.sql"
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

for f in "${CORE_FILES[@]}"; do
  run_sql "$(dirname "$0")/../core/$f"
done

echo "Core seeds applied successfully"
