# REST API & SignalR Specification

## Base Route: \/api/v1/\
All API routes are versioned, HTTPS-only, and require Bearer JWT authorization unless explicitly marked public (e.g., login, forgot-password).

### Authentication
- \POST /api/v1/auth/login\
- \POST /api/v1/auth/refresh\
- \POST /api/v1/auth/mfa/verify\
- \POST /api/v1/auth/sessions/revoke\

### Real-Time SignalR Hubs
- \/hubs/orders\: Broadcasts real-time events (\ORDER_CREATED\, \ORDER_UPDATED\, \ORDER_READY\, \BILL_PAID\).
- \/hubs/riders\: Broadcasts rider dispatch updates and GPS coordinates.
