# DEVELOPER_ACTIONS.md — Human Operational Manual
**Version:** 2.0 — Functional Testing & Deployment Stage  
**Target System:** Multi-Tenant Restaurant Management SaaS Platform

---

## 1. Structure of Developer Responsibilities

In accordance with strict production-readiness principles, the boundary between automated agent implementation and human operational provisioning is defined below:

* **Antigravity Will Do:**
  * Write all backend C# / ASP.NET Core source code, clean architecture project files, EF Core configurations, and migrations.
  * Write Next.js and responsive Web POS with real-time SignalR order processing and FBR fiscal integration.
  * Write Flutter Android applications for Waiters and Riders with offline SQLite outbox sync and dynamic server host discovery.
  * Implement SignalR hubs, offline SQLite sync engine, idempotent API handlers, and unit/integration tests.
  * Provide Docker compose orchestration for PostgreSQL, Redis, ASP.NET Core API, and Nginx Web POS.
  * Provide deployment and testing manuals.

* **Human Developer (You) Must Do:**
  * Create Hyper-V External Switch on Windows host and attach to Ubuntu VM.
  * Run Docker Compose on Ubuntu Server to start the stack.
  * Run Flutter APK builds for Waiter and Rider applications.
  * Provide physical/local Wi-Fi connectivity for mobile testing devices.
  * Create external cloud and SaaS accounts (Cloudflare, Firebase, Google Maps, Payment Gateway, FBR) when migrating from local testing to commercial cloud production.

---

## 2. Local Ubuntu Server & Hyper-V Operational Guide

### Action 1: Configure Hyper-V External Virtual Switch
* **What to do:** Switch the Ubuntu VM network adapter from Default Switch to an External Switch bridged to your physical Wi-Fi/Ethernet.
* **Why:** Default switch places the VM in private host NAT; test mobile phones and tablets on your Wi-Fi will not be able to connect unless bridged to your physical LAN.
* **Where:** Windows Host $\rightarrow$ Hyper-V Manager $\rightarrow$ Virtual Switch Manager.
* **Exact Instructions:**
  1. Open **Hyper-V Manager** as Administrator.
  2. Click **Virtual Switch Manager** on the right panel.
  3. Choose **External** $\rightarrow$ Click **Create Virtual Switch**.
  4. Name: `LAN-Bridge-Switch`. Select your physical Wi-Fi / Ethernet adapter.
  5. Check *"Allow management operating system to share this network adapter"*. Click **Apply** $\rightarrow$ **OK**.
  6. Shut down Ubuntu VM $\rightarrow$ Right-click VM $\rightarrow$ **Settings** $\rightarrow$ **Network Adapter** $\rightarrow$ Select `LAN-Bridge-Switch` $\rightarrow$ **Apply**.
  7. Start the VM.

### Action 2: Provision Ubuntu Server & Launch Containers
* **What to do:** Install Docker on Ubuntu and run Docker Compose.
* **Where:** Ubuntu VM terminal / SSH.
* **Exact Commands:**
  ```bash
  # Check assigned local IP
  ip a
  # (Note your IP, e.g., 192.168.1.150)

  # Install Docker & Compose
  sudo apt update && sudo apt install -y curl git docker.io docker-compose-v2 ufw
  sudo usermod -aG docker $USER
  newgrp docker

  # Allow firewall ports
  sudo ufw allow 22/tcp
  sudo ufw allow 80/tcp
  sudo ufw allow 5000/tcp
  sudo ufw allow 5432/tcp
  sudo ufw reload

  # Clone repo & launch
  git clone https://github.com/saim125khanzada-sudo/restaurant-saas.git
  cd restaurant-saas
  cp .env.example .env
  docker compose up -d --build
  ```

### Action 3: Seed Demo Tenant Data
* **What to do:** Populate database with "The Urban Gourmet Bistro", categories, products, tables, and test accounts.
* **Command:**
  ```bash
  curl -X POST http://localhost:5000/api/v1/auth/seed-demo
  ```
* **Test Credentials Created:**
  * **Super / Restaurant Admin:** `admin@urbanbistro.com` / `AdminPass123!`
  * **Waiter:** `waiter@branch1.com` / `WaiterPass123!`
  * **Rider:** `rider@branch1.com` / `RiderPass123!`

---

## 3. Mobile Apps (Waiter & Rider) Testing Instructions

### Action 1: Build Android APKs
* **What to do:** Generate debug/testing APKs for Android devices.
* **Where:** Local workstation with Flutter SDK.
* **Commands:**
  ```bash
  # Build Waiter App
  cd src/Mobile/waiter_app
  flutter pub get
  flutter build apk --debug

  # Build Rider App
  cd ../rider_app
  flutter pub get
  flutter build apk --debug
  ```
* **Output files:**
  * `src/Mobile/waiter_app/build/app/outputs/flutter-apk/app-debug.apk`
  * `src/Mobile/rider_app/build/app/outputs/flutter-apk/app-debug.apk`

### Action 2: Install and Configure Server Endpoint
1. Transfer APKs to physical Android phones or tablets connected to the **same Wi-Fi**.
2. Open Waiter or Rider App.
3. Tap the **Settings (gear)** icon on the top right.
4. Set Server URL to: `http://<UBUNTU_VM_IP>:5000` (e.g. `http://192.168.1.150:5000`).
5. Tap **Save & Apply**.
6. Sign in with the test accounts.

---

## 4. Environment Secrets Configuration (`.env`)

```bash
# Database & Cache
DATABASE_CONNECTION_STRING="Host=postgres;Port=5432;Database=restaurant_saas_prod;Username=postgres;Password=postgrespassword;"
REDIS_CONNECTION_STRING="redis:6379"

# Security & JWT
JWT_SECRET="ProductionSuperSecretKeyForMultiTenantRestaurantSaaSPlatform2026!MustBeVeryLong"
JWT_ISSUER="RestaurantSaaS.Api"
JWT_AUDIENCE="RestaurantSaaS.Clients"
JWT_ACCESS_EXPIRATION_MINUTES=15
JWT_REFRESH_EXPIRATION_DAYS=7

# Firebase & Google Maps (Optional for Local Test, Required for Cloud Production)
FIREBASE_CREDENTIAL_PATH="/path/to/firebase-adminsdk.json"
GOOGLE_MAPS_API_KEY="AIzaSy...REPLACE_WITH_RESTRICTED_KEY"
```
