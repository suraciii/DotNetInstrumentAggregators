# InstrumentAggregators

Aggregate and group `System.Diagnostics.Metrics` counters and collect them as `ObservableCounter`, to reduce performance overhead.

## Basic Usage:

```csharp
internal sealed class MyTelemetry
{
    public static Meter Meter { get; }
    private static readonly CounterAggregatorGroup s_myCounterAggregatorGroup
        = new(Meter, "counter_name1");
    private static readonly HistogramAggregatorGroup s_myHistogramAggregatorGroup = new(
        new HistogramAggregatorOptions(
            new long[] { 5, 20, 100, 500, 1_000, 5_000, 20_000 }),
        Meter, "latency_histogram_name2"
    );
        = new(Meter, "my_metrics_name2");

    private readonly CounterAggregator _myCounter;
    public MyTelemetry(string tagValue1, string tagValue2)
    {
        var tagList = new TagList("tagName1", tagValue1, "tagName2", tagValue2);
        // get aggregator with specific tags from aggregator group
        _myCounter = s_newMessageFetchedCounterGroup.FindOrCreate(tagList);
        _myHistogram = s_myHistogramAggregatorGroup.FindOrCreate(tagList);
    }

    public void RecordCounter()
    {
        _myCounter.Add(1);
    }

    // histograms will be record as 3 counters:
    // latency_histogram_name2_count, latency_histogram_name2_sum and latency_histogram_name2_buckets
    public void RecordHistogram(TimeSpan timeSpan)
    {
        _myHistogram.Record((long)latency.TotalMilliseconds);
    }
}
```

