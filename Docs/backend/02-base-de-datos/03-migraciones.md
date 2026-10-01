# Migraciones y Versionado de Base de Datos

## Estrategia Actual

**No hay framework de migraciones** (EF Core, FluentMigrator, DbUp).  
El proyecto usa **SQL scripts manuales** versionados en Git.

## Flujo de Cambios de Esquema

### 1. Desarrollo Local
```
1. Modificar CreateDatabase.sql (agregar ALTER TABLE, nueva tabla, etc.)
2. Ejecutar script en LocalDB
3. Probar cambios con la API
4. Commit: "feat(db): add column X to table Y"
```

### 2. Producción
```bash
# En servidor de producción
sqlcmd -S "TU_SERVIDOR" -d InstitutoDB -i "CreateDatabase.sql"
```
> El script usa `IF NOT EXISTS` → es **idempotente** (seguro re-ejecutar)

## Patrones para Cambios Comunes

### Agregar Columna (Nullable)
```sql
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('Alumnos') AND name = 'NuevaColumna')
BEGIN
    ALTER TABLE Alumnos ADD NuevaColumna NVARCHAR(100) NULL;
END
```

### Agregar Columna NOT NULL (con default)
```sql
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('Carreras') AND name = 'NuevaColumna')
BEGIN
    ALTER TABLE Carreras ADD NuevaColumna NVARCHAR(50) NOT NULL DEFAULT 'ValorPorDefecto';
END
```

### Crear Índice
```sql
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Alumnos_Email')
BEGIN
    CREATE INDEX IX_Alumnos_Email ON Alumnos(Email);
END
```

### Agregar Foreign Key
```sql
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Nueva_FK')
BEGIN
    ALTER TABLE NuevaTabla 
    ADD CONSTRAINT FK_Nueva_FK 
    FOREIGN KEY (RefId) REFERENCES TablaReferencia(Id);
END
```

### Modificar Check Constraint
```sql
-- Dropear y recrear (SQL Server no permite ALTER CHECK)
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Alumnos_DNI')
    ALTER TABLE Alumnos DROP CONSTRAINT CK_Alumnos_DNI;

ALTER TABLE Alumnos 
ADD CONSTRAINT CK_Alumnos_DNI CHECK (DNI BETWEEN 1000000 AND 999999999);
```

## Versionado Semántico de BD

| Versión | Cambio | Archivo/Commit |
|---------|--------|----------------|
| v1.0.0 | Esquema inicial (4 tablas) | `CreateDatabase.sql` initial |
| v1.1.0 | Agregar `FechaCreacion` a todas las tablas | feat(db): add audit columns |
| v1.2.0 | Índice `IX_Alumnos_Email` | fix(db): add email index for search |

## Buenas Prácticas

| ✅ Hacer | ❌ No Hacer |
|----------|-------------|
| Usar `IF NOT EXISTS` en todo | `DROP TABLE` / `DROP COLUMN` sin backup |
| Agregar columnas `NULL` primero, luego `NOT NULL` | Cambiar tipo de columna existente sin migración de datos |
| Scripts idempotentes (re-ejecutables) | Scripts que fallan si ya se aplicaron |
| Probar en LocalDB antes de commit | Editar BD prod directamente sin script |
| Documentar breaking changes en CHANGELOG | Asumir que `ALTER` es instantáneo en tablas grandes |

## Migración a Framework (Futuro)

Si el proyecto crece, migrar a **EF Core Migrations** o **DbUp**:

```bash
# EF Core (requiere DbContext)
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet ef migrations add InitialCreate
dotnet ef database update

# DbUp (scripts SQL embebidos en assembly)
dotnet add package DbUp
# Program.cs: EnsureDatabase.For.SqlDatabase(connectionString)...
# DeployChanges.To.SqlDatabase(connectionString).WithScriptsEmbeddedInAssembly(...).Build().PerformUpgrade()
```

## Rollback Manual

```sql
-- Ejemplo: revertir adición de columna
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID('Alumnos') AND name = 'ColumnaAEliminar')
BEGIN
    ALTER TABLE Alumnos DROP COLUMN ColumnaAEliminar;
END
```

> **Regla de oro**: Nunca commitear un script que destruya datos sin probar rollback antes.