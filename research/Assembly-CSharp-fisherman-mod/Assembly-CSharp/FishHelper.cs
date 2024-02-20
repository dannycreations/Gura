using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;

public static class FishHelper
{
	public static string FillFishes(FishLicenseConstraint[] licenseFishes, List<Fish> Items)
	{
		string[] array = new string[]
		{
			ScriptLocalization.Get("YoungType").ToLower(),
			ScriptLocalization.Get("CommonType").ToLower(),
			ScriptLocalization.Get("TrophyType").ToLower(),
			ScriptLocalization.Get("UniqueType").ToLower()
		};
		IEnumerable<int> FishIds = licenseFishes.Select((FishLicenseConstraint x) => x.FishId);
		IEnumerable<IGrouping<int, Fish>> enumerable = from f in Items
			where FishIds.Contains(f.FishId)
			group f by f.CategoryId;
		string text = string.Empty;
		foreach (IGrouping<int, Fish> grouping in enumerable)
		{
			string text2 = string.Empty;
			string[] array2 = new string[]
			{
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty
			};
			Fish fish = null;
			foreach (Fish fish2 in grouping)
			{
				fish = fish2;
				if (fish2.IsTrophy != null && fish2.IsTrophy.Value)
				{
					array2[2] = array[2];
				}
				else if (fish2.IsYoung != null && fish2.IsYoung.Value)
				{
					array2[0] = array[0];
				}
				else if (fish2.IsUnique != null && fish2.IsUnique.Value)
				{
					array2[3] = array[3];
				}
				else
				{
					array2[1] = array[1];
				}
			}
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i] != string.Empty)
				{
					text2 += ((!(text2 == string.Empty)) ? (", " + array2[i]) : array2[i]);
				}
			}
			text2 = CacheLibrary.MapCache.GetFishCategory(fish.CategoryId).Name + " (" + text2 + ")";
			text = text + ((!(text == string.Empty)) ? ", " : string.Empty) + text2;
		}
		return (!(text == string.Empty)) ? text : ScriptLocalization.Get("NoRestriction");
	}

	public static List<FishTypes> EventFishCategories = new List<FishTypes>
	{
		FishTypes.EventFish,
		FishTypes.HistoricalFish,
		FishTypes.SpookyFish,
		FishTypes.XmasFish
	};
}
