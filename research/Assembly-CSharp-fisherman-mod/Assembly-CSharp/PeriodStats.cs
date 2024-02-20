using System;
using System.Collections.Generic;

public class PeriodStats
{
	public List<StatAchievement> Achievements { get; set; }

	public List<StatLevel> Levels { get; set; }

	public List<StatPenalty> Penalties { get; set; }

	public bool ShouldDisplayDialog { get; set; }

	public bool IsLastDay;

	public int FishCount;

	public int TrophyCount;

	public int UniqueCount;

	public int Escapes;

	public int Hitches;

	public int LineBreaks;

	public int OtherBreaks;

	public int Experience;

	public int? Silver;

	public int? Gold;

	public int FishInCageCount;

	public int? FishSilver;

	public int? FishGold;

	public int? TravelCostSilver;

	public int? TravelCostGold;

	public int? StayCostSilver;

	public int? StayCostGold;

	public int StayPeriod;
}
