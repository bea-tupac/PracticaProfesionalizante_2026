using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Moq;

namespace Instituto.AD.Test;

[TestClass]
public sealed class ListadoRepositoryTests
{
    private Mock<IListadoRepository> _mockRepo = null!;
    private List<ListadoItem> _testListado = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IListadoRepository>();
        _testListado = new List<ListadoItem>
        {
            new ListadoItem
            {
                AlumnoId = 1,
                NombreCompleto = "Pérez, Juan",
                DNI = 12345678,
                Email = "juan@test.com",
                Carrera = "Tecnicatura en Programación",
                Turno = "Mañana",
                Edad = 26
            }
        };
    }

    [TestMethod]
    public void GetListado_ReturnsListOfListadoItems()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetListado()).Returns(_testListado);

        // Act
        var result = _mockRepo.Object.GetListado();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Pérez, Juan", result[0].NombreCompleto);
    }
}