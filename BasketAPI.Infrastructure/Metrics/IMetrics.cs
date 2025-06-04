namespace BasketAPI.Infrastructure.Metrics;

public interface IMetrics
{
    void IncrementCounter(string name, string help, params string[] labelValues);
    void ObserveHistogram(string name, string help, double value, params string[] labelValues);
}
