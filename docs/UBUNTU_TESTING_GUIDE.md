# Ubuntu Server (Hyper-V) & Mobile Apps Deployment Runbook

This manual guides your functional testing team step-by-step through setting up the local Ubuntu Server VM on Hyper-V, running the multi-tenant SaaS backend + web POS, and configuring the Waiter and Rider Android applications for real-time local network testing.

---

## Part 1: Hyper-V Virtual Machine Network Configuration

By default, Hyper-V uses a Default NAT Switch which isolates your VM behind a private host network. In order for testing phones and tablets on your local Wi-Fi to reach the VM, you must configure an **External Virtual Switch**.

### Step 1.1: Create an External Virtual Switch
1. On your Windows Host machine, open **Hyper-V Manager** as Administrator.
2. In the right-hand **Actions** panel, select **Virtual Switch Manager...**.
3. Under *Create virtual switch*, select **External**, then click **Create Virtual Switch**.
4. Configure the switch:
   * **Name:** `LAN-Bridge-Switch`
   * **Connection type:** Select **External network**.
   * Choose your active network adapter from the dropdown (e.g., *Intel(R) Wi-Fi 6 AX201* or *Realtek PCIe GbE Family Controller*).
   * Ensure **"Allow management operating system to share this network adapter"** is checked.
5. Click **Apply** and then **OK** (your host network may briefly reconnect).

### Step 1.2: Attach Switch to Ubuntu VM
1. In Hyper-V Manager, ensure your Ubuntu VM is turned off.
2. Right-click your Ubuntu VM $\rightarrow$ **Settings...**.
3. Under *Hardware*, click **Network Adapter**.
4. In the **Virtual switch** dropdown, change from *Default Switch* to **`LAN-Bridge-Switch`**.
5. Click **Apply** $\rightarrow$ **OK**, then start the virtual machine.

---

## Part 2: Ubuntu Server Provisioning & Docker Deployment

Log in to your Ubuntu Server terminal via SSH or the Hyper-V console.

### Step 2.1: Find your Ubuntu VM LAN IP Address
Run the command:
```bash
ip a
```
Locate the `inet` IP assigned to your network interface (typically `eth0`, `enp0s3`, or `eth1`).  
*Example:* `192.168.1.150` (referred to as `<UBUNTU_IP>` below).

Test pinging this IP from your Windows machine:
```powershell
ping 192.168.1.150
```

### Step 2.2: Install Docker Engine and Git
```bash
# Update system repositories
sudo apt update && sudo apt upgrade -y

# Install prerequisites
sudo apt install -y ca-certificates curl gnupg lsb-release git ufw

# Set up Docker official GPG key & repository
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker and Compose plugin
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Allow non-root docker execution
sudo usermod -aG docker $USER
newgrp docker
```

### Step 2.3: Configure Firewall (UFW)
Allow testing devices to access the backend API, POS, and database:
```bash
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 80/tcp    # HTTP POS / Web
sudo ufw allow 5000/tcp  # ASP.NET Core API / SignalR Hubs
sudo ufw allow 5432/tcp  # PostgreSQL
sudo ufw allow 6379/tcp  # Redis
sudo ufw --force enable
sudo ufw status
```

### Step 2.4: Clone & Launch Restaurant SaaS Platform
```bash
# Clone the repository
git clone https://github.com/saim125khanzada-sudo/restaurant-saas.git
cd restaurant-saas

# Copy environment configuration
cp .env.example .env

# Start PostgreSQL, Redis, and ASP.NET Core API
docker compose up -d --build

# Verify all containers are healthy
docker compose ps
```

### Step 2.5: Seed Demo Restaurant Data
Once the containers are up, trigger the demo seed endpoint:
```bash
curl -X POST http://localhost:5000/api/v1/auth/seed-demo
```
*Expected Output:* JSON payload returning seeded tenant `The Urban Gourmet Bistro`, branches, tables, menu categories, products, and default test accounts:
* **Admin:** `admin@urbanbistro.com` / `AdminPass123!`
* **Waiter:** `waiter@branch1.com` / `WaiterPass123!`
* **Rider:** `rider@branch1.com` / `RiderPass123!`

---

## Part 3: Accessing Web POS on Any Device in LAN

The repository includes a responsive Web POS client in `src/Frontend/web-pos/index.html`.

You can host this lightweight POS directly on Ubuntu via Python or Nginx:
```bash
cd ~/restaurant-saas/src/Frontend/web-pos
python3 -m http.server 80 &
```
Now, any iPad, tablet, or browser on the same Wi-Fi can open:
`http://<UBUNTU_IP>` (e.g. `http://192.168.1.150`)
* In the top bar, ensure the API status indicator shows green: **Connected (Port 5000)**.
* Cashiers can select tables, tap dishes, customize quantities, and execute cash/card checkout.

---

## Part 4: Waiter & Rider Android Applications Configuration

The mobile apps are built in Flutter with **offline SQLite storage + live SignalR synchronization**.

### Step 4.1: Dynamic Server IP Configuration
Both mobile applications support runtime server IP switching:
- On the Waiter and Rider login screens, tap the **Settings (gear)** icon.
- Enter your Ubuntu VM IP (e.g., `http://192.168.1.150:5000`).
- Tap Save. All API calls and SignalR WebSocket channels immediately redirect to your Ubuntu server without needing to recompile!

### Step 4.2: Build APKs on your Development Machine
Ensure Flutter is installed on your computer (`flutter --version`), then run:
```bash
# Build Waiter APK
cd src/Mobile/waiter_app
flutter pub get
flutter build apk --debug

# The APK will be generated at:
# src/Mobile/waiter_app/build/app/outputs/flutter-apk/app-debug.apk

# Build Rider APK
cd ../rider_app
flutter pub get
flutter build apk --debug

# The APK will be generated at:
# src/Mobile/rider_app/build/app/outputs/flutter-apk/app-debug.apk
```

### Step 4.3: Transfer and Test on Physical Devices
1. Connect test phones to the **same Wi-Fi network** as your Ubuntu VM.
2. Install the APK on the phones via USB cable or download it by sharing the file over local network.
3. Open the **Waiter App**:
   * Login with `waiter@branch1.com` / `WaiterPass123!`.
   * Tap any table (e.g. Table 2) $\rightarrow$ Add Burger + Cheese Add-on $\rightarrow$ Tap **"Send Order to Kitchen"**.
   * Check Web POS / Kitchen display: The order appears instantly without manual refresh.
4. Open the **Rider App**:
   * Login with `rider@branch1.com` / `RiderPass123!`.
   * View dispatched delivery tasks $\rightarrow$ Tap **"Accept"** $\rightarrow$ Tap **"Pickup"** $\rightarrow$ Tap **"Navigate"** (opens Google Maps) $\rightarrow$ Complete Delivery with cash collection.

---

## Part 5: Functional Team End-to-End Test Matrix

| # | Test Scenario | Steps | Expected Result |
|---|---------------|-------|-----------------|
| **1** | **Multi-Tenancy Isolation** | Query orders using Restaurant A token vs Restaurant B header | Server rejects unauthorized cross-tenant requests with `403 Forbidden` / empty set. |
| **2** | **Offline Order Taking** | Turn phone on Airplane Mode; place order on Waiter App | App saves order in local SQLite Outbox; UI alerts "Saved Offline". |
| **3** | **Reconnection & Sync** | Reconnect Wi-Fi on Waiter phone | Sync engine fires batch sync; server responds `200 OK`; order appears on POS. |
| **4** | **Rider Cash Settlement** | Rider marks COD order delivered and submits cash reconciliation | Order moves to `DELIVERED`, cash ledger updates, manager approves settlement. |
| **5** | **FBR Tax Breakdown** | POS checkout with Card vs Cash | Card calculates 5% tax; Cash calculates 15% tax; generates verified invoice code. |
