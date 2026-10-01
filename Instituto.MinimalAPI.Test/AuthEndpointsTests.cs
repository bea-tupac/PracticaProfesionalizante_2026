using System.Net;
using System.Net.Http.Json;
using Instituto.MinimalAPI;
using Instituto.MinimalAPI.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Instituto.MinimalAPI.Test;

[TestClass]
public sealed class AuthControllerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var mockService = new Mock<Instituto.BR.Interfaces.IAdministradorService>();
                var adminResult = new Instituto.BR.DTOs.AdminResult(1, "Admin", "Test", "admin@test.com", "Admin");
                
                mockService.Setup(s => s.Login("admin@test.com", "Password123")).Returns(adminResult);
                mockService.Setup(s => s.Login("wrong@test.com", "Password123")).Returns((Instituto.BR.DTOs.AdminResult?)null);
                
                services.AddScoped(_ => mockService.Object);
            });
        });
        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest { Email = "admin@test.com", Password = "Password123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data?.Token);
        Assert.AreEqual("admin@test.com", result.Data.Admin.Email);
    }

    [TestMethod]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "wrong@test.com", Password = "Password123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}