using System.Diagnostics.Metrics;

namespace InstrumentAggregators;

public static class AggregatorGroupExtensions
{
    public static ObservableCounter<long> CreateInstrument(
        this CounterAggregatorGroup group,
        Meter meter,
        string name,
        string? unit = null,
        string? description = null) => meter.CreateObservableCounter(name, group.Collect, unit, description);

    public static HistogramCounters CreateInstrument(
        this HistogramAggregatorGroup group,
        Meter meter,
        string name,
        string? unit = null,
        string? description = null)
    {
        return new (
            meter.CreateObservableCounter(name + "_bucket", group.CollectBuckets, unit, description),
            meter.CreateObservableCounter(name + "_count", group.CollectCount, unit, description),
            meter.CreateObservableCounter(name + "_sum", group.CollectSum, unit, description)
        );
    }
}

public record struct HistogramCounters(
    ObservableCounter<long> Sum,
    ObservableCounter<long> Count,
    ObservableCounter<long> Bucket);
