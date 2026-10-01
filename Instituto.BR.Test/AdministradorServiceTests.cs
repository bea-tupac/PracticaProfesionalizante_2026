using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Instituto.BR.Services;
using Moq;

namespace Instituto.BR.Test;

[TestClass]
public sealed class AdministradorServiceTests
{
    private Mock<IAdministradorRepository> _mockRepo = null!;
    private IAdministradorService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepo = new Mock<IAdministradorRepository>();
        _service = new AdministradorService(_mockRepo.Object);
    }

    [TestMethod]
    public void Create_ValidAdmin_ReturnsSuccess()
    {
        // Arrange
        var admin = new Administrador { Nombre = "Admin", Apellido = "Test", Email = "admin@test.com", Role = "Admin" };
        _mockRepo.Setup(r => r.ExistsByEmail("admin@test.com")).Returns(false);
        _mockRepo.Setup(r => r.Create(It.IsAny<Administrador>())).Returns<Administrador>(a => { a.Id = 1; return a; });

        // Act
        var result = _service.Create(admin, "Password123");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("admin@test.com", result.Data!.Email);
    }

    [TestMethod]
    public void Create_DuplicateEmail_ReturnsFail()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsByEmail("admin@test.com")).Returns(true);

        // Act
        var result = _service.Create(new Administrador { Nombre = "Admin", Apellido = "Test", Email = "admin@test.com", Role = "Admin" }, "Password123");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Ya existe un administrador con ese email.", result.Message);
    }

    [TestMethod]
    public void Create_ShortPassword_ReturnsFail()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsByEmail("admin@test.com")).Returns(false);

        // Act
        var result = _service.Create(new Administrador { Nombre = "Admin", Apellido = "Test", Email = "admin@test.com", Role = "Admin" }, "123");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("La contraseña debe tener al menos 8 caracteres.", result.Message);
    }

    [TestMethod]
    public void Login_ValidCredentials_ReturnsAdminResult()
    {
        // Arrange
        var admin = new Administrador
        {
            Id = 1,
            Nombre = "Admin",
            Apellido = "Test",
            Email = "admin@test.com",
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123", 12)
        };
        _mockRepo.Setup(r => r.GetByEmail("admin@test.com")).Returns(admin);

        // Act
        var result = _service.Login("admin@test.com", "Password123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("admin@test.com", result!.Email);
        Assert.AreEqual("Admin", result.Role);
    }

    [TestMethod]
    public void Login_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var admin = new Administrador
        {
            Id = 1,
            Nombre = "Admin",
            Apellido = "Test",
            Email = "admin@test.com",
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123", 12)
        };
        _mockRepo.Setup(r => r.GetByEmail("admin@test.com")).Returns(admin);

        // Act
        var result = _service.Login("admin@test.com", "WrongPassword");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ChangePassword_ValidCurrentPassword_ReturnsSuccess()
    {
        // Arrange
        var admin = new Administrador
        {
            Id = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123", 12)
        };
        _mockRepo.Setup(r => r.Exists(1)).Returns(true);
        _mockRepo.Setup(r => r.GetById(1)).Returns(admin);

        // Act
        var result = _service.ChangePassword(1, "OldPass123", "NewPass456");

        // Assert
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void ChangePassword_InvalidCurrentPassword_ReturnsFail()
    {
        // Arrange
        var admin = new Administrador
        {
            Id = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123", 12)
        };
        _mockRepo.Setup(r => r.Exists(1)).Returns(true);
        _mockRepo.Setup(r => r.GetById(1)).Returns(admin);

        // Act
        var result = _service.ChangePassword(1, "WrongPass", "NewPass456");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("La contraseña actual es incorrecta.", result.Message);
    }
}