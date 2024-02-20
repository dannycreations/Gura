using ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gura.Utils;

public static class FishUtil
{
    public static void SafeEscape(this IFishController fish)
    {
        if (fish.Behavior == FishBehavior.Go) return;

        StateUtil.GameAdapter.FinishAttack(false, true, false, false, 0f);
        fish.Behavior = FishBehavior.Go;
    }

    public static float GetFishWeight(this IFishController fish) =>
        Safe.Result(() => MeasuringSystemManager.FishWeight(fish.FishTemplate.Weight));

    public static float GetFishLength(this IFishController fish) =>
        Safe.Result(() => MeasuringSystemManager.FishLength(fish.Owner.Length));

    public static string ToFormatName(this IFishController fish) =>
        $"<color={fish.FishTemplate.GetFishColor()}>{fish.FishTemplate.Name}</color>";

    public static string ToFormatWeight(this IFishController fish) =>
        $"{fish.GetFishWeight()} {MeasuringSystemManager.FishWeightSufix()}";

    public static string ToFormatLength(this IFishController fish) =>
        $"{fish.GetFishLength()} {MeasuringSystemManager.FishLengthSufix()}";

    private static readonly Dictionary<Il2CppSystem.Guid, FishTypes> CacheFishType = [];

    public static FishTypes GetFishType(this Fish fish)
    {
        var fishType = FishTypes.Common;
        if (CacheFishType.TryGetValue(fish.InstanceId.Value, out fishType))
            return fishType;

        if (fish.CodeName.EndsWith("Y") ||
            fish.Name.Find("young")
        ) fishType = FishTypes.Young;

        if (fish.IsEvent ||
            EventFish.Contains(fish.Name) ||
            XmasFish.Contains(fish.Name) ||
            HistoricalFish.Contains(fish.Name)
        ) fishType = FishTypes.Event;

        if (fish.CodeName.EndsWith("T") ||
            fish.Name.Find("trophy") ||
            Safe.IsResult(() => fish.IsTrophy.Value)
        ) fishType = FishTypes.Trophy;

        if (fish.CodeName.EndsWith("U") ||
            fish.Name.Find("unique") ||
            Safe.IsResult(() => fish.IsUnique.Value)
        ) fishType = FishTypes.Unique;

        if (CacheFishType.Count >= 10)
            CacheFishType.Remove(CacheFishType.First().Key);

        CacheFishType.Add(fish.InstanceId.Value, fishType);
        return fishType;
    }

    public static string GetFishColor(this Fish fish, string fishColor = null)
    {
        var fishType = fish.GetFishType();
        if (fishType == FishTypes.Common ||
            fishType == FishTypes.Young)
            return $"#{ColorFishCommon}";

        if (fishType == FishTypes.Event)
            return $"#{ColorFishEvent}";

        if (fishType == FishTypes.Trophy)
            return $"#{ColorFishTrophy}";

        if (fishType == FishTypes.Unique)
            return $"#{ColorFishUnique}";

        return $"#{(string.IsNullOrEmpty(fishColor) ? ColorFishCommon : fishColor)}";
    }

    private static readonly string ColorFishTrophy = global::UIHelper.FishTrophyColor;
    private static readonly string ColorFishUnique = global::UIHelper.FishUniqueColor;
    private static readonly string ColorFishEvent = global::UIHelper.FishEventColor;
    private static readonly string ColorFishCommon = "FFFFFFFF";

    private static readonly string[] EventFish = [
        "Albino Yeti Gar",
        "Antlered Salmon",
        "Barbel-Ghost",
        "Black Vampire Gar",
        "Bluegill Skeleton",
        "Buzzman Tambaqui",
        "Carp-Ghost",
        "Catfish-Demon",
        "Crystal Burbot",
        "Fierce Muskie",
        "Fire Muskie",
        "Frankenfish",
        "Furry Trout",
        "Ghost Pike",
        "Grass Carp-Ghost",
        "Green Ogre Gar",
        "Hellish Muskie",
        "Hybrid Carp-Ghost",
        "Largemouth Bass-Phantom",
        "Leather Carp-Ghost",
        "Leprechaun Fish",
        "Mirror Carp-Ghost",
        "Northern Snakehead",
        "Prussian Carp-Ghost",
        "Rowdy Bass",
        "Smallmouth Bass-Phantom",
        "Steelhead-Phantom",
        "Sturgeon-Demon",
        "Tarpon-Demon",
        "Trout-Ghost",
        "Trout-Phantom",
        "Trout-Skeleton",
        "White Carp-Ghost"
    ];

    private static readonly string[] XmasFish = [
        "Blue Bandit Aracu",
        "Blue Dunce Crappie",
        "Blue Foolish Goby",
        "Clumsy Krampus Eel",
        "Crumbling Krampus Payara",
        "Decorated Alligator Gar",
        "Decorated Asp",
        "Decorated Chain Pickerel",
        "Decorated Channel Catfish",
        "Decorated Chum Salmon",
        "Decorated Colorado Golden Trout",
        "Decorated Cutthroat Trout",
        "Decorated Ghost Carp",
        "Decorated Lake Trout",
        "Decorated Longnose Gar",
        "Decorated Marble Trout",
        "Decorated Prussian Carp",
        "Decorated Red Drum",
        "Decorated Silver Carp",
        "Decorated Tiger Muskie",
        "Decorated White Crappie",
        "Decorated White Sturgeon",
        "Decorated Zander",
        "Green Bandit Aracu",
        "Green Dunce Crappie",
        "Green Foolish Goby",
        "Red Bandit Aracu",
        "Red Dunce Crappie",
        "Red Foolish Goby",
        "Unlucky Krampus Tarpon"
    ];

    private static readonly string[] HistoricalFish = [
        "Historic Black Crappie",
        "Historic Brown Trout",
        "Historic Bull Trout",
        "Historic Butterfly Peacock Bass",
        "Historic Chain Pickerel",
        "Historic Chinook Salmon",
        "Historic Colorado Golden Trout",
        "Historic Cutthroat Trout",
        "Historic Grass Pickerel",
        "Historic Largemouth Bass",
        "Historic Muskie",
        "Historic Northern Pike",
        "Historic Rainbow Trout",
        "Historic Smallmouth Bass",
        "Historic Spotted Bass",
        "Historic Steelhead Trout",
        "Historic Striped Bass",
        "Historic Walleye",
        "Historic White Bass",
        "Historic White Crappie"
    ];

    public enum FishTypes
    {
        Young,
        Common,
        Trophy,
        Unique,
        Event
    }
}