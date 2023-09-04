using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;

namespace InstrumentAggregators;

public sealed class HistogramAggregator
{
    private readonly KeyValuePair<string, object?>[] _tags;
    private readonly HistogramBucketAggregator[] _buckets;
    private readonly HistogramAggregatorOptions _options;
    private readonly ThreadLocal<long> _count = new(true);
    private readonly ThreadLocal<long> _sum = new(true);

    public HistogramAggregator(
        in TagList tagList,
        HistogramAggregatorOptions options)
    {
        _tags = tagList.ToArray();

        long[] buckets = options.Buckets;
        if (buckets[^1] != long.MaxValue)
        {
            buckets = [.. buckets, .. new[] { long.MaxValue }];
        }

        Func<long, KeyValuePair<string, object?>> getLabel;
        if (options.GetLabel is not null)
        {
            getLabel = options.GetLabel;
        }
        else
        {
            if (options.AggregationType == HistogramAggregationType.Cumulative)
            {
                getLabel = b => b == long.MaxValue ? new("le", "+Inf") : new("le", b);
            }
            else
            {
                getLabel = b => new("bucket", b);
            }
        }
        _buckets = buckets.Select(b => new HistogramBucketAggregator(_tags, b, getLabel(b))).ToArray();
        _options = options;
    }

    public (ObservableCounter<long> Sum,ObservableCounter<long> Count,ObservableCounter<long> Bucket)CreateInstruments(Meter meter, string name){
        return (
            meter.CreateObservableCounter(name + "_bucket", CollectBuckets),
            meter.CreateObservableCounter(name + "_count", CollectCount),
            meter.CreateObservableCounter(name + "_sum", CollectSum)
        );
    }

    public void Record(long number)
    {
        int i;
        for (i = 0; i < _buckets.Length; i++)
        {
            if (number <= _buckets[i].Bound)
            {
                break;
            }
        }
        _buckets[i].Add(1);
        _count.Value++;
        _sum.Value+= number;
    }

    public IEnumerable<Measurement<long>> CollectBuckets()
    {
        return _options.AggregationType switch
        {
            HistogramAggregationType.Delta => CollectDelta(),
            _ => CollectCumulative(),
        };

        IEnumerable<Measurement<long>> CollectDelta()
        {
            foreach (var bucket in _buckets)
            {
                yield return new(bucket.Value, bucket.Tags);
            }
        }

        IEnumerable<Measurement<long>> CollectCumulative()
        {
            long sum = 0;
            foreach (var bucket in _buckets)
            {
                sum += bucket.Value;
                yield return new(sum, bucket.Tags);
            }
        }
    }

    public Measurement<long> CollectCount() => new(_count.Values.Sum(), _tags);

    public Measurement<long> CollectSum() => new(_sum.Values.Sum(), _tags);
}
