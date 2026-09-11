# System Architecture Specification

## 1. Architectural Style: Modular Monolith
The backend is structured as an ASP.NET Core (.NET 10 LTS-compatible) Modular Monolith.
Microservices are avoided to eliminate premature distributed transaction complexity, network serialization, and operational overhead while maintaining strict internal domain boundaries.

### Domain Bounded Modules
1. **Identity & Multi-Tenancy Module:** User accounts, password hashing (BCrypt), JWT access/refresh token rotation, MFA, role assignments, tenant resolution.
2. **Restaurant & Branch Module:** Tenant profile, branding, branch settings, business hours, floor & table layouts.
3. **Menu & Catalog Module:** Categories, items, variations, modifiers, add-ons, branch availability.
4. **Order & POS Engine Module:** Server-authoritative state machine, dine-in, takeaway, delivery, ticket generation, KDS integration.
5. **Offline Sync & Reconciliation:** Idempotency checking, outbox queue processing, client operation deduplication.
6. **Inventory & Procurement Module:** Transactional stock movements (Opening, Purchase, Consumption, Waste, Adjustment, Transfer), vendor catalog, purchase orders.
7. **Accounting & Finance Module:** Double-entry ledger (Chart of Accounts, Journal Entries, Receivables, Payables, Cash Registers, Financial Statements).
8. **HR, Attendance & Payroll Module:** Employee records, biometric attendance adapter, salary contracts, payroll calculation runs, loan ledgers.
9. **Taxation & Fiscal Module:** Tax configuration rules, monthly reporting, abstracted FBR fiscalization adapter.

## 2. Multi-Tenancy Isolation
* **Strategy:** Shared PostgreSQL Database, Shared Schema with discriminator columns (\RestaurantId\, \BranchId\).
* **Zero-Trust Client Authorization:** \RestaurantId\ and \BranchId\ supplied in HTTP request payloads or headers are NEVER trusted as authorization proofs. The backend resolves the permitted scope strictly from the authenticated identity and user claims.
* **EF Core Global Filters:** Enforces \WHERE RestaurantId = @CurrentTenant\ on all queries.
