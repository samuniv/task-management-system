-- Initialize the database for Task Management System
-- This script is automatically executed when the MySQL container starts

-- Create the main database if it doesn't exist
CREATE DATABASE IF NOT EXISTS TasksDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create test database for integration tests
CREATE DATABASE IF NOT EXISTS TasksTestDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create application user (optional, for production environments)
-- CREATE USER IF NOT EXISTS 'app_user'@'%' IDENTIFIED BY 'app_password';
-- GRANT ALL PRIVILEGES ON TasksDb.* TO 'app_user'@'%';
-- GRANT ALL PRIVILEGES ON TasksTestDb.* TO 'app_user'@'%';

-- Flush privileges to ensure all changes take effect
FLUSH PRIVILEGES;

-- Switch to the main database
USE TasksDb;

-- Basic health check table (optional)
CREATE TABLE IF NOT EXISTS health_check (
    id INT AUTO_INCREMENT PRIMARY KEY,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) DEFAULT 'healthy'
);

-- Insert a test record for health checks
INSERT INTO health_check (status) VALUES ('initialized');

-- Display initialization success
SELECT 'Database initialization completed successfully' as message;
