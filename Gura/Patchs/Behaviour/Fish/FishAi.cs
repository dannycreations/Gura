using FishRefactoring;
using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch(typeof(FishAi))]
public class FishAiPatch
{
    [HarmonyPatch(nameof(FishAi.GenerateRandomEscape))]
    [HarmonyPrefix]
    public static bool GenerateRandomEscape() =>
        false;

    [HarmonyPatch(nameof(FishAi.GenerateEscapePointForPlay))]
    [HarmonyPrefix]
    public static bool GenerateEscapePointForPlay() =>
        false;

    [HarmonyPatch(nameof(FishAi.GenerateEscapePointForPool))]
    [HarmonyPrefix]
    public static bool GenerateEscapePointForPool() =>
        false;

    [HarmonyPatch(nameof(FishAi.GenerateEscapePointForFinalEscape))]
    [HarmonyPrefix]
    public static bool GenerateEscapePointForFinalEscape() =>
        false;
}