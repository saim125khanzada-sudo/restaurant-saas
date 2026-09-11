# Operational Troubleshooting Guide

## 1. SignalR Disconnections
- Ensure Redis backplane is healthy and reachable.
- Verify WebSocket traffic is allowed through Cloudflare WAF.

## 2. Tenant Authorization Mismatch (403 Forbidden)
- Check JWT claims: \RestaurantId\ claim must match the requested resource's tenant scope.
