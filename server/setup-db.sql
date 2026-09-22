-- Run this once as the postgres superuser to create the app's database role and database.
-- Example: psql -U postgres -h localhost -f setup-db.sql

DO
$$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'expense_app') THEN
      CREATE ROLE expense_app WITH LOGIN PASSWORD 'ExpenseApp_Dev_2026!';
   END IF;
END
$$;

SELECT 'CREATE DATABASE expense_tracker OWNER expense_app'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'expense_tracker')\gexec
