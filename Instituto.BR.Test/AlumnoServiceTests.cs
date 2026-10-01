using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Instituto.BR.Services;
using Moq;

namespace Instituto.BR.Test;

[TestClass]
public sealed class AlumnoServiceTests
{
    private Mock<IAlumnoRepository> _mockAlumnoRepo = null!;
    private Mock<ICarreraRepository> _mockCarreraRepo = null!;
    private IAlumnoService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockAlumnoRepo = new Mock<IAlumnoRepository>();
        _mockCarreraRepo = new Mock<ICarreraRepository>();
        _service = new AlumnoService(_mockAlumnoRepo.Object, _mockCarreraRepo.Object);
    }

    [TestMethod]
    public void Create_ValidAlumno_ReturnsSuccess()
    {
        // Arrange
        var alumno = new Alumno
        {
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan@test.com",
            DNI = 12345678,
            FechaNacimiento = new DateTime(2000, 5, 15),
            CarreraId = 1
        };
        _mockCarreraRepo.Setup(r => r.Exists(1)).Returns(true);
        _mockAlumnoRepo.Setup(r => r.ExistsByDNI(12345678)).Returns(false);
        _mockAlumnoRepo.Setup(r => r.ExistsByEmail("juan@test.com")).Returns(false);
        _mockAlumnoRepo.Setup(r => r.Create(It.IsAny<Alumno>())).Returns<Alumno>(a => { a.Id = 1; return a; });

        // Act
        var result = _service.Create(alumno);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
    }

    [TestMethod]
    public void Create_DuplicateDNI_ReturnsFail()
    {
        // Arrange
        _mockCarreraRepo.Setup(r => r.Exists(1)).Returns(true);
        _mockAlumnoRepo.Setup(r => r.ExistsByDNI(12345678)).Returns(true);

        // Act
        var result = _service.Create(new Alumno { Nombre = "Juan", Apellido = "Pérez", Email = "juan@test.com", DNI = 12345678, FechaNacimiento = DateTime.Now, CarreraId = 1 });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Ya existe un alumno con ese DNI.", result.Message);
    }

    [TestMethod]
    public void Create_DuplicateEmail_ReturnsFail()
    {
        // Arrange
        _mockCarreraRepo.Setup(r => r.Exists(1)).Returns(true);
        _mockAlumnoRepo.Setup(r => r.ExistsByDNI(12345678)).Returns(false);
        _mockAlumnoRepo.Setup(r => r.ExistsByEmail("juan@test.com")).Returns(true);

        // Act
        var result = _service.Create(new Alumno { Nombre = "Juan", Apellido = "Pérez", Email = "juan@test.com", DNI = 12345678, FechaNacimiento = DateTime.Now, CarreraId = 1 });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Ya existe un alumno con ese email.", result.Message);
    }

    [TestMethod]
    public void Create_NonExistentCarrera_ReturnsFail()
    {
        // Arrange
        _mockCarreraRepo.Setup(r => r.Exists(99)).Returns(false);

        // Act
        var result = _service.Create(new Alumno { Nombre = "Juan", Apellido = "Pérez", Email = "juan@test.com", DNI = 12345678, FechaNacimiento = DateTime.Now, CarreraId = 99 });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("La carrera con Id 99 no existe.", result.Message);
    }

    [TestMethod]
    public void Create_InvalidDNI_ReturnsFail()
    {
        // Arrange
        _mockCarreraRepo.Setup(r => r.Exists(1)).Returns(true);

        // Act
        var result = _service.Create(new Alumno { Nombre = "Juan", Apellido = "Pérez", Email = "juan@test.com", DNI = 123, FechaNacimiento = DateTime.Now, CarreraId = 1 });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("El DNI debe tener entre 7 y 8 dígitos.", result.Message);
    }
}