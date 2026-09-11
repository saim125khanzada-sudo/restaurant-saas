# Database Schema & Isolation Architecture

## Engine: PostgreSQL 16+
The persistence tier uses managed PostgreSQL. All foreign keys, unique constraints, and check constraints are enforced at the database level.

### Core Normalized Entities
- **Tenancy:** \Restaurants\, \Branches\
- **Identity & RBAC:** \Users\, \Roles\, \Permissions\, \UserRoles\, \RolePermissions\
- **Catalog:** \Categories\, \Products\, \ProductVariants\, \Addons\, \Tables\
- **Orders & POS:** \Orders\, \OrderItems\, \OrderItemAddons\, \OrderStatusHistory\, \Payments\, \CashSettlements\
- **Inventory & Purchasing:** \InventoryItems\, \InventoryTransactions\, \Vendors\, \VendorProducts\
- **Accounting:** \ChartOfAccounts\, \JournalEntries\, \JournalEntryLines\
- **HR & Payroll:** \Employees\, \Attendance\, \PayrollRuns\, \PayrollItems\, \Loans\
- **System:** \AuditLogs\, \SyncOperations\, \Notifications\, \DeviceRegistrations\

### Isolation Guarantees
Every multi-tenant table inherits \IMultiTenantEntity\ and includes an index on \(RestaurantId, CreatedAt DESC)\.
