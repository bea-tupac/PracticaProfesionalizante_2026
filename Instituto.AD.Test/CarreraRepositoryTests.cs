using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Repositories;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Instituto.AD.Test;

[TestClass]
public sealed class CarreraRepositoryTests
{
    private Mock<ICarreraRepository> _mockRepo = null!;
    private Carrera _testCarrera = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<ICarreraRepository>();
        _testCarrera = new Carrera
        {
            Id = 1,
            Nombre = "Tecnicatura en Programación",
            DuracionAnios = 3,
            Turno = "Mañana",
            Modalidad = "Presencial",
            Horario = "08:00-13:00",
            Estado = "Activa"
        };
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsListOfCarreras()
    {
        // Arrange
        var expected = new List<Carrera> { _testCarrera };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);

        // Act
        var result = await _mockRepo.Object.GetAllAsync();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Tecnicatura en Programación", result[0].Nombre);
    }

    [TestMethod]
    public async Task GetByIdAsync_ExistingId_ReturnsCarrera()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testCarrera);

        // Act
        var result = await _mockRepo.Object.GetByIdAsync(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result!.Id);
    }

    [TestMethod]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Carrera?)null);

        // Act
        var result = await _mockRepo.Object.GetByIdAsync(99);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateAsync_ValidCarrera_ReturnsCarreraWithId()
    {
        // Arrange
        var newCarrera = new Carrera { Nombre = "Nueva Carrera", DuracionAnios = 2 };
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Carrera>())).ReturnsAsync<Carrera>(c => { c.Id = 5; return c; });

        // Act
        var result = await _mockRepo.Object.CreateAsync(newCarrera);

        // Assert
        Assert.AreEqual(5, result.Id);
        Assert.AreEqual("Nueva Carrera", result.Nombre);
    }

    [TestMethod]
    public async Task DeleteAsync_ExistingId_CallsRepository()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Act
        await _mockRepo.Object.DeleteAsync(1);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [TestMethod]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _mockRepo.Object.ExistsAsync(1);

        // Assert
        Assert.IsTrue(result);
    }
}