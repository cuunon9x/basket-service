using Prometheus;

namespace BasketAPI.Infrastructure.Metrics;

public class PrometheusMetrics : IMetrics
{
    private readonly Dictionary<string, Counter> _counters = new();
    private readonly Dictionary<string, Histogram> _histograms = new();

    public void IncrementCounter(string name, string help, params string[] labelValues)
    {
        var counter = GetOrCreateCounter(name, help, labelValues.Length);
        if (labelValues.Length > 0)
        {
            counter.WithLabels(labelValues).Inc();
        }
        else
        {
            counter.Inc();
        }
    }

    public void ObserveHistogram(string name, string help, double value, params string[] labelValues)
    {
        var histogram = GetOrCreateHistogram(name, help, labelValues.Length);
        if (labelValues.Length > 0)
        {
            histogram.WithLabels(labelValues).Observe(value);
        }
        else
        {
            histogram.Observe(value);
        }
    }

    private Counter GetOrCreateCounter(string name, string help, int labelCount)
    {
        var key = $"{name}:{labelCount}";
        if (!_counters.TryGetValue(key, out var counter))
        {
            if (labelCount > 0)
            {
                var labelNames = GetDefaultLabelNames(name, labelCount);
                counter = Prometheus.Metrics.CreateCounter(name, help,
                    new CounterConfiguration
                    {
                        LabelNames = labelNames
                    });
            }
            else
            {
                counter = Prometheus.Metrics.CreateCounter(name, help);
            }
            _counters[key] = counter;
        }
        return counter;
    }

    private Histogram GetOrCreateHistogram(string name, string help, int labelCount)
    {
        var key = $"{name}:{labelCount}";
        if (!_histograms.TryGetValue(key, out var histogram))
        {
            if (labelCount > 0)
            {
                var labelNames = GetDefaultLabelNames(name, labelCount);
                histogram = Prometheus.Metrics.CreateHistogram(name, help,
                    new HistogramConfiguration
                    {
                        LabelNames = labelNames,
                        Buckets = new[] { .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 }
                    });
            }
            else
            {
                histogram = Prometheus.Metrics.CreateHistogram(name, help,
                    new HistogramConfiguration
                    {
                        Buckets = new[] { .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 }
                    });
            }
            _histograms[key] = histogram;
        }
        return histogram;
    }

    private static string[] GetDefaultLabelNames(string metricName, int labelCount)
    {
        // Define standard label names based on metric name and count
        return metricName switch
        {
            "basket_repository_operations_total" when labelCount == 2 => new[] { "operation", "status" },
            "basket_repository_operation_duration_seconds" when labelCount == 1 => new[] { "operation" },
            _ => Enumerable.Range(0, labelCount).Select(i => $"label{i}").ToArray()
        };
    }
}
