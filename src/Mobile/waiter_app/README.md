# Multi-Tenant Waiter Mobile Application

## Architecture Overview
The Waiter Mobile Application is an Android-optimized Flutter application designed for high-concurrency table order-taking, live table status visualization, kitchen firing (KOT), and resilient offline synchronization.

### 1. State Management: BLoC (Business Logic Component)
- **`AuthBloc`**: Manages staff credential authentication, secure JWT storage via `flutter_secure_storage` (Android Keystore), and active tenant resolution (`X-Restaurant-Id`).
- **`TableBloc`**: Manages interactive dining floor sections and live table occupancy statuses (`Available`, `Occupied`, `Reserved`, `Billed`). Automatically synchronizes state changes via ASP.NET Core SignalR.
- **`CatalogBloc`**: Loads categories, menu products, variants, and addons. Employs a dual-tier strategy: online HTTP fetch with local SQLite cache fallback.
- **`OrderCartBloc`**: Manages table order drafting, quantity increments, variant selection, addons/modifiers, and kitchen firing (`FireKotToKitchen`).

### 2. Offline-First Resilience & Outbox Pattern
Floor Wi-Fi in busy restaurants can experience intermittent signal drops or interference. To guarantee **zero lost orders**:
- Orders created while offline or during connection drops are caught by `SyncEngine`.
- The draft order is serialized with a cryptographically unique `idempotencyKey` (UUID v4) and inserted into the local SQLite `outbox_orders` table.
- A background synchronization timer (`startPeriodicSync`) retries pending orders against `/api/v1/orders`.
- The server's idempotency middleware ensures that if a packet was partially processed before a disconnection, retries will never create duplicate tickets or bill the customer twice.

### 3. Real-Time Kitchen Synchronization
- Uses `SignalRService` connected to `/hubs/orders`.
- Subscribes to `OrderStatusUpdated` and `TableStatusUpdated` events scoped strictly to the waiter's authenticated `tenant_{restaurantId}_branch_{branchId}` group.
- Alert notifications are fired when kitchen stations mark prep items as "Ready".

### 4. Running the App Locally (When Flutter SDK is installed)
```bash
cd src/Mobile/waiter_app
flutter pub get
flutter run
```
