using Analysis.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Analysis.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de validación de usuarios
/// Se comunica con el microservicio Users API para validar usuarios
/// </summary>
public class UserValidationService(
    HttpClient httpClient,
    ILogger<UserValidationService> logger,
    IConfiguration configuration) : IUserValidationService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<UserValidationService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly string _usersApiBaseUrl = configuration.GetValue<string>("ExternalServices:UsersApi:BaseUrl")
            ?? throw new InvalidOperationException("Users API base URL is not configured. Please set 'ExternalServices:UsersApi:BaseUrl' in your configuration.");

    /// <inheritdoc />
    public async Task<bool> ValidateUserExistsAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Validating user existence for userId: {UserId}", userId);

            // En desarrollo/testing, permitir usuarios sin validación
            if (IsTestEnvironment())
            {
                _logger.LogInformation("Test environment detected, skipping user validation for userId: {UserId}", userId);
                return true;
            }

            var response = await _httpClient.GetAsync($"{_usersApiBaseUrl}/api/users/{userId}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("User {UserId} validated successfully", userId);
                return true;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return false;
            }

            _logger.LogWarning("Failed to validate user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error validating user {UserId}. Allowing request to proceed.", userId);
            // En caso de error de red, permitir la operación para evitar fallos en cascada
            return true;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout validating user {UserId}", userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateUserCanAccessAnalysisAsync(int userId, int analysisId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Primero validar que el usuario existe
            if (!await ValidateUserExistsAsync(userId, cancellationToken))
            {
                return false;
            }

            // Aquí podrías agregar lógica adicional de autorización
            // Por ejemplo, verificar permisos específicos, roles, etc.

            _logger.LogDebug("User {UserId} can access analysis {AnalysisId}", userId, analysisId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user {UserId} access to analysis {AnalysisId}", userId, analysisId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<int, bool>> ValidateMultipleUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<int, bool>();
        var tasks = new List<Task>();

        foreach (var userId in userIds)
        {
            tasks.Add(Task.Run(async () =>
            {
                var isValid = await ValidateUserExistsAsync(userId, cancellationToken);
                lock (results)
                {
                    results[userId] = isValid;
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        return results;
    }

    /// <summary>
    /// Determina si estamos en un entorno de testing donde no se requiere validación estricta
    /// </summary>
    private bool IsTestEnvironment()
    {
        var environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
        return environment == "Test" ||
               environment == "Development" && _configuration.GetValue<bool>("Testing:SkipUserValidation", false);
    }
}
