# Production & Local Deployment Runbook

## Overview
This platform is a high-performance, multi-tenant Restaurant Management SaaS designed to host ~15 restaurant tenants with multiple branches each. It can be run either natively on a Windows 11 workstation or via Docker Compose with zero cloud hosting costs.

---

## 1. Local Native Windows 11 Running (Current Development Mode)

### Prerequisites:
- .NET 9 SDK
- PostgreSQL 18 (Localhost port 5432, user postgres, database estaurant_saas_dev)
- Redis 7 (Localhost port 6379)

### Start the Backend API:
`powershell
$env:PATH = "C:\Program Files\dotnet;C:\Program Files\Git\cmd;$env:USERPROFILE\.dotnet\tools;$env:PATH"
cd src\Backend\RestaurantSaaS.Api
dotnet run --launch-profile http
`
- API Base URL: http://localhost:5000
- Swagger Documentation: http://localhost:5000/swagger
- Real-time Order Hub: ws://localhost:5000/hubs/orders
- Real-time Delivery Hub: ws://localhost:5000/hubs/delivery
- Health Probe: http://localhost:5000/health

---

## 2. Docker Compose Production / Staging Deployment

### To start the entire stack:
`ash
docker compose up -d --build
`

### Services Managed:
1. **postgres** (Port 5432) - Isolated data volume postgres_data
2. **redis** (Port 6379) - In-memory cache & Pub/Sub volume edis_data
3. **api** (Port 5000 -> 8080) - Multi-stage container running ASP.NET Core 9 Web API

### Verifying Stack Health:
`ash
docker compose ps
curl http://localhost:5000/health
`

---

## 3. Database Backup & Disaster Recovery

### Automated Backup Command:
`powershell
pg_dump -U postgres -h localhost -d restaurant_saas_dev -F c -b -v -f "backups/db_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').dump"
`

### Restore Command:
`powershell
pg_restore -U postgres -h localhost -d restaurant_saas_dev -v "backups/db_backup_filename.dump"
`

---

## 4. Mobile Apps Build Runbook (Waiter & Rider)

### Flutter Waiter App:
`powershell
cd src\Mobile\waiter_app
flutter pub get
flutter build apk --release
`

### Flutter Rider App:
`powershell
cd src\Mobile\rider_app
flutter pub get
flutter build apk --release
`

---

## 5. Summary of Built Architectural Features:
- **Phase 0**: Architecture specifications, solution scaffolding, complete documentation suite.
- **Phase 1**: BCrypt Auth, JWT token rotation, 18 RBAC policies, Tenant isolation middleware.
- **Phase 2**: Branch hierarchy, Floor plans, Tables, Catalog (Products, Variants, Addons).
- **Phase 3**: POS state machine, Order processing, ESC/POS printing, SignalR /hubs/orders.
- **Phase 4**: Flutter Waiter App, BLoC pattern, offline SQLite Outbox sync, table map.
- **Phase 5**: Delivery dispatch, Rider tracking, Google Maps intents, COD reconciliation, Flutter Rider App.
- **Phase 6**: Recipe Bill of Materials (BOM), Real-time stock depletion, Vendors, Purchase orders.
- **Phase 7**: Double-entry general ledger, Chart of Accounts, Shift register auditing, Trial Balance.
- **Phase 8**: HR Employee management, Biometric hardware adapter, Attendance, Payroll engine.
- **Phase 9**: Dynamic multi-rate taxation (Cash vs Card), FBR fiscalization adapter, QR generation.
- **Phase 10**: Business intelligence reports, Sales analytics, Rush hour breakdown, SaaS tenant subscriptions.
- **Phase 11**: Cryptographic SHA-256 audit log chaining, Security headers, OWASP hardening.
- **Phase 12**: Multi-stage Dockerfile, Docker Compose stack, GitHub Actions CI/CD pipeline, Runbook.
