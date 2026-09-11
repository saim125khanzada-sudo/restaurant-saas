# DEVELOPER_ACTIONS.md â€” Human Operational Manual
**Version:** 1.0 â€” Architecture Stage
**Target System:** Multi-Tenant Restaurant Management SaaS Platform

---

## 1. Structure of Developer Responsibilities

In accordance with strict production-readiness principles, the boundary between automated agent implementation and human operational provisioning is defined below:

* **Antigravity Will Do:**
  * Write all backend C# / ASP.NET Core source code, clean architecture project files, EF Core configurations, and migrations.
  * Write Next.js web portals (Super Admin, Restaurant Admin, POS, KDS) and Tailwind UI components.
  * Write Flutter Android applications for Waiters and Riders.
  * Implement SignalR hubs, offline SQLite sync engine, idempotent API handlers, and unit/integration tests.
  * Implement integration adapters/interfaces for External Payment, SMS, Maps, Biometric Attendance, and FBR Tax Authority.
  * Provide Docker compose files, CI/CD GitHub Actions workflows, and deployment documentation.

* **Human Developer (You) Must Do:**
  * Create external cloud and SaaS accounts (Cloudflare, Cloud Provider, Firebase, Google Cloud / Maps, Payment Gateway, Domain Registrar).
  * Generate, safely store, and inject production API keys and database credentials into `.env` / Cloud Secret Manager.
  * Physically configure or provide connection parameters for thermal receipt printers (LAN/USB) and biometric machines.
  * Apply for official FBR tax registration and live API credentials before production deployment.
  * Manage Android app signing keystores and publish APK/AAB builds to internal testing or Google Play.

---

## 2. Phase 0 & Phase 1 Human Operational Checklist

### Action 1: Create Git Repository & Branch Protection
* **What to do:** Create a private GitHub repository for `restaurant-saas`.
* **Why:** Central version control, code review, and automated CI/CD pipeline triggers.
* **Where:** [GitHub New Repository](https://github.com/new)
* **Step-by-Step Instructions:**
  1. Log in to GitHub.
  2. Click **New Repository**.
  3. Name: `restaurant-saas`.
  4. Select **Private**.
  5. Check **Add .gitignore** and choose `.NET`.
  6. Click **Create repository**.
  7. Under **Settings** $\rightarrow$ **Branches**, add branch protection for `main` (require pull request reviews before merging).

### Action 2: Cloudflare & Domain DNS Setup
* **What to do:** Point your production/staging domain nameservers to Cloudflare.
* **Why:** Provides DDoS mitigation, Web Application Firewall (WAF), Edge SSL/TLS, and caching.
* **Where:** Domain Registrar (Namecheap/GoDaddy) & [Cloudflare Dashboard](https://dash.cloudflare.com)
* **Step-by-Step Instructions:**
  1. Add your domain in Cloudflare (e.g., `yourdomain.com`).
  2. Select the Free or Pro plan.
  3. Copy the two Cloudflare nameservers provided.
  4. Log in to your domain registrar and replace default nameservers with Cloudflare's nameservers.
  5. In Cloudflare **SSL/TLS** tab, set mode to **Full (Strict)**.

### Action 3: Provision PostgreSQL and Redis (Local / Dev)
* **What to do:** Install Docker Desktop locally or prepare a managed cloud database.
* **Why:** Required for EF Core migrations, tenant schema verification, and cache storage.
* **Where:** Local Workstation (Docker Desktop) or Cloud Console (DigitalOcean / AWS / Azure).
* **Expected Result:** A valid connection string formatted as:
  `Host=localhost;Port=5432;Database=restaurant_saas_dev;Username=postgres;Password=YOUR_SECURE_PASSWORD;`

### Action 4: Create Firebase Project for Cloud Messaging (FCM)
* **What to do:** Create a Firebase project for push notifications to Waiter and Rider apps.
* **Why:** Waiters need real-time order alerts; Riders need delivery dispatch notices.
* **Where:** [Firebase Console](https://console.firebase.google.com)
* **Step-by-Step Instructions:**
  1. Click **Add Project** $\rightarrow$ Name it `restaurant-saas-notifications`.
  2. Under Project Settings $\rightarrow$ **Service Accounts**, click **Generate new private key**.
  3. Save the JSON file securely (e.g., `firebase-adminsdk.json`). **NEVER commit this to Git.**
  4. In the mobile apps section, register two Android apps:
     * Waiter App: `com.restaurantsaas.waiter`
     * Rider App: `com.restaurantsaas.rider`
  5. Download `google-services.json` for each and keep them ready for Phase 4 and Phase 5.

### Action 5: Set Up Google Maps Platform API Key
* **What to do:** Create a Google Cloud project and enable Maps SDK for Android & Places API.
* **Why:** Required by the Rider app for delivery route navigation, address geocoding, and distance calculations.
* **Where:** [Google Cloud Console](https://console.cloud.google.com)
* **Step-by-Step Instructions:**
  1. Create project: `restaurant-saas-maps`.
  2. Navigate to **APIs & Services** $\rightarrow$ **Library**.
  3. Enable:
     * Maps SDK for Android
     * Geocoding API
     * Distance Matrix API
  4. Go to **Credentials** $\rightarrow$ **Create Credentials** $\rightarrow$ **API Key**.
  5. Restrict the API key by Android app package name and SHA-1 fingerprint for mobile, and by HTTP referrer for web.

---

## 3. Required Environment Secrets Matrix (`.env.example`)

The following variables must be maintained in `.env` (locally) or Cloud Secrets Manager (production):

```bash
# Database & Cache
DATABASE_CONNECTION_STRING="Host=localhost;Port=5432;Database=restaurant_saas_dev;Username=postgres;Password=ChangeThisPasswordInProduction"
REDIS_CONNECTION_STRING="localhost:6379,password=ChangeThisRedisPassword"

# Security & JWT
JWT_SECRET="Min64CharactersLongSecureRandomHexKeyForJwtTokensSigningProductionUseOnly!"
JWT_ISSUER="RestaurantSaaS.Api"
JWT_AUDIENCE="RestaurantSaaS.Clients"
JWT_ACCESS_EXPIRATION_MINUTES=15
JWT_REFRESH_EXPIRATION_DAYS=7

# Object Storage (S3 / Cloudflare R2)
OBJECT_STORAGE_ENDPOINT="https://<account-id>.r2.cloudflarestorage.com"
OBJECT_STORAGE_ACCESS_KEY="REPLACE_WITH_S3_ACCESS_KEY"
OBJECT_STORAGE_SECRET_KEY="REPLACE_WITH_S3_SECRET_KEY"
OBJECT_STORAGE_BUCKET_NAME="restaurant-saas-assets"

# Push Notifications (Firebase)
FIREBASE_CREDENTIAL_PATH="/path/to/firebase-adminsdk.json"

# Google Maps Platform
GOOGLE_MAPS_API_KEY="AIzaSy...REPLACE_WITH_RESTRICTED_KEY"

# External Integrations (Stubs for Dev, Configured in Phase 7 & 9)
PAYMENT_GATEWAY_API_KEY="REPLACE_WHEN_GATEWAY_SELECTED"
FBR_ENVIRONMENT="Sandbox"
FBR_POS_REGISTRATION_NUMBER="REPLACE_WITH_OFFICIAL_FBR_ID"
FBR_BEARER_TOKEN="REPLACE_WITH_OFFICIAL_FBR_TOKEN"
```

---

## 4. Verification & Testing Instructions for Human Developer
1. **Never commit real secrets:** Run `git status` before every push to ensure no `.json` credential keys or `.env` files are tracked.
2. **Database Connectivity:** Test connection string locally using `psql` or pgAdmin.
3. **Backup Restoration Verification:** In staging and production, scheduled backups must be tested by restoring into an isolated sandbox database instance.

