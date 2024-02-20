using System;

public class FishStat : StatsCounter
{
	public FishStat()
	{
		base.Type = StatsCounterType.FishType;
	}

	public int FishCategoryId { get; set; }

	public string FishCategoryName { get; set; }

	public FishInfo MaxFish { get; set; }

	public int MaxFishPond { get; set; }

	public int[] MaxFishTackleSet { get; set; }

	public DateTime MaxFishDate { get; set; }

	public float Weight { get; set; }
}
