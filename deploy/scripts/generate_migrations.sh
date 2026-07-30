#!/bin/bash

# Generate Migrations
echo "Generating Migrations..."

# Flights
dotnet ef migrations add InitialPostgres --project src/Modules/v1/Flights/AirlineTicket.Modules.Flights.Infrastructure --startup-project src/Api/AirlineTicket.Api -c FlightDbContext

# Bookings
dotnet ef migrations add InitialPostgres --project src/Modules/v1/Bookings/AirlineTicket.Modules.Bookings.Infrastructure --startup-project src/Api/AirlineTicket.Api -c BookingDbContext

# Users
dotnet ef migrations add InitialPostgres --project src/Modules/v1/Users/AirlineTicket.Modules.Users.Infrastructure --startup-project src/Api/AirlineTicket.Api -c UserDbContext

# Promotions
dotnet ef migrations add InitialPostgres --project src/Modules/v1/Promotions/AirlineTicket.Modules.Promotions.Infrastructure --startup-project src/Api/AirlineTicket.Api -c PromotionDbContext

# Interactions
dotnet ef migrations add InitialPostgres --project src/Modules/v1/Interactions/AirlineTicket.Modules.Interactions.Infrastructure --startup-project src/Api/AirlineTicket.Api -c InteractionDbContext

# CMS
dotnet ef migrations add InitialPostgres --project src/Modules/v1/CMS/AirlineTicket.Modules.CMS.Infrastructure --startup-project src/Api/AirlineTicket.Api -c CMSDbContext

# Notifications
dotnet ef migrations add InitialPostgres --project src/Modules/v1/Notifications/AirlineTicket.Modules.Notifications.Infrastructure --startup-project src/Api/AirlineTicket.Api -c NotificationDbContext

# Logs
dotnet ef migrations add InitialPostgres --project src/Modules/v1/Logs/AirlineTicket.Modules.Logs.Infrastructure --startup-project src/Api/AirlineTicket.Api -c LogsDbContext

echo "Migrations generated successfully!"
