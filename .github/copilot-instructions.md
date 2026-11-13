# KitchenPulse – instructions for AI assistants

## Project
- KitchenPulse is a .NET application for a chain of restaurants, built with Clean Architecture.
- Multi-site: staff and restaurant managers see and manage only their own site;
  area managers and owners see all sites; IT admins manage users and settings.
- Allergens: if a customer has an allergy, unsafe ingredients are removed from the dish.
- Stock: every change is a StockMovement (Sale, Delivery, Waste, StaffMeal,
  StocktakeGain, StocktakeLoss). Deliveries and gains increase stock;
  sales, waste, staff meals and losses reduce it.
- Stocktake: staff enter counts and the system compares them with its calculated stock.
- Fridges: temperature monitoring, with alerts on screen (e.g. a door left open).
- Food-safety assistant (RAG): answers questions like cooking times from documents.

Built / in progress: allergens and stock (Domain), Ingredient persistence (Infrastructure).
Planned (do not build unless asked): fridges, RAG, login and roles.

Design every decision for high data volume and high traffic.

## Tech
- .NET 10, SQL Server, EF Core 10, xUnit.
- Planned (do not add until asked): Angular (frontend), SignalR (live kitchen screen),
  Kafka (fridge sensor events), Redis (caching), Docker and Azure (deployment).

## Layers
- Domain: business rules. Depends on nothing.
- Application: one task per use case: load data, ask the Domain to apply the rules, save changes. Depends on Domain.
- Infrastructure: database code (EF Core, migrations). Depends on Domain.
- Api: HTTP endpoints. Depends on Application; uses Infrastructure only in Program.cs for startup wiring.

## Coding rules
### Domain
- Entity properties have private setters; state changes only through methods.
- Collections are private fields, exposed read-only (IReadOnlyCollection),
  changed only via methods that check the rules (e.g. AddAllergen).
- Invalid input throws ArgumentException with nameof(parameter) as paramName.
- Validate enums with Guard.EnumDefined(value, nameof(value)) from Domain/Common.
- New Ids use Guid.CreateVersion7(), never Guid.NewGuid().
- Store all dates/times as UTC; names end in Utc (e.g. occurredAtUtc).
  Convert to local time only for display.
- Every site-owned record carries a SiteId; empty Guids are rejected.

### Infrastructure
- All EF mapping in IEntityTypeConfiguration classes. No EF attributes in Domain.

## Testing rules
- xUnit. Every Domain rule has at least one test.
- Test names: Method_Scenario_ExpectedResult (e.g. Constructor_EmptySiteId_Throws).
- Several inputs for the same behaviour → [Theory] with [InlineData], not copied tests.
- Shared test data (e.g. SiteId) is a static readonly field at the top of the test class.
- Repeated setup goes in a private helper method (e.g. CreateClassicWithMilkyCheese()).
- Exception tests also assert ParamName.
- Assert the actual result (Assert.Equal on values), not just that something exists.
- Every test must go red if the code it tests is broken.
- Write the tests first and see them fail (red) before writing the code.
- Integration tests use a real SQL Server LocalDB, one fresh database per test (TestDatabase).

## How to work with me
- Before changing anything, explain your plan and wait for my approval.
- Only change code that is part of the task. Don't touch anything I didn't ask for.
- Ask before adding NuGet packages, new projects or patterns (Repository, MediatR, etc.).
- Work in small steps: one class + its tests, or one or two files, then stop and show me.
- Never edit or delete an applied migration; always create a new one.
  Never run Update-Database or change connection strings without asking.
- After each change: build, run all tests, and tell me the result.
- I'm learning: after each change, explain what you did and why, in simple words.