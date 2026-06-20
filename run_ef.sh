#!/bin/bash

echo "Starting EF Core Migrations for AirlineTicket Modules..."

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