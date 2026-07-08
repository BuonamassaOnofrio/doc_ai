using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;
using UserManagement.Api.Repositories;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository repository, ILogger<UsersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Restituisce l'elenco di tutti gli utenti.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllAsync(cancellationToken);
        return Ok(users.Select(ToDto));
    }

    /// <summary>Restituisce un utente specifico tramite Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Utente non trovato",
                Detail = $"Nessun utente trovato con Id '{id}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(ToDto(user));
    }

    /// <summary>Crea un nuovo utente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto request, CancellationToken cancellationToken)
    {
        if (await _repository.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Email già in uso",
                Detail = $"Esiste già un utente con email '{request.Email}'.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Utente creato con Id {UserId}", user.Id);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToDto(user));
    }

    /// <summary>Aggiorna integralmente un utente esistente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserDto request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Utente non trovato",
                Detail = $"Nessun utente trovato con Id '{id}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (await _repository.EmailExistsAsync(request.Email, id, cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Email già in uso",
                Detail = $"Esiste già un altro utente con email '{request.Email}'.",
                Status = StatusCodes.Status409Conflict
            });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email.Trim().ToLowerInvariant();
        user.UpdatedAt = DateTime.UtcNow;

        _repository.Update(user);
        await _repository.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(user));
    }

    /// <summary>Elimina un utente esistente.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Utente non trovato",
                Detail = $"Nessun utente trovato con Id '{id}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        _repository.Remove(user);
        await _repository.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
