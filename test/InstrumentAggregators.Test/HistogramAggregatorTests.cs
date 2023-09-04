using System.Diagnostics.Metrics;
using System.Linq;

namespace InstrumentAggregators.Test;

public class HistogramAggregatorTests
{
    private readonly Meter _meter;
    public HistogramAggregatorTests()
    {
        _meter = new Meter(nameof(HistogramAggregatorTests));
    }

    [Fact]
    public void CollectHistogram()
    {
        var bounds = new long[] { 1, 3, 5, 8, 13 };
        var group = new HistogramAggregatorGroup(new HistogramAggregatorOptions(bounds));
        var metricsName = "request_duration";
        var (bucket, count, sum) = group.CreateInstrument(_meter, metricsName);
        var aggregator = group.FindOrCreate(new TagList("foo", "bar"));
        aggregator.Record(0);
        aggregator.Record(2);
        aggregator.Record(5);
        aggregator.Record(7);
        aggregator.Record(10);
        aggregator.Record(11);
        aggregator.Record(13);
        aggregator.Record(15);
        aggregator.Record(20);
        aggregator.Record(100);

        var buckets = group.CollectBuckets().ToArray();
        for (int i = 0; i < bounds.Length; i++)
        {
            Assert.Equal("foo", buckets[i].Tags[0].Key);
            Assert.Equal("bar", buckets[i].Tags[0].Value);
            Assert.Equal("le", buckets[0].Tags[^1].Key);
            Assert.Equal(bounds[i], buckets[i].Tags[^1].Value);
        }
        Assert.Equal("+Inf", buckets.Last().Tags[^1].Value);
        Assert.Equal(1, buckets[0].Value);
        Assert.Equal(2, buckets[1].Value);
        Assert.Equal(3, buckets[2].Value);
        Assert.Equal(4, buckets[3].Value);
        Assert.Equal(7, buckets[4].Value);
        Assert.Equal(10, buckets[5].Value);
        
        Assert.Equal(10, group.CollectCount().ToArray()[0].Value);
        Assert.Equal(183, group.CollectSum().ToArray()[0].Value);
    }
}
