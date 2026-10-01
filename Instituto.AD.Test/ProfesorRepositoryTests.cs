using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Moq;

namespace Instituto.AD.Test;

[TestClass]
public sealed class ProfesorRepositoryTests
{
    private Mock<IProfesorRepository> _mockRepo = null!;
    private Profesor _testProfesor = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IProfesorRepository>();
        _testProfesor = new Profesor
        {
            Id = 1,
            Nombre = "Carlos",
            Apellido = "López",
            Email = "carlos@test.com",
            Telefono = "11-1234-5678",
            Especialidad = "Programación"
        };
    }

    [TestMethod]
    public void GetAll_ReturnsListOfProfesores()
    {
        // Arrange
        var expected = new List<Profesor> { _testProfesor };
        _mockRepo.Setup(r => r.GetAll()).Returns(expected);

        // Act
        var result = _mockRepo.Object.GetAll();

        // Assert
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void ExistsByEmail_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsByEmail("carlos@test.com")).Returns(true);

        // Act
        var result = _mockRepo.Object.ExistsByEmail("carlos@test.com");

        // Assert
        Assert.IsTrue(result);
    }
}