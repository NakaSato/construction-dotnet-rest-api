-- Database initialization script for PostgreSQL
-- This script will be executed when the PostgreSQL container starts for the first time

-- Create database if it doesn't exist (PostgreSQL automatically creates the database specified in POSTGRES_DB)
-- Additional setup can be added here if needed

-- Enable UUID extension for GUID support
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create any additional databases or users if needed
-- Example:
-- CREATE DATABASE test_db;
-- CREATE USER app_user WITH PASSWORD 'app_password';
-- GRANT ALL PRIVILEGES ON DATABASE SolarProjectsDb TO app_user;

-- Log successful initialization
SELECT 'Database initialization completed successfully' AS status;
