using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Moq;

namespace Instituto.AD.Test;

[TestClass]
public sealed class FormularioRepositoryTests
{
    private Mock<IFormularioRepository> _mockRepo = null!;
    private Formulario _testFormulario = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IFormularioRepository>();
        _testFormulario = new Formulario
        {
            Id = 1,
            Nombre = "Inscripción 2026",
            Estado = "Abierto",
            FechaApertura = new DateTime(2026, 1, 15),
            FechaCierre = new DateTime(2026, 3, 31),
            Descripcion = "Formulario de inscripción"
        };
    }

    [TestMethod]
    public void GetAll_ReturnsListOfFormularios()
    {
        // Arrange
        var expected = new List<Formulario> { _testFormulario };
        _mockRepo.Setup(r => r.GetAll()).Returns(expected);

        // Act
        var result = _mockRepo.Object.GetAll();

        // Assert
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetById_ExistingId_ReturnsFormulario()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(1)).Returns(_testFormulario);

        // Act
        var result = _mockRepo.Object.GetById(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Inscripción 2026", result!.Nombre);
    }
}