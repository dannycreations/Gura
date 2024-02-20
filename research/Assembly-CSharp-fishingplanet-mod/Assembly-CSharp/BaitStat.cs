using System;

public class BaitStat : StatsCounter
{
	public BaitStat()
	{
		base.Type = StatsCounterType.BaitType;
	}

	public int BaitId { get; set; }
}
