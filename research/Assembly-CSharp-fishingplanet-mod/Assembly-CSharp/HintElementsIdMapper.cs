using System;
using System.Collections.Generic;
using System.Linq;

public static class HintElementsIdMapper
{
	public static HintElementsIdMapper.HintPrefabs GetHintPrefabByElemId(string elemId)
	{
		KeyValuePair<HintElementsIdMapper.HintPrefabs, string[]> keyValuePair = HintElementsIdMapper.Mapping.FirstOrDefault((KeyValuePair<HintElementsIdMapper.HintPrefabs, string[]> p) => p.Value.Contains(elemId));
		return keyValuePair.Equals(default(KeyValuePair<HintElementsIdMapper.HintPrefabs, string[]>)) ? HintElementsIdMapper.HintPrefabs.None : keyValuePair.Key;
	}

	private static readonly Dictionary<HintElementsIdMapper.HintPrefabs, string[]> Mapping = new Dictionary<HintElementsIdMapper.HintPrefabs, string[]>
	{
		{
			HintElementsIdMapper.HintPrefabs.ColorImageParent,
			new string[] { "HUDLureIndicator", "HUDBobberIndicatorSurfacing", "HUDBobberIndicatorDive" }
		},
		{
			HintElementsIdMapper.HintPrefabs.ColorTextChildren,
			new string[] { "HUDTimePanel", "HUDWeatherPanel", "HUDFishKeepnetPanel", "HUDHelpText", "HUDLengthLine", "HUDLengthLineFull" }
		},
		{
			HintElementsIdMapper.HintPrefabs.CastSimple,
			new string[] { "HUDCastSimple" }
		},
		{
			HintElementsIdMapper.HintPrefabs.CastTarget,
			new string[] { "HUDCastTarget" }
		},
		{
			HintElementsIdMapper.HintPrefabs.Bobber,
			new string[] { "HUDBobberIndicator" }
		},
		{
			HintElementsIdMapper.HintPrefabs.FrictionSpeed,
			new string[] { "HUDFrictionSpeed" }
		},
		{
			HintElementsIdMapper.HintPrefabs.Friction,
			new string[] { "HUDFriction" }
		},
		{
			HintElementsIdMapper.HintPrefabs.BobberIndicatorBottom,
			new string[] { "HUDBobberIndicatorBottom" }
		},
		{
			HintElementsIdMapper.HintPrefabs.BobberIndicatorTop,
			new string[] { "HUDBobberIndicatorTop" }
		},
		{
			HintElementsIdMapper.HintPrefabs.BobberIndicatorTimer,
			new string[] { "HUDBobberIndicatorTimer" }
		},
		{
			HintElementsIdMapper.HintPrefabs.LineRodReelIndicator,
			new string[] { "HUDLineRodReelIndicator" }
		},
		{
			HintElementsIdMapper.HintPrefabs.Achivements,
			new string[] { "ACHIEVEMENTS" }
		},
		{
			HintElementsIdMapper.HintPrefabs.PondLicensesToggle,
			new string[] { "PondLicensesToggle" }
		},
		{
			HintElementsIdMapper.HintPrefabs.FeederFishingIndicator,
			new string[] { "HUDFeederFishingIndicator" }
		},
		{
			HintElementsIdMapper.HintPrefabs.BottomFishingIndicator,
			new string[] { "HUDBottomFishingIndicator" }
		}
	};

	public enum HintPrefabs : byte
	{
		None,
		ColorImageParent,
		ColorTextChildren,
		ColorImageChildren,
		BobberIndicatorBottom,
		BobberIndicatorTop,
		BobberIndicatorTimer,
		LineRodReelIndicator,
		FrictionSpeed,
		Friction,
		CastSimple,
		CastTarget,
		Bobber,
		Achivements,
		PondLicensesToggle,
		FeederFishingIndicator,
		BottomFishingIndicator
	}
}
