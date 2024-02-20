using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch(typeof(GameActionAdapter))]
public class GameActionAdapterPatch
{
    [HarmonyPatch(nameof(GameActionAdapter.FightFish))]
    [HarmonyPrefix]
    public static bool FightFish() =>
        false;

    [HarmonyPatch(nameof(GameActionAdapter.FightFishOnPod))]
    [HarmonyPrefix]
    public static bool FightFishOnPod() =>
        false;
}