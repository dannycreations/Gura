using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class TotalFishFormStat : StatsCounter
{
	public TotalFishFormStat()
	{
		base.Type = StatsCounterType.TotalFishForm;
		this.CaughtFishForms = new List<int>();
	}

	[JsonIgnore]
	public static int[] RegularFishForms { get; set; }

	public List<int> CaughtFishForms { get; set; }

	[JsonIgnore]
	public int DistinctCount
	{
		get
		{
			if (TotalFishFormStat.RegularFishForms == null)
			{
				return 0;
			}
			return this.CaughtFishForms.Count((int fishId) => TotalFishFormStat.RegularFishForms.Contains(fishId));
		}
	}

	public bool CatchFish(int fishId)
	{
		if (TotalFishFormStat.RegularFishForms == null)
		{
			return false;
		}
		if (!this.CaughtFishForms.Contains(fishId) && TotalFishFormStat.RegularFishForms.Contains(fishId))
		{
			this.CaughtFishForms.Add(fishId);
			return true;
		}
		return false;
	}
}
