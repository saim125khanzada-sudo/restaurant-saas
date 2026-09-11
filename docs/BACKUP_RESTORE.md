# Database Backup & Disaster Recovery Runbook

## 1. Automated Backups
- PostgreSQL managed backups run daily at 02:00 UTC.
- 7-Day Point-in-Time Recovery (PITR) enabled via continuous WAL archiving.

## 2. Manual Backup Command
\\\ash
pg_dump -h localhost -U postgres -d restaurant_saas_dev -F c -b -v -f backup_manual_\.dump
\\\

## 3. Restoration Procedure
1. Create a clean isolated target database: \CREATE DATABASE restaurant_saas_restore;\
2. Execute restore:
\\\ash
pg_restore -h localhost -U postgres -d restaurant_saas_restore -v backup_file.dump
\\\
3. Run verification smoke tests on restored data before routing traffic.
