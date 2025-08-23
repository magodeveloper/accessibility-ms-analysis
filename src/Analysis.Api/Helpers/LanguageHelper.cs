namespace Analysis.Api.Helpers
{
    public static class LanguageHelper
    {
        public static string GetRequestLanguage(HttpRequest request)
        {
            var acceptLanguage = request.Headers.AcceptLanguage.FirstOrDefault();
            return acceptLanguage?.Split(',')[0] ?? "es";
        }
    }
}
