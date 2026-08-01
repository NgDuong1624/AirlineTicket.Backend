#!/bin/bash
# Master script to run all seed files (Core + Development)
# Usage: ./run_seeds.sh <connection_string>
# Example: ./run_seeds.sh "postgres://user:pass@localhost:5432/airline_ticket"

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

DIR="$(dirname "$0")"

echo "=== Running Core Seeds ==="
"$DIR/scripts/run_seed_core.sh" "$PSQL_CONN" || exit 1

echo "=== Running Development Seeds ==="
"$DIR/scripts/run_seed_dev.sh" "$PSQL_CONN" || exit 1

echo "All seeds applied successfully"
