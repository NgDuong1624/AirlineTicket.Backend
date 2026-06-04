#!/bin/bash
cd /home/thaiduong/Project/AirlineTicket/AirlineTicket.Backend

echo "Generating and applying migrations for Flights..."
~/.dotnet/tools/dotnet-ef migrations add InitialFlights -p src/Modules/v1/Flights/AirlineTicket.Modules.Flights.Infrastructure/AirlineTicket.Modules.Flights.Infrastructure.csproj -s src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --context FlightDbContext -o Data/Migrations
~/.dotnet/tools/dotnet-ef database update -p src/Modules/v1/Flights/AirlineTicket.Modules.Flights.Infrastructure/AirlineTicket.Modules.Flights.Infrastructure.csproj -s src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --context FlightDbContext

echo "Generating and applying migrations for Bookings..."
~/.dotnet/tools/dotnet-ef migrations add InitialBookings -p src/Modules/v1/Bookings/AirlineTicket.Modules.Bookings.Infrastructure/AirlineTicket.Modules.Bookings.Infrastructure.csproj -s src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --context BookingDbContext -o Data/Migrations
~/.dotnet/tools/dotnet-ef database update -p src/Modules/v1/Bookings/AirlineTicket.Modules.Bookings.Infrastructure/AirlineTicket.Modules.Bookings.Infrastructure.csproj -s src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --context BookingDbContext

echo "Generating and applying migrations for Users..."
~/.dotnet/tools/dotnet-ef migrations add InitialUsers -p src/Modules/v1/Users/AirlineTicket.Modules.Users.Infrastructure/AirlineTicket.Modules.Users.Infrastructure.csproj -s src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --context UserDbContext -o Data/Migrations
~/.dotnet/tools/dotnet-ef database update -p src/Modules/v1/Users/AirlineTicket.Modules.Users.Infrastructure/AirlineTicket.Modules.Users.Infrastructure.csproj -s src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --context UserDbContext

echo "Done"

echo "Generating and applying migrations for Promotions..."
~/.dotnet/tools/dotnet-ef migrations add InitialPromotions -p src/Modules/v1/Promotions/AirlineTicket.Modules.Promotions.Infrastructure/AirlineTicket.Modules.Promotions.Infrastructure.csproj -s src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --context PromotionDbContext -o Data/Migrations
~/.dotnet/tools/dotnet-ef database update -p src/Modules/v1/Promotions/AirlineTicket.Modules.Promotions.Infrastructure/AirlineTicket.Modules.Promotions.Infrastructure.csproj -s src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj --context PromotionDbContext
