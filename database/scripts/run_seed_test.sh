#!/bin/bash
# Run TEST seed files only (minimal data for integration/unit tests)
# NOTE: Requires CORE seeds first -> run ./run_seed_core.sh <conn> before this
# Usage: ./run_seed_test.sh <connection_string>
# Example: ./run_seed_test.sh "postgres://user:pass@localhost:5432/airline_ticket"

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

TEST_FILES=(
  "minimal_data/users/01_test_users.sql"
  "minimal_data/flights/01_test_airports.sql"
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

for f in "${TEST_FILES[@]}"; do
  run_sql "$(dirname "$0")/../test/$f"
done

echo "Test seeds applied successfully"
