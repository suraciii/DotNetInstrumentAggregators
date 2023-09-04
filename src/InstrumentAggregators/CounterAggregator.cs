using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;

namespace InstrumentAggregators;

public sealed class CounterAggregator(
    in TagList tagList)
{
    public CounterAggregator()
        : this(new TagList()) { }

    private readonly KeyValuePair<string, object?>[] _tags = tagList.ToArray();

    private readonly ThreadLocal<long> _value = new(true);

    public long Value => _value.Values.Sum();

    public void Add(long measurement) => _value.Value += measurement;

    public Measurement<long> Collect() => new(Value, _tags);
}
