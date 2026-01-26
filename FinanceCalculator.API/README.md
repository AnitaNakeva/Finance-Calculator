# FinanceCalculator.API

ASP.NET Core 8 REST API for financial calculators (credit, leasing, refinance) with JWT auth, per-user history, favorites, and admin endpoints. Uses SQLite by default.

## Run
```bash
cd FinanceCalculator.API/FinanceCalculator.API
# change port in Properties/launchSettings.json if needed
dotnet run
```
Swagger (Development): `http://localhost:5195/swagger`

## Project structure
- `Models/` now contains per-feature folders (`Auth`, `Credit`, `Refinance`, `Leasing`, `History`, `Favorites`, `Common`) to keep related DTOs/entities grouped.

## Configuration
`appsettings.json` / `appsettings.Development.json`:
- `ConnectionStrings:DefaultConnection` = `Data Source=identifier.sqlite`
- `Jwt:Key` (replace for production), `Jwt:Issuer`, `Jwt:Audience`
- Logging, AllowedHosts

## Authentication
- Register: `POST /api/auth/register` → returns JWT. First user becomes Admin; others are User.
- Login: `POST /api/auth/login` → JWT.
- Logout: `POST /api/auth/logout` → revokes current token.
- Roles: Admin/User (claim `role`).  
- Swagger: click **Authorize** → `Bearer <token>`.

## Calculators (JWT required)

### 1) Credit calculator  
`POST /api/credit/calculate`

**Input (`CreditRequest`):** `Principal`, `TermMonths`, `AnnualInterestRate`, `PaymentType` (0 Annuity / 1 Decreasing), `GraceMonths`, `PromoMonths`, `PromoAnnualInterestRate`, initial fees (`ApplicationFee`, `ProcessingFee`, `OtherInitialFees`), monthly fees (`MonthlyManagementFee`, `OtherMonthlyFees`), annual fees (`AnnualManagementFee`, `OtherAnnualFees`).

**Logic:**  
- Annuity: payment = P*r/(1-(1+r)^-n); recalculates when promo/grace ends.  
- Decreasing: fixed principal per month, payment decreases; history shows `MonthlyPayment` as averaged (“Average ...”).  
- Grace: first `GraceMonths` pay interest only; last installment adjusts remaining balance.

**Output (`CreditResponse`):** `MonthlyPayment` (annuity) or averaged (decreasing), `TotalInterest`, fee breakdown (`InitialFeesTotal`, `MonthlyFeesTotal`, `AnnualFeesTotal`, `TotalFees`), `TotalPaid` (installments + fees), `AnnualPercentageRate`, `Schedule[]` (Month, OpeningBalance, Interest, Principal, Payment, ClosingBalance).

### 2) Leasing goods calculator  
`POST /api/leasing-goods/calculate`

**Input (`LeasingGoodsRequest`):** `ItemPrice`, `DownPayment`, `TermMonths`, `MonthlyPayment` (fixed, no interest), `ProcessingFeePercent`.

**Logic:** financed = price – downPayment; fee = financed * percent; totalPaid = downPayment + fee + all installments; overpayment = totalPaid – price; schedule with zero interest, last payment adjusts remaining balance.

**Output (`LeasingGoodsResponce`):** `FinancedAmount`, `ProcessingFeeAmount`, `TotalPaid`, `OverpaymentAmount/Percent`, `Schedule[]` (Principal=Payment, Interest=0).

### 3) Refinance calculator  
`POST /api/refinance/calculate`

**Input (`RefinanceRequest`):** current loan (`CurrentPrincipal`, `CurrentAnnualInterestRate`, `CurrentTermMonths`, `PaymentsMade`); `EarlyRepaymentFeePercent`; new loan `NewAnnualInterestRate`; upfront fees `UpfrontFeesPercent`, `UpfrontFeesFixed`.

**Logic:** remainingMonths = term – paymentsMade; remaining principal via annuity schedule up to paymentsMade; cost to close = remaining schedule + early fee; new loan principal = remaining + upfront fees; new annuity schedule; savings = cost to close – new total paid.

**Output (`RefinaceResponce`):** `RemainingMonths`, `RemainingPrincipal`, `CurrentMonthlyPayment`, `CurrentTotalPaidRemaining`, `EarlyRepaymentFeeAmount`, `CurrentTotalCostToClose`, `NewLoanPrincipal`, `NewMonthlyPayment`, `UpfrontFeesPercentAmount`, `UpfrontFeesFixedAmount`, `NewTotalPaid`, `Savings`, `CurrentRemainingSchedule[]`, `NewLoanSchedule[]`.

All calculators persist a `CalculationRecord` for the current user after a successful calculation.

## History
- `GET /api/calculations/history` — filters: calculationType, from, to, search; sortOrder asc/desc; page/pageSize (default 50, max 200). Returns `CalculationHistoryResponse` with `CalculationHistoryView` (principal, term, rate, paymentType, monthlyPayment — “Average …” for decreasing, totalPaid/interest, etc.).
- `GET /api/calculations/history/{id}` — detailed parsed view for your record.
- `GET /api/calculations/history/export/csv` — same filters, up to 5000 rows.
- `POST /api/calculations/history/{id}/favorite` — create Favorite from a history record (optional `{ "name": "..." }`).

## Favorites
- `GET /api/calculations/favorites`
- `GET /api/calculations/favorites/{id}`
- `PATCH /api/calculations/favorites/{id}` (body `{ "name": "..." }`)
- `DELETE /api/calculations/favorites/{id}`
- Creation only via the history endpoint above.

## Admin (Admin role)
- `GET /api/admin/users`
- `PATCH /api/admin/users/{id}/role` — body `{ "role": "Admin"|"User" }`
- `GET /api/admin/calculations` (`?userId` optional)
- `GET /api/admin/audit` (`?userId` optional)

## Data & database
- SQLite file: `identifier.sqlite`. Auto-created (`EnsureCreated`). If schema changes → delete the file or switch to EF migrations (`dotnet ef migrations add ...; dotnet ef database update`) and replace EnsureCreated with Migrate().
- Tables: Users, CalculationRecords, RevokedTokens, AuditLogs, FavoriteCalculations (FK to Users, cascade delete).

## Tests
- Folder `Tests/`, project `Tests/FinanceCalculator.API.Tests.csproj` (xUnit, FluentAssertions).
- Run:
```bash
dotnet restore Tests/FinanceCalculator.API.Tests.csproj
dotnet test    Tests/FinanceCalculator.API.Tests.csproj
```
- Coverage: credit (annuity, decreasing, promo/grace, 0% interest), leasing (validation, final installment adjust), refinance (remaining months, fees, zero payments made).

## Example requests (curl)
- Register:
```bash
curl -X POST http://localhost:5195/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"P@ssw0rd"}'
```
- Login:
```bash
TOKEN=$(curl -s -X POST http://localhost:5195/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"P@ssw0rd"}' | jq -r .token)
```
- Credit:
```bash
curl -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"Principal":10000,"TermMonths":12,"AnnualInterestRate":8,"PaymentType":0,"GraceMonths":0,"PromoMonths":0,"PromoAnnualInterestRate":0}' \
  http://localhost:5195/api/credit/calculate
```
- History (filters):
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5195/api/calculations/history?calculationType=Credit&sortOrder=desc&page=1&pageSize=20"
```
- Favorite from history:
```bash
curl -X POST -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"My credit"}' \
  http://localhost:5195/api/calculations/history/6/favorite
```

## Notes
- Changing `Jwt:Key` invalidates old tokens.
- If port 5195 is busy: change launchSettings URL or run with `ASPNETCORE_URLS=http://localhost:5000 dotnet run`.
- For production: enable migrations instead of EnsureCreated and set `RequireHttpsMetadata=true` on JWT bearer.
