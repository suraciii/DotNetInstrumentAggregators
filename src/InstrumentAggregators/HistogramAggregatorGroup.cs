using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace InstrumentAggregators;

public sealed class HistogramAggregatorGroup(HistogramAggregatorOptions options)
{
    public ConcurrentDictionary<TagList, HistogramAggregator> Aggregators { get; } = new();

    public HistogramAggregator FindOrCreate(in TagList tagList)
    {
        if (Aggregators.TryGetValue(tagList, out var stat))
        {
            return stat;
        }
        return Aggregators.GetOrAdd(tagList, new HistogramAggregator(tagList, options));
    }

    public IEnumerable<Measurement<long>> CollectBuckets()
    {
        return Aggregators.Values.SelectMany(x => x.CollectBuckets());
    }

    public IEnumerable<Measurement<long>> CollectCount()
    {
        return Aggregators.Values.Select(x => x.CollectCount());
    }

    public IEnumerable<Measurement<long>> CollectSum()
    {
        return Aggregators.Values.Select(x => x.CollectSum());
    }
}
