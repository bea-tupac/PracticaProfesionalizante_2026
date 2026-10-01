using System.Net;
using System.Net.Http.Json;
using Instituto.MinimalAPI;
using Instituto.MinimalAPI.Models;
using Instituto.AD.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Security.Claims;
using Moq;

namespace Instituto.MinimalAPI.Test;

[TestClass]
public sealed class CarreraControllerTests
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
                var mockService = new Mock<Instituto.BR.Interfaces.ICarreraService>();
                var testCarrera = new Carrera { Id = 1, Nombre = "Programación", DuracionAnios = 3, Estado = "Activa" };
                
                mockService.Setup(s => s.GetAll()).Returns(new List<Carrera> { testCarrera });
                mockService.Setup(s => s.GetById(1)).Returns(testCarrera);
                mockService.Setup(s => s.GetById(99)).Returns((Carrera?)null);
                
                // Mock Create to validate duration
                mockService.Setup(s => s.Create(It.Is<Carrera>(c => c.DuracionAnios >= 1 && c.DuracionAnios <= 10)))
                    .Returns<Carrera>(c => { c.Id = 2; return Instituto.BR.DTOs.ServiceResult<Carrera>.Ok(c); });
                
                mockService.Setup(s => s.Create(It.Is<Carrera>(c => c.DuracionAnios < 1 || c.DuracionAnios > 10)))
                    .Returns<Carrera>(c => Instituto.BR.DTOs.ServiceResult<Carrera>.Fail("La duración debe estar entre 1 y 10 años."));
                
                mockService.Setup(s => s.Update(It.IsAny<int>(), It.IsAny<Carrera>()))
                    .Returns((int _, Carrera c) => Instituto.BR.DTOs.ServiceResult<Carrera>.Ok(c));
                
                mockService.Setup(s => s.Delete(1))
                    .Returns(Instituto.BR.DTOs.ServiceResult.Ok("Eliminada"));
                
                services.AddScoped(_ => mockService.Object);
                
                // Disable JWT validation for testing
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
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
    public async Task GetAll_ReturnsListOfCarreras()
    {
        // Act
        var response = await _client.GetAsync("/api/carreras");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Carrera>>>();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Data!.Count);
    }

    [TestMethod]
    public async Task GetById_ExistingId_ReturnsCarrera()
    {
        // Act
        var response = await _client.GetAsync("/api/carreras/1");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Carrera>>();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Programación", result.Data!.Nombre);
    }

    [TestMethod]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/carreras/99");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Create_ValidCarrera_ReturnsCreated()
    {
        // Arrange
        var newCarrera = new Carrera { Nombre = "Nueva", DuracionAnios = 2 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/carreras", newCarrera);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Carrera>>();
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Create_InvalidCarrera_ReturnsBadRequest()
    {
        // Arrange - duration > 10
        var invalidCarrera = new Carrera { Nombre = "Test", DuracionAnios = 15 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/carreras", invalidCarrera);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

// Test authentication handler that always succeeds
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "test-user") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}