namespace Analysis.Domain.Entities
{
    public enum ContentType
    { url, html }
    public enum ToolUsed
    {
        [System.Text.Json.Serialization.JsonStringEnumMemberName("axe-core")]
        axecore,
        [System.Text.Json.Serialization.JsonStringEnumMemberName("equal-access")]
        equalaccess,
        both
    }
    public enum AnalysisStatus
    { pending, success, error }
    public enum WcagLevel
    { A, AA, AAA }
    public enum ResultLevel
    { violation, recommendation, potentialViolation, manualCheck, pass }
    public enum Severity
    { high, medium, low }
}