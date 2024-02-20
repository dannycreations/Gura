using System;
using ObjectModel;

public class ConsumedItemStat : StatsCounter
{
	public ConsumedItemStat()
	{
		base.Type = StatsCounterType.ConsumedItemType;
	}

	public ItemSubTypes ItemSubType { get; set; }
}
