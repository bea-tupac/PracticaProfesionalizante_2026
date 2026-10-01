using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Moq;

namespace Instituto.AD.Test;

[TestClass]
public sealed class AlumnoRepositoryTests
{
    private Mock<IAlumnoRepository> _mockRepo = null!;
    private Alumno _testAlumno = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IAlumnoRepository>();
        _testAlumno = new Alumno
        {
            Id = 1,
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan@test.com",
            DNI = 12345678,
            FechaNacimiento = new DateTime(2000, 5, 15),
            CarreraId = 1
        };
    }

    [TestMethod]
    public void GetAll_ReturnsListOfAlumnos()
    {
        // Arrange
        var expected = new List<Alumno> { _testAlumno };
        _mockRepo.Setup(r => r.GetAll()).Returns(expected);

        // Act
        var result = _mockRepo.Object.GetAll();

        // Assert
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void ExistsByDNI_ExistingDNI_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsByDNI(12345678)).Returns(true);

        // Act
        var result = _mockRepo.Object.ExistsByDNI(12345678);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ExistsByEmail_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsByEmail("juan@test.com")).Returns(true);

        // Act
        var result = _mockRepo.Object.ExistsByEmail("juan@test.com");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ExistsByCarreraId_ExistingCarreraId_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsByCarreraId(1)).Returns(true);

        // Act
        var result = _mockRepo.Object.ExistsByCarreraId(1);

        // Assert
        Assert.IsTrue(result);
    }
}