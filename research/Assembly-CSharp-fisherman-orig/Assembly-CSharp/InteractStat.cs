using System;

public class InteractStat : StatsCounter
{
	public InteractStat()
	{
		base.Type = StatsCounterType.InteractStat;
	}

	public int ObjectId { get; set; }
}
