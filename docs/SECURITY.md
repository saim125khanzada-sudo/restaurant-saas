# Security & Threat Mitigation Policy

## 1. Authentication & Session Security
- Passwords hashed using BCrypt (work factor 12).
- JWT Access Tokens have 15-minute lifespan.
- Refresh tokens stored in Redis with SHA-256 hash and revoked on password reset or explicit logout.
- MFA (TOTP) supported for privileged users (Super Admin, Restaurant Admin, Manager).
- Account lockout triggered after 5 consecutive failed attempts.

## 2. Authorization & Tenant Isolation
- Role-Based Access Control (RBAC) enforced via server-side ASP.NET Core Policies.
- Tenant isolation enforced in EF Core DbContext via Global Query Filters.
- Cross-tenant data tampering strictly defended by verifying route parameters against authenticated user scope.
