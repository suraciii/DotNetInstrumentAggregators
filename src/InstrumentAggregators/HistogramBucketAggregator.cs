using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CloudEventDotNet.Diagnostics.Aggregators;

internal class HistogramBucketAggregator(
    KeyValuePair<string, object?>[] tags,
    double bound,
    KeyValuePair<string, object?> label)
{
    private long _value = 0;
    private readonly KeyValuePair<string, object?>[] _tags = tags.Concat(new[] { label }).ToArray();
    public double Bound { get; } = bound;

    public ReadOnlySpan<KeyValuePair<string, object?>> Tags => _tags;

    public long Value => _value;

    public void Add(long measurement) => Interlocked.Add(ref _value, measurement);
}
