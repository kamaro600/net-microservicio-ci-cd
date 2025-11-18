-- Crear base de datos simple 
 DROP DATABASE IF EXISTS university_auth;
CREATE DATABASE university_auth;


-- Crear schema si no existe
CREATE SCHEMA IF NOT EXISTS public;

-- Habilitar extensiones necesarias
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";


-- 1. Tabla: roles (formato estándar)
CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(200),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_role_name UNIQUE (name)
);

-- 2. Tabla: users (formato estándar)
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(200) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP,
    CONSTRAINT uk_user_username UNIQUE (username),
    CONSTRAINT uk_user_email UNIQUE (email)
);

-- 3. Tabla: user_roles (relación muchos a muchos)
CREATE TABLE IF NOT EXISTS user_roles (
    user_id INTEGER NOT NULL,
    role_id INTEGER NOT NULL,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
);

-- Índices para performance
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_is_active ON users(is_active);
CREATE INDEX idx_user_roles_user ON user_roles(user_id);
CREATE INDEX idx_user_roles_role ON user_roles(role_id);

-- =============================================
-- INSERTAR DATOS INICIALES
-- =============================================

-- Insertar roles por defecto
INSERT INTO roles (id, name, description) VALUES
(1, 'Admin', 'Administrator with full access'),
(2, 'User', 'Regular user with limited access'),
(3, 'Student', 'Student role for enrollment operations'),
(4, 'Professor', 'Professor role for academic operations');

-- Insertar usuario admin por defecto 
-- Password: "Admin123!" -> Hash BCrypt: $2a$11$7dp2ktHw4Lkwmb21jwO10OGvK6vI/7SBNpBQmAnzbAGox.dyBkoRi
INSERT INTO users (id, username, email, password_hash, first_name, last_name, is_active, created_at) VALUES
(1, 'admin', 'admin@universidad.edu', '$2a$11$7dp2ktHw4Lkwmb21jwO10OGvK6vI/7SBNpBQmAnzbAGox.dyBkoRi', 'Administrator', 'System', true, CURRENT_TIMESTAMP);

-- Crear usuario para Ana García basado en los datos de estudiantes
-- Password: "Admin123!" -> Hash BCrypt: $2a$11$7dp2ktHw4Lkwmb21jwO10OGvK6vI/7SBNpBQmAnzbAGox.dyBkoRi
INSERT INTO users (id, username, email, password_hash, first_name, last_name, is_active, created_at) VALUES
(2, 'ana.garcia', 'ana.garcia@email.com', '$2a$11$7dp2ktHw4Lkwmb21jwO10OGvK6vI/7SBNpBQmAnzbAGox.dyBkoRi', 'Ana', 'García', true, CURRENT_TIMESTAMP);

-- Asignar roles
INSERT INTO user_roles (user_id, role_id, assigned_at) VALUES
(1, 1, CURRENT_TIMESTAMP), -- Admin -> Admin role
(2, 3, CURRENT_TIMESTAMP); -- Ana -> Student role

-- Reiniciar secuencias
SELECT setval('roles_id_seq', (SELECT MAX(id) FROM roles));
SELECT setval('users_id_seq', (SELECT MAX(id) FROM users));