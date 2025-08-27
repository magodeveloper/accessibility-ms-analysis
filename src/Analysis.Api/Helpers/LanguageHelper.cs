using Microsoft.AspNetCore.Http;

namespace Analysis.Api.Helpers
{
    public static class LanguageHelper
    {
        public static string GetRequestLanguage(HttpRequest request)
        {
            try
            {
                var header = request.Headers["Accept-Language"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(header)) return "es"; // default

                // Tomar el primer token antes de coma y limpiarlo
                var token = header.Split(',')[0].Trim();
                if (token.Length < 2) return "es"; // demasiado corto

                var lang2 = token[..2].ToLowerInvariant();
                // Limitar a idiomas soportados; fallback a 'es'
                return lang2 is "es" or "en" ? lang2 : "es";
            }
            catch
            {
                return "es"; // nunca debe romper el flujo por el header
            }
        }
    }
}
