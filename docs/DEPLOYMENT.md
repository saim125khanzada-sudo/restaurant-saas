# Deployment & Production Provisioning Guide

## Initial Architecture (~15 Restaurants)
- Cloudflare Pro WAF -> Reverse Proxy (Nginx) -> Single Linux Cloud Host running Docker containers.
- Managed PostgreSQL 16 + Redis.
- Automated CI/CD via GitHub Actions.
