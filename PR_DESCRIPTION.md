# PR Title: feat(backend): structure modular monolith and configure CMS, Interactions, Notifications, Logs modules

## Description
This pull request introduces the modular monolith architecture setup for the backend services and fixes project reference issue in CMS Module as well as solution file syntax.

## Changes Made
- Restructured `AirlineTicket.BuildingBlocks` into separate Api, Application, Domain, and Infrastructure libraries.
- Added and configured `CMS`, `Interactions`, `Notifications`, and `Logs` modules inside `v1`.
- Resolved compiler and XML parsing errors in `AirlineTicket.Backend.slnx` (restored matching tags for `/src/Modules/v1/Flights/`).
- Added framework reference (`Microsoft.AspNetCore.App`) and project/package references in `AirlineTicket.Modules.CMS.Api.csproj`.
- Configured module dependency injection extension methods in the API main project (`Program.cs` and `AirlineTicket.Api.csproj`).

## Type of Change
- [ ] Bug fix
- [x] New feature
- [ ] Documentation update
- [ ] Refactoring
- [ ] CI/CD / DevOps config
