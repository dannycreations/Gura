using Gura.Utils;
using HarmonyLib;
using ObjectModel;

namespace Gura.Patchs;

[HarmonyPatch(typeof(RewindHoursHandler))]
public class RewindHoursHandlerPatch
{
    [HarmonyPatch(nameof(RewindHoursHandler.StartChangeTime))]
    [HarmonyPrefix]
    public static void StartChangeTime()
    {
        // Temp fix
        Container.FishInCage.fishList.Clear();
        foreach (var fish in StateUtil.FishCage.fishList)
            Container.FishInCage.fishList.Add(fish.MemberwiseClone().Cast<CaughtFish>());
    }
}