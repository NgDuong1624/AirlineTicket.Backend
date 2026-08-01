# Database Seeding Structure

This directory contains modular SQL seed files for the Airline Ticket System database.

## Directory Structure

```
database/
├── core/                  # Core reference data (required for system operation)
│   ├── users/
│   │   ├── 01_roles.sql
│   │   ├── 02_permissions.sql
│   │   ├── 03_role_permissions.sql
│   │   └── 04_system_admin.sql
│   ├── flights/
│   │   ├── 01_airlines.sql
│   │   ├── 02_airports.sql
│   │   ├── 03_aircraft_models.sql
│   │   └── 04_seat_templates.sql
│   └── notifications/
│       └── 01_notification_templates.sql
├── development/           # Sample data for local development and demo
│   ├── users/
│   │   ├── 01_sample_users.sql
│   │   └── 02_airline_users.sql
│   ├── flights/
│   │   ├── 01_airplanes.sql
│   │   ├── 02_routes.sql
│   │   ├── 03_flights.sql
│   │   └── 04_flight_seats.sql
│   ├── bookings/
│   │   ├── 01_bookings.sql
│   │   ├── 02_passengers.sql
│   │   ├── 03_tickets.sql
│   │   └── 04_payments.sql
│   ├── promotions/
│   │   ├── 01_coupons.sql
│   │   └── 02_campaigns.sql
│   ├── cms/
│   │   ├── 01_categories.sql
│   │   └── 02_articles.sql
│   └── interactions/
│       └── 01_reviews.sql
└── test/                  # Minimal data for integration/unit tests
    └── minimal_data/
        ├── users/
        │   └── 01_test_users.sql
        └── flights/
            └── 01_test_airports.sql
```

## Execution

You can run individual seed scripts depending on your environment:

### 1. Core Data (Required for all environments)
```bash
./scripts/run_seed_core.sh "postgres://user:pass@localhost:5432/airline_ticket"
```

### 2. Development Data (Sample data for local dev/demo)
*Note: Requires Core data to be seeded first.*
```bash
./scripts/run_seed_dev.sh "postgres://user:pass@localhost:5432/airline_ticket"
```

### 3. Test Data (Minimal data for integration/unit tests)
*Note: Requires Core data to be seeded first.*
```bash
./scripts/run_seed_test.sh "postgres://user:pass@localhost:5432/airline_ticket"
```

### 4. Run All (Core + Development)
To apply both Core and Development seeds in one command:
```bash
./run_seeds.sh "postgres://user:pass@localhost:5432/airline_ticket"
```
