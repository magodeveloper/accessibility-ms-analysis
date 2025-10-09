namespace Analysis.Domain.Services;

/// <summary>
/// Domain service para validar la integridad de usuarios en análisis
/// Reemplaza las foreign key constraints de base de datos para compatibilidad con Docker/microservicios
/// </summary>
public interface IUserValidationService
{
    /// <summary>
    /// Valida si un usuario existe y es válido para crear análisis
    /// </summary>
    /// <param name="userId">ID del usuario a validar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el usuario existe y es válido</returns>
    Task<bool> ValidateUserExistsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si un usuario puede acceder a un análisis específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="analysisId">ID del análisis</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si el usuario puede acceder al análisis</returns>
    Task<bool> ValidateUserCanAccessAnalysisAsync(int userId, int analysisId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida múltiples usuarios de forma eficiente
    /// </summary>
    /// <param name="userIds">Lista de IDs de usuarios</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Diccionario con userId -> isValid</returns>
    Task<Dictionary<int, bool>> ValidateMultipleUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
}
