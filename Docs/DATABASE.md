# Base de Datos — InstitutoDB

> Última actualización: 2026-09-28
> Motor: SQL Server 2022
> Instancia: `localhost` (default)
> Usuario: `instituto_user` / `Instituto2026`

---

## 1. Inventario de Tablas

| Tabla | Columnas | Registros | FKs Salientes | FKs Entrantes | Índices Únicos |
|-------|----------|-----------|---------------|---------------|----------------|
| Administradores | 9 | 4 | 0 | 0 | 2 |
| Alumnos | 14 | 4 | 1 | 0 | 2 |
| Carreras | 8 | 4 | 0 | 1 | 1 |
| Formularios | 7 | 2 | 0 | 0 | 1 |
| Profesores | 7 | 3 | 0 | 0 | 2 |

---

## 2. Relaciones (Foreign Keys)

| Nombre FK | Tabla Hija | Columna Hija | Tabla Padre | Columna Padre | ON DELETE | ON UPDATE |
|-----------|------------|--------------|-------------|---------------|-----------|-----------|
| FK_Alumnos_Carreras | Alumnos | CarreraId | Carreras | Id | NO_ACTION | NO_ACTION |

**Diagrama de relaciones:**

```
┌─────────────────┐       ┌─────────────────┐
│    CARRERAS     │       │     ALUMNOS     │
├─────────────────┤       ├─────────────────┤
│ PK Id           │       │ PK Id           │
│ Nombre          │       │ Nombre          │
│ DuracionAnios   │       │ Apellido        │
│ Turno           │       │ Email           │
│ Modalidad       │       │ DNI             │
│ Horario         │       │ FechaNacimiento │
│ Estado          │       │ Direccion       │
│ FechaCreacion   │       │ Nacionalidad    │
└────────┬────────┘       │ FechaInscripcion│
         │ 1:N            │ Telefono        │
         ▼                │ TituloSecundario│
┌─────────────────┐       │ Turno           │
│    (ninguna)    │       │ FK CarreraId ───┘
└─────────────────┘       │ FechaCreacion   │
                          └─────────────────┘
```

---

## 3. Estructura Detallada por Tabla

### 3.1 Administradores

| Columna | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|---------|------|----------|----------|----------|-------------|---------|
| Id | int |  | NO | SÍ | PK |  |
| Nombre | nvarchar | 100 | NO |  |  |  |
| Apellido | nvarchar | 100 | NO |  |  |  |
| Email | nvarchar | 150 | NO |  | UNIQUE (parcial) |  |
| PasswordHash | nvarchar | 255 | NO |  |  |  |
| PasswordTemp | nvarchar | 255 | SÍ |  |  |  |
| Role | nvarchar | 50 | NO |  |  | 'Admin' |
| Activo | bit |  | NO |  |  | 1 |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

**Índices UNIQUE:**
- `PK__Administ__...`  Primary Key sobre `Id`.
- `UQ_Administradores_Email_Activo`  UNIQUE parcial sobre `Email WHERE Activo = 1`.

**Notas:**
- Implementa **soft delete** (borrar = `Activo = 0`).
- El índice UNIQUE parcial permite reutilizar emails de registros inactivos.

---

### 3.2 Alumnos

| Columna | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|---------|------|----------|----------|----------|-------------|---------|
| Id | int |  | NO | SÍ | PK |  |
| Nombre | nvarchar | 100 | NO |  |  |  |
| Apellido | nvarchar | 100 | NO |  |  |  |
| Email | nvarchar | 150 | NO |  | UNIQUE |  |
| DNI | int |  | NO |  | UNIQUE |  |
| FechaNacimiento | datetime |  | NO |  |  |  |
| Direccion | nvarchar | 200 | SÍ |  |  |  |
| Nacionalidad | nvarchar | 100 | SÍ |  |  |  |
| FechaInscripcion | datetime |  | SÍ |  |  | GETDATE() |
| Telefono | nvarchar | 50 | SÍ |  |  |  |
| TituloSecundario | nvarchar | 200 | SÍ |  |  |  |
| Turno | nvarchar | 50 | SÍ |  |  |  |
| CarreraId | int |  | NO |  | **FK**  Carreras.Id |  |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

**Relaciones:** FK_Alumnos_Carreras  Carreras(Id)

---

### 3.3 Carreras

| Columna | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|---------|------|----------|----------|----------|-------------|---------|
| Id | int |  | NO | SÍ | PK |  |
| Nombre | nvarchar | 200 | NO |  |  |  |
| DuracionAnios | int |  | NO |  |  |  |
| Turno | nvarchar | 50 | SÍ |  |  |  |
| Modalidad | nvarchar | 50 | SÍ |  |  |  |
| Horario | nvarchar | 100 | SÍ |  |  |  |
| Estado | nvarchar | 50 | SÍ |  |  | 'Activa' |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

**Nota:** No usa soft delete. Al eliminar, la FK de Alumnos impide la operación si hay alumnos asociados.

---

### 3.4 Formularios

| Columna | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|---------|------|----------|----------|----------|-------------|---------|
| Id | int |  | NO | SÍ | PK |  |
| Nombre | nvarchar | 200 | NO |  |  |  |
| Estado | nvarchar | 50 | NO |  |  | 'Borrador' |
| FechaApertura | datetime |  | NO |  |  |  |
| FechaCierre | datetime |  | NO |  |  |  |
| Descripcion | nvarchar | 500 | SÍ |  |  |  |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

**Estados válidos:** `Borrador`, `Abierto`, `Cerrado`.

---

### 3.5 Profesores

| Columna | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|---------|------|----------|----------|----------|-------------|---------|
| Id | int |  | NO | SÍ | PK |  |
| Nombre | nvarchar | 100 | NO |  |  |  |
| Apellido | nvarchar | 100 | NO |  |  |  |
| Email | nvarchar | 150 | NO |  | UNIQUE |  |
| Telefono | nvarchar | 50 | SÍ |  |  |  |
| Especialidad | nvarchar | 100 | SÍ |  |  |  |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

---

## 4. Datos Actuales (Snapshot 2026-09-27)

### 4.1 Carreras (4 registros)

| Id | Nombre | Duración | Turno | Modalidad | Horario | Estado |
|----|--------|----------|-------|-----------|---------|--------|
| 1 | Tecnicatura Superior en Desarrollo de Software | 3 años | Noche | Presencial | 18:30 a 22:30 | Activa |
| 2 | Tecnicatura Superior en Enfermería | 3 años | Mañana | Presencial | 08:00 a 13:00 | Activa |
| 3 | Tecnicatura Superior en Administración de Empresas | 3 años | Tarde | Mixta | 14:00 a 18:30 | Activa |
| 4 | Profesorado en Educación Inicial | 4 años | Mañana | Presencial | 08:00 a 13:30 | Activa |

### 4.2 Administradores (4 registros, solo 2 activos)

| Id | Nombre | Apellido | Email | Role | Activo | Estado |
|----|--------|----------|-------|------|--------|--------|
| 16 | isaac | Acevedo Rengifo | admin@instituto.edu.ar | Admin | 0 |  Soft delete |
| 17 | isaac | Acevedo Rengifo | admin@tupac.edu.ar | SuperAdmin | 1 |  Activo |
| 20 | Tahiel | Cassata | isa@test.com | Admin | 0 |  Soft delete |
| 21 | Tahiel | Cassata | isa@test.com | Admin | 1 |  Activo |

> **Nota:** El email `isa@test.com` aparece 2 veces (Id 20 inactivo + Id 21 activo).
> Eso es posible gracias al índice UNIQUE parcial `WHERE Activo = 1`.

### 4.3 Alumnos (4 registros)

| Id | Nombre | Apellido | DNI | Turno | CarreraId | Carrera |
|----|--------|----------|-----|-------|-----------|---------|
| 1 | Ana | Martínez | 40123456 | Noche | 1 | Desarrollo de Software |
| 2 | Pedro | López | 41234567 | Noche | 1 | Desarrollo de Software |
| 3 | Lucía | Fernández | 42345678 | Mañana | 2 | Enfermería |
| 4 | Diego | Sánchez | 43456789 | Tarde | 3 | Administración |

### 4.4 Profesores (3 registros)

| Id | Nombre | Apellido | Email | Teléfono | Especialidad |
|----|--------|----------|-------|----------|--------------|
| 1 | Juan | Pérez | juan.perez@tupac.edu.ar | 3814567890 | Programación |
| 2 | María | González | maria.gonzalez@tupac.edu.ar | 3814567891 | Enfermería |
| 3 | Carlos | Rodríguez | carlos.rodriguez@tupac.edu.ar | 3814567892 | Matemáticas |

### 4.5 Formularios (2 registros)

| Id | Nombre | Estado | FechaApertura | FechaCierre |
|----|--------|--------|---------------|-------------|
| 1 | Inscripción 2026 - Primer Cuatrimestre | Cerrado | 2026-09-27 | 2026-09-30 |
| 2 | Inscripción 2026 - Segundo Cuatrimestre | Borrador | 2026-06-01 | 2026-08-31 |

---

## 5. Script para Recrear la Base desde Cero

```sql
-- 
-- CREAR BASE
-- 
CREATE DATABASE InstitutoDB;
GO
USE InstitutoDB;
GO

-- 
-- TABLAS
-- 

CREATE TABLE dbo.Carreras (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(200) NOT NULL,
    DuracionAnios INT NOT NULL,
    Turno         NVARCHAR(50) NULL,
    Modalidad     NVARCHAR(50) NULL,
    Horario       NVARCHAR(100) NULL,
    Estado        NVARCHAR(50) NULL DEFAULT 'Activa',
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Administradores (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Apellido      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(150) NOT NULL,
    PasswordHash  NVARCHAR(255) NOT NULL,
    PasswordTemp  NVARCHAR(255) NULL,
    Role          NVARCHAR(50) NOT NULL DEFAULT 'Admin',
    Activo        BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

-- Índice UNIQUE parcial (solo activos)
CREATE UNIQUE INDEX UQ_Administradores_Email_Activo
ON dbo.Administradores(Email)
WHERE Activo = 1;

CREATE TABLE dbo.Alumnos (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    Nombre           NVARCHAR(100) NOT NULL,
    Apellido         NVARCHAR(100) NOT NULL,
    Email            NVARCHAR(150) NOT NULL UNIQUE,
    DNI              INT NOT NULL UNIQUE,
    FechaNacimiento  DATETIME NOT NULL,
    Direccion        NVARCHAR(200) NULL,
    Nacionalidad     NVARCHAR(100) NULL,
    FechaInscripcion DATETIME NULL DEFAULT GETDATE(),
    Telefono         NVARCHAR(50) NULL,
    TituloSecundario NVARCHAR(200) NULL,
    Turno            NVARCHAR(50) NULL,
    CarreraId        INT NOT NULL,
    FechaCreacion    DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Alumnos_Carreras 
        FOREIGN KEY (CarreraId) REFERENCES dbo.Carreras(Id)
);

CREATE TABLE dbo.Profesores (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Apellido      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(150) NOT NULL UNIQUE,
    Telefono      NVARCHAR(50) NULL,
    Especialidad  NVARCHAR(100) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Formularios (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(200) NOT NULL,
    Estado        NVARCHAR(50) NOT NULL DEFAULT 'Borrador',
    FechaApertura DATETIME NOT NULL,
    FechaCierre   DATETIME NOT NULL,
    Descripcion   NVARCHAR(500) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 
-- USUARIO SQL
-- 
CREATE LOGIN instituto_user 
WITH PASSWORD = 'Instituto2026', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;
GO
CREATE USER instituto_user FOR LOGIN instituto_user;
ALTER ROLE db_owner ADD MEMBER instituto_user;
GO
```

---

## 6. Comandos Útiles de Mantenimiento

**Ver todos los admins (activos e inactivos):**
```sql
SELECT Id, Nombre, Apellido, Email, Role, Activo, FechaCreacion 
FROM dbo.Administradores 
ORDER BY Id;
```

**Solo admins activos (los que ve el frontend):**
```sql
SELECT Id, Nombre, Apellido, Email, Role 
FROM dbo.Administradores 
WHERE Activo = 1
ORDER BY Id;
```

**Restaurar un admin soft-deleted:**
```sql
UPDATE dbo.Administradores SET Activo = 1 WHERE Id = 16;
```

**Borrar definitivamente los inactivos:**
```sql
DELETE FROM dbo.Administradores WHERE Activo = 0;
```

**Verificar índices UNIQUE de una tabla:**
```sql
SELECT i.name, i.is_unique, i.has_filter, i.filter_definition
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.Administradores') AND i.is_unique = 1;
```

---

## 7. Historial de Cambios de la BD

| Fecha | Cambio | Autor |
|-------|--------|-------|
| 2026-09-27 | Creación de la BD con 5 tablas | Equipo |
| 2026-09-27 | Habilitado **Mixed Mode Auth** en SQL Server | Equipo |
| 2026-09-27 | Creado usuario SQL `instituto_user` con `db_owner` | Equipo |
| 2026-09-27 | Implementado **soft delete** en Administradores | Equipo |
| 2026-09-27 | Reemplazado UNIQUE total por **UNIQUE parcial** (`WHERE Activo = 1`) | Equipo |
| 2026-09-27 | Datos de prueba insertados (4 carreras, 4 alumnos, 3 profesores, 2 formularios) | Equipo |
| 2026-09-28 | Documentación actualizada (diagrama ASCII, contraseñas, estructura interfaces) | Equipo |

---

## 8. Contraseñas de Prueba (Desarrollo)

| Email | Contraseña | Rol | Activo |
|-------|------------|-----|--------|
| `admin@tupac.edu.ar` | `Tupac123` | SuperAdmin |  |
| `isa@test.com` | (definida al crear) | Admin |  |
| `admin@instituto.edu.ar` | (histórica) | Admin |  |

>  **Antes de producción:** eliminar todos los usuarios de prueba y crear nuevos con contraseñas seguras.

---

## 9. Estadísticas

| Métrica | Valor |
|---------|-------|
| Total tablas | 5 |
| Total columnas | 45 |
| Total registros | 17 |
| FKs definidas | 1 |
| Índices UNIQUE | 7 |
| Índices UNIQUE parciales | 1 (`UQ_Administradores_Email_Activo`) |