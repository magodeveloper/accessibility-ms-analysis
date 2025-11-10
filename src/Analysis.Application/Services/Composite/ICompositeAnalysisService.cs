using Analysis.Application.Dtos;

namespace Analysis.Application.Services.Composite;

/// <summary>
/// Servicio para obtener análisis completos con sus resultados y errores asociados.
/// </summary>
public interface ICompositeAnalysisService
{
    /// <summary>
    /// Obtiene un análisis completo por ID incluyendo resultados y errores.
    /// </summary>
    /// <param name="analysisId">ID del análisis</param>
    /// <returns>Análisis completo con resultados y errores</returns>
    Task<CompleteAnalysisDto?> GetCompleteAnalysisByIdAsync(int analysisId);

    /// <summary>
    /// Obtiene todos los análisis completos de un usuario incluyendo resultados y errores.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Lista de análisis completos con resultados y errores</returns>
    Task<IEnumerable<CompleteAnalysisDto>> GetCompleteAnalysesByUserIdAsync(int userId);
}
