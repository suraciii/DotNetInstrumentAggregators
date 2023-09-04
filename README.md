# InstrumentAggregators

Aggregate and group `System.Diagnostics.Metrics` counters and collect them as `ObservableCounter`, to reduce performance overhead.

## Basic Usage:

### Histogram Metrics:

```csharp
var group = new CounterAggregatorGroup();
var counter = group.CreateInstrument(_meter, "request_count");
var aggregator1 = group.FindOrCreate(new("method", "get"));
var aggregator2 = group.FindOrCreate(new("method", "post"));
aggregator1.Add(1);
```

### Histogram Metrics:

```csharp
var bounds = new long[] { 1, 3, 5, 8, 13 };
var group = new HistogramAggregatorGroup(new HistogramAggregatorOptions(bounds));
var metricsName = "request_duration";
var (bucket, count, sum) = group.CreateInstrument(_meter, metricsName);
var aggregator = group.FindOrCreate(new TagList("foo", "bar"));
aggregator.Record(0);
aggregator.Record(2);
aggregator.Record(5);
```

