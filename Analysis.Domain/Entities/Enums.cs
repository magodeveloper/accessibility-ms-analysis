namespace Analysis.Domain.Entities
{
    public enum ContentType
    {
        url,
        html
    }

    public enum ToolUsed
    {
        axecore,
        equalaccess,
        both
    }
    public enum AnalysisStatus
    {
        pending,
        success,
        error
    }

    public enum WcagVersion
    {
        _2_0,
        _2_1,
        _2_2
    }

    public enum WcagLevel
    {
        A,
        AA,
        AAA
    }

    public enum ResultLevel
    {
        violation,
        recommendation
    }

    public enum Severity
    {
        high,
        medium,
        low
    }
}