# KitchenPulse

**A kitchen operations platform for busy restaurants: live kitchen screens, allergen safety checks and real-time stock tracking from POS orders.**

Designed and built by a working Head Chef, from the problems I deal with on the line every day.

> **Status:** In active development. The domain layer (the core business rules) is nearly complete and covered by unit tests. The API, database and kitchen screens are next. See the [roadmap](#roadmap).

---

## The problem

In a busy kitchen, three things go wrong again and again:

- **Allergens.** A customer with a milk allergy orders a burger with "no cheese", but the sauce also contains milk. Paper dockets and memory don't catch that.
- **Timing.** Grill and fryer finish at different times, starters and mains get mixed up, and food waits at the pass going cold.
- **Stock.** Nobody knows the real stock level until the weekly count, and when the numbers don't match, there's no way to find out why.

KitchenPulse connects to the restaurant's till and turns every order into clear kitchen screens, allergen warnings and accurate stock levels.

---

## Features

### Built (domain layer)

- **Menu and recipes:** ingredients with allergens, units and storage type; dishes with recipes, prices and the station that cooks them (grill, fryer, front).
- **Orders from the till:** order lines with quantities, removed ingredients ("no cheese") and extras ("+ bacon"), with validation that stops impossible combinations.
- **Allergen safety:**
  - works out the allergens of what is *actually on the plate* after the customer's changes, not just the menu
  - records the customer's allergy on the specific dish it applies to
  - flags exactly which ingredients are unsafe, so the waitress can remove them or offer another dish
- **Kitchen flow:**
  - each item is marked done by the person who cooks it
  - a course is "ready" only when every kitchen item in it is done (drinks don't count)
  - mains are held until the starters have been collected
  - collecting is only allowed once a course is ready, and double taps are ignored
- **Stock tracking:**
  - every stock change (sale, delivery, waste, staff meal, stocktake gain or loss) is recorded as a stock movement
  - each ingredient keeps a running current stock, updated as each movement is applied
  - stock below zero is allowed but flagged, as a sign that something (usually a delivery) wasn't recorded

### Planned

- REST API that receives orders from the till (POS adapter) and a POS simulator for demos
- Live kitchen display screens (grill, fryer, front) updating in real time
- Allergen check at the till before the order is sent, plus a second check in the kitchen
- Deliveries, suppliers, purchase orders, waste log and a locked weekly stocktake with manager approval to reopen
- Fridge temperature monitoring with alerts
- Staff, roles, rota with manager approval, and an audit log
- AI food safety assistant that answers staff questions from the restaurant's own food safety documents and live data

---

## Tech stack

| Area | Now | Planned |
|---|---|---|
| Language and runtime | C#, .NET 10 | |
| Architecture | Clean Architecture (Domain, Application, Infrastructure, API) | |
| Testing | xUnit, test-driven development | Integration tests |
| Back end | | ASP.NET Core Web API |
| Data | | Entity Framework Core, SQL Server |
| Front end | | Angular, Bootstrap 5 |
| Real time and events | | SignalR, Kafka, Redis |
| AI | | Azure OpenAI, Azure AI Search (RAG) |
| Hosting | | Docker, Azure |

---

## Architecture

```
KitchenPulse.Api             HTTP entry point (controllers)
      |
      v
KitchenPulse.Application     use cases: receive an order, record a delivery...
      |
      v
KitchenPulse.Domain          business rules: allergens, kitchen flow, stock
      ^
      |
KitchenPulse.Infrastructure  database and external services
```

The **Domain** project depends on nothing else. All the kitchen rules live there and can be tested without a database, a web server or a screen.

---

## Design decisions

A few choices that shaped the code:

1. **Allergens come from the plate, not the menu.** "Dish allergens minus the removed ingredient's allergens" is unsafe, because another ingredient (like a sauce) may contain the same allergen. So the app rebuilds what's on the plate (recipe minus removed, plus extras) and reads the allergens of each ingredient.
2. **The customer's allergy belongs to the order line, not the whole order.** At a table of four, only one person may have the allergy. Flagging every dish would create false alarms, and false alarms train staff to ignore warnings.
3. **Strict allergy rule.** If a customer declares an allergy, the only options are to remove every unsafe ingredient or choose another dish. "Just a little" is never accepted.
4. **Stock is a diary plus a running total.** Every change is recorded as a stock movement that is never edited, which gives a full history for audits and stocktake differences. Each ingredient also keeps a running total, so reading the stock is instant even after years of history.
5. **Quantities are always positive, and the movement type decides the direction.** Movements are created through named methods (`StockMovement.Sale(...)`, `StockMovement.Delivery(...)`), so a sale can never be recorded as a delivery by mistake.
6. **The domain never trusts the screen.** Even if a button is hidden, the domain still enforces the rule, because the same rules are reached from the till, the kitchen screens and the API.

---

## Running the tests

With the .NET 10 SDK installed:

```bash
git clone https://github.com/binupeter07/kitchenpulse.git
cd kitchenpulse
dotnet test
```

Or open the solution in Visual Studio and run all tests from Test Explorer.

---

## Roadmap

- [ ] **Stage 1a:** Domain: menu, orders, allergen checks, kitchen flow, stock movements (in progress: turning sales into stock movements)
- [ ] **Stage 1b:** Database, API, POS simulator and live kitchen screens
- [ ] **Stage 2:** Deliveries, suppliers, purchase orders, waste and weekly stocktake
- [ ] **Stage 3:** Fridge temperature monitoring, alerts, Kafka and Redis
- [ ] **Stage 4:** Staff, roles, rota and audit log
- [ ] **Stage 5:** AI food safety assistant (RAG), Docker and Azure deployment

---

## Author

**Binu Peter**
[LinkedIn](https://www.linkedin.com/in/binu-peter-050b22250) · [GitHub](https://github.com/binupeter07)

---

© 2026 Binu Peter. All rights reserved. This code is shared for portfolio viewing only.
