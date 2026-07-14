using System.Net;
using System.Net.Http.Json;
using UserManagement.Api.Dtos;
using Xunit;

namespace UserManagement.Api.Tests;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateUserDto NewUser(string email) => new()
    {
        FirstName = "Mario",
        LastName = "Rossi",
        Email = email
    };

    [Fact]
    public async Task Create_ReturnsCreated_WhenPayloadIsValid()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users", NewUser($"{Guid.NewGuid()}@example.com"));

        //Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        // Simulo fallimento test
        Assert.NotEqual(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/v1/users", NewUser(email));

        var response = await _client.PostAsJsonAsync("/api/v1/users", NewUser(email));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenEmailIsInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users", NewUser("not-an-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForUnknownId()
    {
        var response = await _client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/users", NewUser($"{Guid.NewGuid()}@example.com"));
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/v1/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var update = new UpdateUserDto
        {
            FirstName = "Luigi",
            LastName = "Verdi",
            Email = created.Email
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/users/{created.Id}", update);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal("Luigi", updated!.FirstName);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/api/v1/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }
}
