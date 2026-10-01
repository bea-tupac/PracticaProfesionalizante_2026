using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Instituto.BR.Services;
using Moq;

namespace Instituto.BR.Test;

[TestClass]
public sealed class CarreraServiceTests
{
    private Mock<ICarreraRepository> _mockCarreraRepo = null!;
    private Mock<IAlumnoRepository> _mockAlumnoRepo = null!;
    private ICarreraService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCarreraRepo = new Mock<ICarreraRepository>();
        _mockAlumnoRepo = new Mock<IAlumnoRepository>();
        _service = new CarreraService(_mockCarreraRepo.Object, _mockAlumnoRepo.Object);
    }

    [TestMethod]
    public void Create_ValidCarrera_ReturnsSuccess()
    {
        // Arrange
        var carrera = new Carrera { Nombre = "Programación", DuracionAnios = 3 };
        _mockCarreraRepo.Setup(r => r.Create(It.IsAny<Carrera>())).Returns<Carrera>(c => { c.Id = 1; return c; });

        // Act
        var result = _service.Create(new Carrera { Nombre = "Programación", DuracionAnios = 3 });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("Programación", result.Data!.Nombre);
    }

    [TestMethod]
    public void Create_InvalidDuration_ReturnsFail()
    {
        // Act
        var result = _service.Create(new Carrera { Nombre = "Test", DuracionAnios = 15 });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("La duración debe estar entre 1 y 10 años.", result.Message);
    }

    [TestMethod]
    public void Create_MissingName_ReturnsFail()
    {
        // Act
        var result = _service.Create(new Carrera { Nombre = "", DuracionAnios = 3 });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("El nombre de la carrera es obligatorio.", result.Message);
    }

    [TestMethod]
    public void Delete_CarreraWithAlumnos_ReturnsFail()
    {
        // Arrange
        _mockCarreraRepo.Setup(r => r.Exists(1)).Returns(true);
        _mockAlumnoRepo.Setup(r => r.ExistsByCarreraId(1)).Returns(true);

        // Act
        var result = _service.Delete(1);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("No se puede eliminar la carrera porque tiene alumnos inscriptos.", result.Message);
    }

    [TestMethod]
    public void Delete_CarreraWithoutAlumnos_ReturnsSuccess()
    {
        // Arrange
        _mockCarreraRepo.Setup(r => r.Exists(1)).Returns(true);
        _mockAlumnoRepo.Setup(r => r.ExistsByCarreraId(1)).Returns(false);

        // Act
        var result = _service.Delete(1);

        // Assert
        Assert.IsTrue(result.Success);
    }
}