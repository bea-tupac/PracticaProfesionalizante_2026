# Esquema de Base de Datos

> **Nota**: El archivo `CreateDatabase.sql` **no existe en el repositorio**. El DDL completo está documentado en [`02-script-creacion.md`](./02-script-creacion.md) y en [`docs/DATABASE.md`](../../DATABASE.md).

## Diagrama Entidad-Relación

```
┌─────────────────┐       ┌─────────────────┐
│  ADMINISTRADORES │       │    CARRERAS     │
├─────────────────┤       ├─────────────────┤
│ PK Id           │       │ PK Id           │
│ Nombre          │       │ Nombre          │
│ Apellido        │       │ DuracionAnios   │
│ Email (UQ*)     │       │ Turno           │
│ PasswordHash    │       │ Modalidad       │
│ PasswordTemp    │       │ Horario         │
│ Role            │       │ Estado          │
│ Activo (bit)    │       └────────┬────────┘
│ FechaCreacion   │                │
└─────────────────┘                │ 1:N
                                    ▼
┌─────────────────┐       ┌─────────────────┐
│    ALUMNOS      │       │   PROFESORES    │
├─────────────────┤       ├─────────────────┤
│ PK Id           │       │ PK Id           │
│ Nombre          │       │ Nombre          │
│ Apellido        │       │ Apellido        │
│ Email (UQ)      │       │ Email (UQ)      │
│ DNI (UQ)        │       │ Telefono        │
│ FechaNacimiento │       │ Especialidad    │
│ Direccion       │       └─────────────────┘
│ Nacionalidad    │
│ FechaInscripcion│
│ Telefono        │
│ TituloSecundario│
│ Turno           │
│ FK CarreraId ───┘
│ FechaCreacion   │
└─────────────────┘

┌─────────────────┐
│  FORMULARIOS    │
├─────────────────┤
│ PK Id           │
│ Nombre          │
│ Estado          │ (Borrador/Abierto/Cerrado)
│ FechaApertura   │
│ FechaCierre     │
│ Descripcion     │
│ FechaCreacion   │
└─────────────────┘

(*) UQ parcial: UNIQUE INDEX `UQ_Administradores_Email_Activo` WHERE Activo = 1
```

## Tablas Detalladas (Basado en Código Real: Modelos + Repositorios)

### 1. Administradores

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

**Índices**:
- PK: `Id` (clustered)
- **UNIQUE NONCLUSTERED parcial**: `UQ_Administradores_Email_Activo` sobre `Email WHERE Activo = 1`
  - Permite reutilizar emails de admins inactivos (soft delete)

**Notas**:
- Implementa **soft delete** (borrar lógico = `Activo = 0`)
- `PasswordTemp` no se usa actualmente en el código (campo heredado)
- Email se normaliza a lowercase en repositorio/servicio antes de guardar

---

### 2. Carreras

| Columna | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|---------|------|----------|----------|----------|-------------|---------|
| Id | int |  | NO | SÍ | PK |  |
| Nombre | nvarchar | 200 | NO |  |  |  |
| DuracionAnios | int |  | NO |  | CHECK (1-10) en servicio |  |
| Turno | nvarchar | 50 | SÍ |  |  |  |
| Modalidad | nvarchar | 50 | SÍ |  |  |  |
| Horario | nvarchar | 100 | SÍ |  |  |  |
| Estado | nvarchar | 50 | SÍ |  |  | 'Activa' |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

**Nota**: No usa soft delete. La FK de Alumnos impide borrar si hay alumnos (validado en `CarreraService.Delete`).

---

### 3. Alumnos

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
| CarreraId | int |  | NO |  | **FK** → Carreras.Id |  |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

**Relaciones**: `FK_Alumnos_Carreras` → `Carreras(Id)` ON DELETE NO ACTION

**Índices** (recomendados):
- PK: `Id`
- UNIQUE: `Email`, `DNI`
- NONCLUSTERED: `IX_Alumnos_CarreraId`

**Edad**: Se calcula en C# (`Alumno.Edad` property), no en BD.

---

### 4. Profesores

| Columna | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|---------|------|----------|----------|----------|-------------|---------|
| Id | int |  | NO | SÍ | PK |  |
| Nombre | nvarchar | 100 | NO |  |  |  |
| Apellido | nvarchar | 100 | NO |  |  |  |
| Email | nvarchar | 150 | NO |  | UNIQUE |  |
| Telefono | nvarchar | 50 | SÍ |  |  |  |
| Especialidad | nvarchar | 100 | SÍ |  |  |  |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

**Nota**: Sin soft delete, sin FK a otras tablas.

---

### 5. Formularios

| Columna | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|---------|------|----------|----------|----------|-------------|---------|
| Id | int |  | NO | SÍ | PK |  |
| Nombre | nvarchar | 200 | NO |  |  |  |
| Estado | nvarchar | 50 | NO |  |  | 'Borrador' |
| FechaApertura | datetime |  | NO |  |  |  |
| FechaCierre | datetime |  | NO |  |  |  |
| Descripcion | nvarchar | 500 | SÍ |  |  |  |
| FechaCreacion | datetime |  | NO |  |  | GETDATE() |

**Estados válidos**: `Borrador`, `Abierto`, `Cerrado` (validado en servicio, no CHECK en BD actual)

---

## Reglas de Negocio a Nivel BD (Implementadas en Servicios)

1. **Soft Delete**: `Administradores.Activo = 0` en lugar de DELETE físico
2. **Sin Cascada**: NO hay ON DELETE CASCADE (integridad manual en servicios BR)
3. **Unicidad**: 
   - Email único en Administradores (solo activos), Profesores, Alumnos
   - DNI único en Alumnos
4. **Validaciones en Servicio (BR)**:
   - `DuracionAnios 1-10` (`CarreraService`)
   - `Password min 8 chars` (`AdministradorService`, `AlumnoService` no tiene password)
   - `Estado IN ('Borrador','Abierto','Cerrado')` (`FormularioService`)
5. **Defaults**: `Activo=1`, `FechaCreacion=GETDATE()`, `Estado='Activa'/'Borrador'`
6. **FK**: `Alumnos.CarreraId` → `Carreras.Id` (NO ACTION, validado en servicio)

---

## Diferencias: Documentación Anterior vs Código Real

| Aspecto | Doc Anterior | Código Real |
|---------|--------------|-------------|
| Admin Email UNIQUE | Total | **Parcial** (`WHERE Activo = 1`) |
| Admin PasswordTemp | No documentado | Existe en modelo/tabla |
| Alumno FechaCreacion | SYSDATETIME() | GETDATE() en INSERT |
| Formulario Estado CHECK | En BD | En servicio (`FormularioService`) |
| Carreras CHECK Duracion | En BD | En servicio (`CarreraService`) |
| Script CreateDatabase.sql | En `Instituto.API/Database/` | **No existe en repo** |