﻿using System.Collections.Generic;

namespace RunningStatistics;

public interface IRunningStatistic<TObs>
{
    public long Count { get; }

    public void Fit(IEnumerable<TObs> values);

    public void Fit(TObs value);

    public void Reset();
    public void Merge(IRunningStatistic<TObs> other);
}