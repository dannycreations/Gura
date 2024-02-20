using System;
using System.Collections.Generic;

public class FishStatItemComparer : IEqualityComparer<FishStat>
{
	public bool Equals(FishStat x, FishStat y)
	{
		return x.MaxFish.FishId == y.MaxFish.FishId && Math.Abs(x.MaxFish.Weight - y.MaxFish.Weight) < 0.001f && x.MaxFishDate == y.MaxFishDate;
	}

	public int GetHashCode(FishStat obj)
	{
		return obj.MaxFish.FishId.GetHashCode() ^ obj.MaxFishDate.GetHashCode() ^ obj.MaxFishTackleSet.GetHashCode();
	}
}
