namespace Analysis.Domain.Entities
{
    public enum ContentType
    { url, html }
    public enum ToolUsed
    { axecore, equalaccess, both }
    public enum AnalysisStatus
    { pending, success, error }
    public enum WcagLevel
    { A, AA, AAA }
    public enum ResultLevel
    { violation, recommendation, potentialViolation, manualCheck, pass }
    public enum Severity
    { high, medium, low }
}