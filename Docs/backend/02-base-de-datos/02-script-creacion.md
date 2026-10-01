# Script de Creación de Base de Datos

> **⚠️ IMPORTANTE**: El archivo `CreateDatabase.sql` **NO existe en el repositorio** (se eliminó o nunca se committeó). Este documento contiene el DDL completo basado en el código real (Modelos + Repositorios + Servicios).

## DDL Completo (Listo para Ejecutar)

```sql
-- ============================================================
-- InstitutoDB - DDL Completo (Basado en Código Real)
-- Compatible: SQL Server 2016+ / Azure SQL / LocalDB
-- ============================================================

-- 1. CREAR BASE DE DATOS
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'InstitutoDB')
BEGIN
    ALTER DATABASE InstitutoDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE InstitutoDB;
END
GO

CREATE DATABASE InstitutoDB
    COLLATE Latin1_General_CI_AS;
GO

USE InstitutoDB;
GO

-- ============================================================
-- 2. TABLA: Carreras (sin dependencias)
-- ============================================================
CREATE TABLE dbo.Carreras (
    Id             INT IDENTITY(1,1) NOT NULL,
    Nombre         NVARCHAR(200)     NOT NULL,
    DuracionAnios  INT               NOT NULL,
    Turno          NVARCHAR(50)      NULL,
    Modalidad      NVARCHAR(50)      NULL,
    Horario        NVARCHAR(100)     NULL,
    Estado         NVARCHAR(50)      NOT NULL DEFAULT 'Activa',
    FechaCreacion  DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Carreras PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Carreras_Duracion CHECK (DuracionAnios BETWEEN 1 AND 10),
    CONSTRAINT CK_Carreras_Estado CHECK (Estado IN ('Activa','Inactiva','Suspendida'))
);
GO

-- ============================================================
-- 3. TABLA: Administradores (con soft delete + UNIQUE parcial)
-- ============================================================
CREATE TABLE dbo.Administradores (
    Id            INT IDENTITY(1,1) NOT NULL,
    Nombre        NVARCHAR(100)     NOT NULL,
    Apellido      NVARCHAR(100)     NOT NULL,
    Email         NVARCHAR(150)     NOT NULL,
    PasswordHash  NVARCHAR(255)     NOT NULL,
    PasswordTemp  NVARCHAR(255)     NULL,
    Role          NVARCHAR(50)      NOT NULL DEFAULT 'Admin',
    Activo        BIT               NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Administradores PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Administradores_Role CHECK (Role IN ('Admin','SuperAdmin')),
    CONSTRAINT CK_Administradores_Activo CHECK (Activo IN (0,1))
);
GO

-- ÍNDICE UNIQUE PARCIAL (solo admins activos) - CLAVE para soft delete
CREATE UNIQUE INDEX UQ_Administradores_Email_Activo
    ON dbo.Administradores (Email)
    WHERE Activo = 1;
GO

CREATE INDEX IX_Administradores_Activo ON dbo.Administradores (Activo);
GO

-- ============================================================
-- 4. TABLA: Alumnos (FK a Carreras)
-- ============================================================
CREATE TABLE dbo.Alumnos (
    Id               INT IDENTITY(1,1) NOT NULL,
    Nombre           NVARCHAR(100)     NOT NULL,
    Apellido         NVARCHAR(100)     NOT NULL,
    Email            NVARCHAR(150)     NOT NULL,
    DNI              INT               NOT NULL,
    FechaNacimiento  DATE              NOT NULL,
    Direccion        NVARCHAR(200)     NULL,
    Nacionalidad     NVARCHAR(100)     NULL,
    FechaInscripcion DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),
    Telefono         NVARCHAR(50)      NULL,
    TituloSecundario NVARCHAR(200)     NULL,
    Turno            NVARCHAR(50)      NULL,
    CarreraId        INT               NOT NULL,
    FechaCreacion    DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Alumnos PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Alumnos_Email UNIQUE NONCLUSTERED (Email),
    CONSTRAINT UQ_Alumnos_DNI UNIQUE NONCLUSTERED (DNI),
    CONSTRAINT FK_Alumnos_Carreras FOREIGN KEY (CarreraId)
        REFERENCES dbo.Carreras (Id) ON DELETE NO ACTION ON UPDATE CASCADE
);
GO

CREATE INDEX IX_Alumnos_CarreraId ON dbo.Alumnos (CarreraId);
CREATE INDEX IX_Alumnos_FechaInscripcion ON dbo.Alumnos (FechaInscripcion);
GO

-- ============================================================
-- 5. TABLA: Profesores
-- ============================================================
CREATE TABLE dbo.Profesores (
    Id            INT IDENTITY(1,1) NOT NULL,
    Nombre        NVARCHAR(100)     NOT NULL,
    Apellido      NVARCHAR(100)     NOT NULL,
    Email         NVARCHAR(150)     NOT NULL,
    Telefono      NVARCHAR(50)      NULL,
    Especialidad  NVARCHAR(100)     NULL,
    FechaCreacion DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Profesores PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Profesores_Email UNIQUE NONCLUSTERED (Email)
);
GO

-- ============================================================
-- 6. TABLA: Formularios
-- ============================================================
CREATE TABLE dbo.Formularios (
    Id              INT IDENTITY(1,1) NOT NULL,
    Nombre          NVARCHAR(200)     NOT NULL,
    Estado          NVARCHAR(20)      NOT NULL DEFAULT 'Borrador',
    FechaApertura   DATE              NOT NULL,
    FechaCierre     DATE              NOT NULL,
    Descripcion     NVARCHAR(500)     NULL,
    FechaCreacion   DATETIME2(3)      NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Formularios PRIMARY KEY CLUSTERED (Id)
);
GO

-- CHECK para Estados (opcional, validado en servicio)
-- ALTER TABLE dbo.Formularios ADD CONSTRAINT CK_Formularios_Estado
--     CHECK (Estado IN ('Borrador','Abierto','Cerrado'));
GO

-- ============================================================
-- 7. USUARIO SQL PARA AUTENTICACIÓN MIXTA (Development)
-- ============================================================
-- Ejecutar en master:
/*
CREATE LOGIN instituto_user
WITH PASSWORD = 'Instituto2026', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;
GO

USE InstitutoDB;
GO
CREATE USER instituto_user FOR LOGIN instituto_user;
ALTER ROLE db_owner ADD MEMBER instituto_user;
GO
*/

PRINT 'Base de datos InstitutoDB y tablas creadas exitosamente.';
```

---

## Instrucciones de Ejecución

### Opción A: SQL Server Management Studio (SSMS)
1. Abrir SSMS
2. Conectar a tu instancia (ej: `localhost`, `localhost\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`)
3. Archivo → Nuevo → Consulta
4. Pegar el script de arriba
5. Ejecutar (F5)

### Opción B: sqlcmd (línea de comandos)
```bash
# LocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -i create_instituto_db.sql

# SQL Server Express
sqlcmd -S "localhost\SQLEXPRESS" -i create_instituto_db.sql

# Con autenticación SQL
sqlcmd -S "localhost" -U instituto_user -P "Instituto2026" -i create_instituto_db.sql
```

### Opción C: Azure Data Studio / VS Code (extensión mssql)
1. Conectar a tu instancia
2. Nuevo archivo `.sql` → Pegar script → Click derecho → "Run Query"

---

## Connection Strings Reales (appsettings)

```json
// appsettings.Development.json (SQL Auth - usado en desarrollo actual)
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=InstitutoDB;User Id=instituto_user;Password=Instituto2026;TrustServerCertificate=True;"
  }
}
```

```json
// appsettings.json (LocalDB - Windows Auth)
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Verificación Post-Ejecución

```sql
USE InstitutoDB;

-- 1. Tablas creadas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
-- Resultado esperado: Administradores, Alumnos, Carreras, Formularios, Profesores

-- 2. Verificar índice único parcial (soft delete)
SELECT i.name, i.has_filter, i.filter_definition
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.Administradores') AND i.is_unique = 1;

-- 3. Verificar FK
SELECT 
    fk.name AS FK_Name,
    tp.name AS Tabla_Hija,
    cp.name AS Columna_Hija,
    tr.name AS Tabla_Padre,
    cr.name AS Columna_Padre
FROM sys.foreign_keys fk
JOIN sys.tables tp ON tp.object_id = fk.parent_object_id
JOIN sys.tables tr ON tr.object_id = fk.referenced_object_id
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns cp ON cp.object_id = tp.object_id AND cp.column_id = fkc.parent_column_id
JOIN sys.columns cr ON cr.object_id = tr.object_id AND cr.column_id = fkc.referenced_column_id;

-- 4. Estructura Administradores
EXEC sp_help 'dbo.Administradores';
```

---

## Datos de Prueba (Opcional - para desarrollo)

```sql
USE InstitutoDB;

-- Carreras base
INSERT INTO dbo.Carreras (Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado) VALUES
('Tecnicatura Superior en Desarrollo de Software', 3, 'Noche', 'Presencial', '18:30 a 22:30', 'Activa'),
('Tecnicatura Superior en Enfermería', 3, 'Mañana', 'Presencial', '08:00 a 13:00', 'Activa'),
('Tecnicatura Superior en Administración de Empresas', 3, 'Tarde', 'Mixta', '14:00 a 18:30', 'Activa'),
('Profesorado en Educación Inicial', 4, 'Mañana', 'Presencial', '08:00 a 13:30', 'Activa');

-- Admin inicial (password: Tupac123 -> BCrypt hash workFactor 12)
-- Generar con: BCrypt.Net.BCrypt.HashPassword("Tupac123", 12)
INSERT INTO dbo.Administradores (Nombre, Apellido, Email, PasswordHash, Role, Activo)
VALUES ('Super', 'Admin', 'admin@tupac.edu.ar', '$2a$12$...hash...', 'SuperAdmin', 1);
```

---

## Diferencias con Documentación Anterior

| Aspecto | Documentación Antigua | Código Real (Este Script) |
|---------|----------------------|---------------------------|
| Archivo físico | `Instituto.API/Database/CreateDatabase.sql` | **No existe en repo** |
| Instancia SQL | `DESKTOP-DQ8JUA\G` | `localhost` / `(localdb)\MSSQLLocalDB` |
| Auth BD | Windows (Trusted_Connection) | **SQL Auth** (usuario `instituto_user`) |
| Admin Email UNIQUE | Total | **Parcial** (`WHERE Activo = 1`) |
| Admin PasswordTemp | No | **Sí** (columna existe) |
| Formulario Estado CHECK | En BD | **En servicio** (no en BD) |
| Carreras CHECK Duracion | En BD | **En servicio** (no en BD) |
| FechaCreacion default | SYSDATETIME() | SYSDATETIME() (DATETIME2(3)) |