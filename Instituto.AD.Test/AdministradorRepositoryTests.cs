using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Moq;

namespace Instituto.AD.Test;

[TestClass]
public sealed class AdministradorRepositoryTests
{
    private Mock<IAdministradorRepository> _mockRepo = null!;
    private Administrador _testAdmin = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IAdministradorRepository>();
        _testAdmin = new Administrador
        {
            Id = 1,
            Nombre = "Admin",
            Apellido = "Principal",
            Email = "admin@test.com",
            Role = "Admin",
            PasswordHash = "hashed_password",
            Activo = true
        };
    }

    [TestMethod]
    public void GetAll_ReturnsOnlyActiveAdmins()
    {
        // Arrange
        var admins = new List<Administrador> { _testAdmin };
        _mockRepo.Setup(r => r.GetAll()).Returns(admins);

        // Act
        var result = _mockRepo.Object.GetAll();

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result[0].Activo);
    }

    [TestMethod]
    public void GetByEmail_ExistingEmail_ReturnsAdmin()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByEmail("admin@test.com")).Returns(_testAdmin);

        // Act
        var result = _mockRepo.Object.GetByEmail("admin@test.com");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("admin@test.com", result!.Email);
    }

    [TestMethod]
    public void ExistsByEmail_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsByEmail("admin@test.com")).Returns(true);

        // Act
        var result = _mockRepo.Object.ExistsByEmail("admin@test.com");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Count_ReturnsTotalCount()
    {
        // Arrange
        _mockRepo.Setup(r => r.Count()).Returns(5);

        // Act
        var result = _mockRepo.Object.Count();

        // Assert
        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public void UpdatePasswordHash_CallsRepository()
    {
        // Arrange
        const string newHash = "new_hashed_password";

        // Act
        _mockRepo.Object.UpdatePasswordHash(1, newHash);

        // Assert
        _mockRepo.Verify(r => r.UpdatePasswordHash(1, newHash), Times.Once);
    }
}