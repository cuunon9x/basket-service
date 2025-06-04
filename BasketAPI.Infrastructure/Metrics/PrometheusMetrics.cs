using Prometheus;

namespace BasketAPI.Infrastructure.Metrics;

public class PrometheusMetrics : IMetrics
{
    private readonly Dictionary<string, Counter> _counters = new();
    private readonly Dictionary<string, Histogram> _histograms = new();

    public void IncrementCounter(string name, string help, params string[] labelValues)
    {
        var counter = GetOrCreateCounter(name, help);
        counter.WithLabels(labelValues).Inc();
    }

    public void ObserveHistogram(string name, string help, double value, params string[] labelValues)
    {
        var histogram = GetOrCreateHistogram(name, help);
        histogram.WithLabels(labelValues).Observe(value);
    }

    private Counter GetOrCreateCounter(string name, string help)
    {
        if (!_counters.TryGetValue(name, out var counter))
        {            counter = Prometheus.Metrics.CreateCounter(name, help);
            _counters[name] = counter;
        }
        return counter;
    }

    private Histogram GetOrCreateHistogram(string name, string help)
    {
        if (!_histograms.TryGetValue(name, out var histogram))
        {
            histogram = Prometheus.Metrics.CreateHistogram(name, help,
                new Prometheus.HistogramConfiguration
                {
                    Buckets = new[] { .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 }
                });
            _histograms[name] = histogram;
        }
        return histogram;
    }
}
