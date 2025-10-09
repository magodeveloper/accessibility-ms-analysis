using Prometheus;

namespace Analysis.Api.Metrics;

/// <summary>
/// Métricas personalizadas de negocio para el microservicio de Analysis
/// Expone contadores, histogramas y gauges para monitoreo en Prometheus
/// </summary>
public static class AnalysisMetrics
{
    // ============================================
    // CONTADORES (Counters)
    // ============================================

    /// <summary>
    /// Total de análisis realizados
    /// Labels: tool (axe_core, ibm_equal_access), status (pending, completed, failed)
    /// </summary>
    public static readonly Counter AnalysesPerformedTotal = Prometheus.Metrics.CreateCounter(
        "analyses_performed_total",
        "Total de análisis de accesibilidad realizados",
        new CounterConfiguration
        {
            LabelNames = ["tool", "status"]
        }
    );

    /// <summary>
    /// Total de consultas de análisis
    /// Labels: operation (get_all, get_by_user, get_by_tool, get_by_status, get_by_date)
    /// </summary>
    public static readonly Counter AnalysesQueriesTotal = Prometheus.Metrics.CreateCounter(
        "analyses_queries_total",
        "Total de consultas de análisis realizadas",
        new CounterConfiguration
        {
            LabelNames = ["operation"]
        }
    );

    /// <summary>
    /// Total de resultados de análisis creados
    /// Labels: severity (critical, serious, moderate, minor)
    /// </summary>
    public static readonly Counter ResultsCreatedTotal = Prometheus.Metrics.CreateCounter(
        "analysis_results_created_total",
        "Total de resultados de análisis creados",
        new CounterConfiguration
        {
            LabelNames = ["severity"]
        }
    );

    /// <summary>
    /// Total de errores de análisis registrados
    /// </summary>
    public static readonly Counter ErrorsCreatedTotal = Prometheus.Metrics.CreateCounter(
        "analysis_errors_created_total",
        "Total de errores de análisis registrados"
    );

    /// <summary>
    /// Total de análisis eliminados
    /// </summary>
    public static readonly Counter AnalysesDeletionsTotal = Prometheus.Metrics.CreateCounter(
        "analyses_deletions_total",
        "Total de análisis eliminados"
    );

    /// <summary>
    /// Total de validaciones de usuario exitosas/fallidas
    /// Labels: result (success, failure)
    /// </summary>
    public static readonly Counter UserValidationsTotal = Prometheus.Metrics.CreateCounter(
        "analysis_user_validations_total",
        "Total de validaciones de usuario realizadas",
        new CounterConfiguration
        {
            LabelNames = ["result"]
        }
    );

    // ============================================
    // HISTOGRAMAS (Histograms)
    // ============================================

    /// <summary>
    /// Duración de análisis en segundos
    /// Labels: tool (axe_core, ibm_equal_access)
    /// </summary>
    public static readonly Histogram AnalysisDuration = Prometheus.Metrics.CreateHistogram(
        "analysis_duration_seconds",
        "Duración de análisis de accesibilidad en segundos",
        new HistogramConfiguration
        {
            LabelNames = ["tool"],
            Buckets = Histogram.ExponentialBuckets(0.1, 2, 12) // 100ms a ~400s
        }
    );

    /// <summary>
    /// Duración de consultas de análisis en segundos
    /// Labels: operation
    /// </summary>
    public static readonly Histogram AnalysisQueryDuration = Prometheus.Metrics.CreateHistogram(
        "analysis_query_duration_seconds",
        "Duración de consultas de análisis en segundos",
        new HistogramConfiguration
        {
            LabelNames = ["operation"],
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) // 1ms a ~1s
        }
    );

    /// <summary>
    /// Número de resultados por análisis
    /// </summary>
    public static readonly Histogram ResultsPerAnalysis = Prometheus.Metrics.CreateHistogram(
        "results_per_analysis",
        "Número de resultados encontrados por análisis",
        new HistogramConfiguration
        {
            Buckets = Histogram.LinearBuckets(0, 10, 20) // 0-200 en buckets de 10
        }
    );

    // ============================================
    // GAUGES (Medidores)
    // ============================================

    /// <summary>
    /// Análisis en progreso actualmente
    /// </summary>
    public static readonly Gauge AnalysesInProgress = Prometheus.Metrics.CreateGauge(
        "analyses_in_progress",
        "Número de análisis actualmente en progreso"
    );

    /// <summary>
    /// Análisis por herramienta
    /// Labels: tool
    /// </summary>
    public static readonly Gauge AnalysesByTool = Prometheus.Metrics.CreateGauge(
        "analyses_by_tool",
        "Número de análisis por herramienta",
        new GaugeConfiguration
        {
            LabelNames = ["tool"]
        }
    );

    /// <summary>
    /// Total de análisis por estado
    /// Labels: status
    /// </summary>
    public static readonly Gauge AnalysesByStatus = Prometheus.Metrics.CreateGauge(
        "analyses_by_status",
        "Número de análisis por estado",
        new GaugeConfiguration
        {
            LabelNames = ["status"]
        }
    );

    // ============================================
    // MÉTODOS DE UTILIDAD
    // ============================================

    /// <summary>
    /// Registra un análisis completado
    /// </summary>
    public static void RecordAnalysisPerformed(string tool, string status)
    {
        AnalysesPerformedTotal.WithLabels(tool.ToLower(), status.ToLower()).Inc();
    }

    /// <summary>
    /// Registra una consulta de análisis
    /// </summary>
    public static void RecordQuery(string operation)
    {
        AnalysesQueriesTotal.WithLabels(operation).Inc();
    }

    /// <summary>
    /// Registra la duración de un análisis
    /// </summary>
    public static IDisposable MeasureAnalysis(string tool)
    {
        return AnalysisDuration.WithLabels(tool.ToLower()).NewTimer();
    }

    /// <summary>
    /// Registra la duración de una consulta
    /// </summary>
    public static IDisposable MeasureQuery(string operation)
    {
        return AnalysisQueryDuration.WithLabels(operation).NewTimer();
    }

    /// <summary>
    /// Incrementa análisis en progreso
    /// </summary>
    public static void IncrementInProgress()
    {
        AnalysesInProgress.Inc();
    }

    /// <summary>
    /// Decrementa análisis en progreso
    /// </summary>
    public static void DecrementInProgress()
    {
        AnalysesInProgress.Dec();
    }
}
