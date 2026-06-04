FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy toàn bộ mã nguồn
COPY . .

# Restore và Build
RUN dotnet restore "src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj"
RUN dotnet build "src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Api/AirlineTicket.Api/AirlineTicket.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AirlineTicket.Api.dll"]
