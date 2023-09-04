using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace InstrumentAggregators.Test;

public class CounterAggregatorGroupTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;
    private readonly Meter _meter = new(nameof(CounterAggregatorGroupTests));

    [Fact]
    public void ValidateAggregatorCache()
    {
        var group = new CounterAggregatorGroup();
        var counter = group.CreateInstrument(_meter, "request_count");
        var aggregator1 = group.FindOrCreate(new("method", "get"));
        var aggregator2 = group.FindOrCreate(new("method", "get"));
        aggregator1.Add(1);

        Assert.Same(aggregator1, aggregator2);
        Assert.Single(group.Aggregators);
    }

    [Fact]
    public void Collect()
    {
        var group = new CounterAggregatorGroup();
        var counter = group.CreateInstrument(_meter, "request_count");
        var aggregator1 = group.FindOrCreate(new("method", "get"));
        var aggregator2 = group.FindOrCreate(new("method", "post"));

        aggregator1.Add(1);
        aggregator1.Add(2);
        aggregator2.Add(2);
        aggregator2.Add(3);

        var measurements = group.Collect().ToList();
        Assert.Equal(2, measurements.Count);
        Assert.Equal(3, measurements.Single(m => m.Tags[0].Value is "get").Value);
        Assert.Equal(5, measurements.Single(m => m.Tags[0].Value is "post").Value);
    }

    [Fact]
    public void TestMultithreadedCorrectness()
    {
        int numOfIterations = 1000000;

        var group = new CounterAggregatorGroup();
        var counter = _meter.CreateObservableCounter(nameof(TestMultithreadedCorrectness), group.Collect);
        var counterCount = Environment.ProcessorCount;

        Parallel.For(0, Environment.ProcessorCount, j =>
        {
            for (int i = 0; i < counterCount; i++)
            {
                var aggregator = group.FindOrCreate(new("test", i));

                for (int k = 0; k < numOfIterations; k++)
                {
                    aggregator.Add(i);
                }
            }
        });

        var measurements = group.Collect().OrderBy(m => m.Tags[0].Value).ToList();
        foreach (var measurement in measurements)
        {
            var i = (int)measurement.Tags[0].Value!;
            _output.WriteLine("{0} {1}", i, measurement.Value);
            Assert.Equal(i * Environment.ProcessorCount * numOfIterations, measurement.Value);
        }
    }

}
