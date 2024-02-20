using System;

public class DailyInteractStat : TimedCounter
{
	public DailyInteractStat()
	{
		base.Type = StatsCounterType.DailyInteractStat;
	}

	public int ObjectId { get; set; }
}
