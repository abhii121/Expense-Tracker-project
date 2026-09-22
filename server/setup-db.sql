-- Run this once against your SQL Server instance to create the database.
-- Example: sqlcmd -S localhost -E -i setup-db.sql
-- (-E uses your current Windows login; drop it and add -U/-P for SQL auth.)

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'expensetracker')
BEGIN
    CREATE DATABASE expensetracker;
END
GO
