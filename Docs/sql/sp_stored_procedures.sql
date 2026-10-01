/*
 * Script de Stored Procedures para Instituto Superior Docente Túpac Amaru
 * Arquitectura: EF Core + Stored Procedures (SOLO Data Access)
 * 
 * REGLAS DE ORO:
 * - SPs SOLO hacen Data Access (SELECT/INSERT/UPDATE/DELETE)
 * - CERO validaciones de negocio en SPs
 * - Validaciones de negocio QUEDAN en los Servicios (BR)
 * - SPs usan SET NOCOUNT ON al inicio
 * - SPs de INSERT usan SCOPE_IDENTITY() para devolver ID
 * - Soft delete = UPDATE Activo = 0 (no DELETE físico)
 * Nomenclatura: sp_{Tabla}_{Operacion}
 */

USE InstitutoDB;
GO

-- ============================================================================
-- TABLA: CARRERAS
-- ============================================================================

-- sp_Carreras_GetAll
CREATE OR ALTER PROCEDURE sp_Carreras_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion
    FROM Carreras
    ORDER BY Id;
END;
GO

-- sp_Carreras_GetById
CREATE OR ALTER PROCEDURE sp_Carreras_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion
    FROM Carreras
    WHERE Id = @Id;
END;
GO

-- sp_Carreras_Create
CREATE OR ALTER PROCEDURE sp_Carreras_Create
    @Nombre NVARCHAR(200),
    @DuracionAnios INT,
    @Turno NVARCHAR(50) = NULL,
    @Modalidad NVARCHAR(50) = NULL,
    @Horario NVARCHAR(100) = NULL,
    @Estado NVARCHAR(50) = 'Activa'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Carreras (Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion)
    VALUES (@Nombre, @DuracionAnios, @Turno, @Modalidad, @Horario, @Estado, GETDATE());
    SELECT SCOPE_IDENTITY() AS Id;
END;
GO

-- sp_Carreras_Update
CREATE OR ALTER PROCEDURE sp_Carreras_Update
    @Id INT,
    @Nombre NVARCHAR(200),
    @DuracionAnios INT,
    @Turno NVARCHAR(50) = NULL,
    @Modalidad NVARCHAR(50) = NULL,
    @Horario NVARCHAR(100) = NULL,
    @Estado NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Carreras
    SET Nombre = @Nombre,
        DuracionAnios = @DuracionAnios,
        Turno = @Turno,
        Modalidad = @Modalidad,
        Horario = @Horario,
        Estado = @Estado
    WHERE Id = @Id;
END;
GO

-- sp_Carreras_Delete
CREATE OR ALTER PROCEDURE sp_Carreras_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Carreras WHERE Id = @Id;
END;
GO

-- sp_Carreras_Exists
CREATE OR ALTER PROCEDURE sp_Carreras_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Carreras WHERE Id = @Id) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- ============================================================================
-- TABLA: ALUMNOS
-- ============================================================================

-- sp_Alumnos_GetAll
CREATE OR ALTER PROCEDURE sp_Alumnos_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad,
           FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion
    FROM Alumnos
    ORDER BY Id;
END;
GO

-- sp_Alumnos_GetById
CREATE OR ALTER PROCEDURE sp_Alumnos_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad,
           FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion
    FROM Alumnos
    WHERE Id = @Id;
END;
GO

-- sp_Alumnos_Create
CREATE OR ALTER PROCEDURE sp_Alumnos_Create
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @Email NVARCHAR(150),
    @DNI INT,
    @FechaNacimiento DATE,
    @Direccion NVARCHAR(200) = NULL,
    @Nacionalidad NVARCHAR(100) = NULL,
    @FechaInscripcion DATETIME = NULL,
    @Telefono NVARCHAR(50) = NULL,
    @TituloSecundario NVARCHAR(200) = NULL,
    @Turno NVARCHAR(50) = NULL,
    @CarreraId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @FechaInscripcion DATETIME = ISNULL(@FechaInscripcion, GETDATE());
    DECLARE @FechaCreacion DATETIME = GETDATE();
    
    INSERT INTO Alumnos (Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad,
                         FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion)
    VALUES (@Nombre, @Apellido, @Email, @DNI, @FechaNacimiento, @Direccion, @Nacionalidad,
            @FechaInscripcion, @Telefono, @TituloSecundario, @Turno, @CarreraId, GETDATE());
    
    SELECT SCOPE_IDENTITY() AS Id;
END;
GO

-- sp_Alumnos_Update
CREATE OR ALTER PROCEDURE sp_Alumnos_Update
    @Id INT,
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @Email NVARCHAR(150),
    @DNI INT,
    @FechaNacimiento DATE,
    @Direccion NVARCHAR(200) = NULL,
    @Nacionalidad NVARCHAR(100) = NULL,
    @Telefono NVARCHAR(50) = NULL,
    @TituloSecundario NVARCHAR(200) = NULL,
    @Turno NVARCHAR(50) = NULL,
    @CarreraId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Alumnos
    SET Nombre = @Nombre,
        Apellido = @Apellido,
        Email = @Email,
        DNI = @DNI,
        FechaNacimiento = @FechaNacimiento,
        Direccion = @Direccion,
        Nacionalidad = @Nacionalidad,
        Telefono = @Telefono,
        TituloSecundario = @TituloSecundario,
        Turno = @Turno,
        CarreraId = @CarreraId
    WHERE Id = @Id;
END;
GO

-- sp_Alumnos_Delete
CREATE OR ALTER PROCEDURE sp_Alumnos_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Alumnos WHERE Id = @Id;
END;
GO

-- sp_Alumnos_Exists
CREATE OR ALTER PROCEDURE sp_Alumnos_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Alumnos WHERE Id = @Id) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- sp_Alumnos_ExistsByDNI
CREATE OR ALTER PROCEDURE sp_Alumnos_ExistsByDNI
    @DNI INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Alumnos WHERE DNI = @DNI) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- sp_Alumnos_ExistsByEmail
CREATE OR ALTER PROCEDURE sp_Alumnos_ExistsByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Alumnos WHERE Email = @Email) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- sp_Alumnos_ExistsByCarreraId
CREATE OR ALTER PROCEDURE sp_Alumnos_ExistsByCarreraId
    @CarreraId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Alumnos WHERE CarreraId = @CarreraId) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- ============================================================================
-- TABLA: ADMINISTRADORES
-- ============================================================================

-- sp_Administradores_GetAll (solo activos)
CREATE OR ALTER PROCEDURE sp_Administradores_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Apellido, Email, Role, Activo, FechaCreacion
    FROM Administradores
    WHERE Activo = 1
    ORDER BY Id;
END;
GO

-- sp_Administradores_GetById
CREATE OR ALTER PROCEDURE sp_Administradores_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Apellido, Email, Role, Activo, FechaCreacion, PasswordHash
    FROM Administradores
    WHERE Id = @Id AND Activo = 1;
END;
GO

-- sp_Administradores_GetByEmail
CREATE OR ALTER PROCEDURE sp_Administradores_GetByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Apellido, Email, Role, Activo, FechaCreacion, PasswordHash
    FROM Administradores
    WHERE Email = @Email AND Activo = 1;
END;
GO

-- sp_Administradores_Create
CREATE OR ALTER PROCEDURE sp_Administradores_Create
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(255),
    @Role NVARCHAR(50) = 'Admin'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Administradores (Nombre, Apellido, Email, PasswordHash, Role, Activo, FechaCreacion)
    VALUES (@Nombre, @Apellido, @Email, @PasswordHash, @Role, 1, GETDATE());
    SELECT SCOPE_IDENTITY() AS Id;
END;
GO

-- sp_Administradores_Update
CREATE OR ALTER PROCEDURE sp_Administradores_Update
    @Id INT,
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @Email NVARCHAR(150),
    @Role NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Administradores
    SET Nombre = @Nombre,
        Apellido = @Apellido,
        Email = @Email,
        Role = @Role
    WHERE Id = @Id AND Activo = 1;
END;
GO

-- sp_Administradores_UpdatePasswordHash
CREATE OR ALTER PROCEDURE sp_Administradores_UpdatePasswordHash
    @Id INT,
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Administradores
    SET PasswordHash = @PasswordHash
    WHERE Id = @Id;
END;
GO

-- sp_Administradores_SoftDelete
CREATE OR ALTER PROCEDURE sp_Administradores_SoftDelete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Administradores SET Activo = 0 WHERE Id = @Id AND Activo = 1;
END;
GO

-- sp_Administradores_ExistsByEmail
CREATE OR ALTER PROCEDURE sp_Administradores_ExistsByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Administradores WHERE Email = @Email AND Activo = 1) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- sp_Administradores_Count
CREATE OR ALTER PROCEDURE sp_Administradores_Count
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Administradores;
END;
GO

-- ============================================================================
-- TABLA: PROFESORES
-- ============================================================================

-- sp_Profesores_GetAll
CREATE OR ALTER PROCEDURE sp_Profesores_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion
    FROM Profesores
    ORDER BY Id;
END;
GO

-- sp_Profesores_GetById
CREATE OR ALTER PROCEDURE sp_Profesores_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion
    FROM Profesores
    WHERE Id = @Id;
END;
GO

-- sp_Profesores_Create
CREATE OR ALTER PROCEDURE sp_Profesores_Create
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @Email NVARCHAR(150),
    @Telefono NVARCHAR(50) = NULL,
    @Especialidad NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Profesores (Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion)
    VALUES (@Nombre, @Apellido, @Email, @Telefono, @Especialidad, GETDATE());
    SELECT SCOPE_IDENTITY() AS Id;
END;
GO

-- sp_Profesores_Update
CREATE OR ALTER PROCEDURE sp_Profesores_Update
    @Id INT,
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100),
    @Email NVARCHAR(150),
    @Telefono NVARCHAR(50) = NULL,
    @Especialidad NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Profesores
    SET Nombre = @Nombre,
        Apellido = @Apellido,
        Email = @Email,
        Telefono = @Telefono,
        Especialidad = @Especialidad
    WHERE Id = @Id;
END;
GO

-- sp_Profesores_Delete
CREATE OR ALTER PROCEDURE sp_Profesores_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Profesores WHERE Id = @Id;
END;
GO

-- sp_Profesores_Exists
CREATE OR ALTER PROCEDURE sp_Profesores_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Profesores WHERE Id = @Id) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- sp_Profesores_ExistsByEmail
CREATE OR ALTER PROCEDURE sp_Profesores_ExistsByEmail
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Profesores WHERE Email = @Email) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- ============================================================================
-- TABLA: FORMULARIOS
-- ============================================================================

-- sp_Formularios_GetAll
CREATE OR ALTER PROCEDURE sp_Formularios_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion
    FROM Formularios
    ORDER BY Id;
END;
GO

-- sp_Formularios_GetById
CREATE OR ALTER PROCEDURE sp_Formularios_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion
    FROM Formularios
    WHERE Id = @Id;
END;
GO

-- sp_Formularios_Create
CREATE OR ALTER PROCEDURE sp_Formularios_Create
    @Nombre NVARCHAR(200),
    @Estado NVARCHAR(20) = 'Borrador',
    @FechaApertura DATETIME,
    @FechaCierre DATETIME,
    @Descripcion NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Formularios (Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion)
    VALUES (@Nombre, @Estado, @FechaApertura, @FechaCierre, @Descripcion, GETDATE());
    SELECT SCOPE_IDENTITY() AS Id;
END;
GO

-- sp_Formularios_Update
CREATE OR ALTER PROCEDURE sp_Formularios_Update
    @Id INT,
    @Nombre NVARCHAR(200),
    @Estado NVARCHAR(20),
    @FechaApertura DATETIME,
    @FechaCierre DATETIME,
    @Descripcion NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Formularios
    SET Nombre = @Nombre,
        Estado = @Estado,
        FechaApertura = @FechaApertura,
        FechaCierre = @FechaCierre,
        Descripcion = @Descripcion
    WHERE Id = @Id;
END;
GO

-- sp_Formularios_Delete
CREATE OR ALTER PROCEDURE sp_Formularios_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Formularios WHERE Id = @Id;
END;
GO

-- sp_Formularios_Exists
CREATE OR ALTER PROCEDURE sp_Formularios_Exists
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(SELECT 1 FROM Formularios WHERE Id = @Id) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- ============================================================================
-- TABLA: LISTADO (JOIN ALUMNOS + CARRERAS)
-- ============================================================================

-- sp_Listado_GetAll
CREATE OR ALTER PROCEDURE sp_Listado_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        a.Id AS AlumnoId,
        a.Nombre + ' ' + a.Apellido AS NombreCompleto,
        a.DNI,
        a.Email,
        c.Nombre AS Carrera,
        a.Turno,
        CASE 
            WHEN a.FechaNacimiento IS NOT NULL 
            THEN FLOOR(DATEDIFF(DAY, a.FechaNacimiento, GETDATE()) / 365.25)
            ELSE NULL 
        END AS Edad
    FROM Alumnos a
    LEFT JOIN Carreras c ON a.CarreraId = c.Id
    ORDER BY a.Id;
END;
GO

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================

PRINT 'Todos los Stored Procedures creados/actualizados correctamente.';
GO