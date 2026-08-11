#!/bin/bash
# Create target database if it doesn't exist
# Usage: ./create_db.sh <connection_string>

CONN="$1"
if [ -z "$CONN" ]; then
  echo "Connection string required"
  echo "Usage: $0 <connection_string>"
  exit 1
fi

if [[ "$CONN" == postgres://* ]]; then
  # Parse postgres://user:password@host:port/database
  # Handles special characters in password by splitting via sed/regex
  USER=$(echo "$CONN" | sed -E 's|^postgres://([^:]+):.*|\1|')
  PASS=$(echo "$CONN" | sed -E 's|^postgres://[^:]+:(.*)@.*|\1|')
  
  # Extract host, port, database from portion after '@'
  HOSTPORTDB=$(echo "$CONN" | sed -E 's|^postgres://[^:]+:.*@(.*)|\1|')
  HOSTPORT="${HOSTPORTDB%%/*}"
  DB="${HOSTPORTDB#*/}"
  
  if [[ "$HOSTPORT" == *:* ]]; then
    HOST="${HOSTPORT%%:*}"
    PORT="${HOSTPORT##*:}"
  else
    HOST="$HOSTPORT"
    PORT="5432"
  fi
elif [[ "$CONN" == *Host=* || "$CONN" == *host=* ]]; then
  # Parse Host=...;Port=...;Database=...;Username=...;Password=...;
  # Case-insensitive matching via grep/sed
  HOST=$(echo "$CONN" | grep -o -i -E 'Host=[^;]+' | cut -d= -f2)
  PORT=$(echo "$CONN" | grep -o -i -E 'Port=[^;]+' | cut -d= -f2)
  DB=$(echo "$CONN" | grep -o -i -E '(Database|Database Name)=[^;]+' | cut -d= -f2)
  USER=$(echo "$CONN" | grep -o -i -E '(Username|User Id|User)=[^;]+' | cut -d= -f2)
  PASS=$(echo "$CONN" | grep -o -i -E 'Password=[^;]+' | cut -d= -f2)

  # Default values if missing
  PORT="${PORT:-5432}"
else
  echo "Error: Connection string must start with postgres:// or be in Host=... format."
  exit 1
fi

if [ -z "$DB" ] || [ -z "$HOST" ] || [ -z "$USER" ]; then
  echo "Error: Failed to parse required connection properties (Host, Database, Username)."
  exit 1
fi

echo "Checking if database '$DB' exists on $HOST:$PORT..."

# Connect to the default 'postgres' database to check and create the target database
EXISTS=$(PGPASSWORD="$PASS" psql -h "$HOST" -p "$PORT" -U "$USER" -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname = '$DB'")

if [ "$EXISTS" = "1" ]; then
  echo "Database '$DB' already exists."
else
  echo "Database '$DB' does not exist. Creating..."
  PGPASSWORD="$PASS" psql -h "$HOST" -p "$PORT" -U "$USER" -d postgres -c "CREATE DATABASE \"$DB\";"
  if [ $? -eq 0 ]; then
    echo "Database '$DB' created successfully."
  else
    echo "Error: Failed to create database '$DB'."
    exit 1
  fi
fi
