using System;
using System.Collections.Generic;

namespace InstrumentAggregators;

public record HistogramAggregatorOptions(
    long[] Buckets,
    HistogramAggregationType AggregationType = HistogramAggregationType.Cumulative,
    Func<long, KeyValuePair<string, object?>>? GetLabel = null
);

public enum HistogramAggregationType
{
    Cumulative, Delta
}

