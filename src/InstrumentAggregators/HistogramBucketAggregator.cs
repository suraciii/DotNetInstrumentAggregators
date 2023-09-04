using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace InstrumentAggregators;

internal sealed class HistogramBucketAggregator(
    KeyValuePair<string, object?>[] tags,
    double bound,
    KeyValuePair<string, object?> label)
{
    private readonly KeyValuePair<string, object?>[] _tags = [.. tags, .. new[] { label }];
    private readonly ThreadLocal<long> _value = new(true);

    public ReadOnlySpan<KeyValuePair<string, object?>> Tags => _tags;

    public double Bound { get; } = bound;

    public long Value => _value.Values.Sum();

    public void Add(long measurement) => _value.Value += measurement;
}
