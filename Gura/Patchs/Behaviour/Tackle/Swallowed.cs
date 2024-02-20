using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch]
public class SwallowedPatch : TackleBase
{
    [HarmonyPatch(typeof(FeederSwallowed))]
    [HarmonyPatch(nameof(FeederSwallowed.onUpdate))]
    [HarmonyPrefix]
    public static bool FeederOnUpdate(FeederSwallowed __instance, ref Il2CppSystem.Type __result) =>
        FeederTackle(__instance, ref __result);

    [HarmonyPatch(typeof(FloatSwallowed))]
    [HarmonyPatch(nameof(FloatSwallowed.onUpdate))]
    [HarmonyPrefix]
    public static bool FloatOnUpdate(FloatSwallowed __instance, ref Il2CppSystem.Type __result) =>
        FloatTackle(__instance, ref __result);

    [HarmonyPatch(typeof(LureSwallowed))]
    [HarmonyPatch(nameof(LureSwallowed.onUpdate))]
    [HarmonyPrefix]
    public static bool LureOnUpdate(LureSwallowed __instance, ref Il2CppSystem.Type __result) =>
        LureTackle(__instance, ref __result);

    [HarmonyPatch(typeof(FeederSwallowed))]
    [HarmonyPatch(nameof(FeederSwallowed.DetectStriking))]
    [HarmonyPrefix]
    public static bool FeederDetectStriking() =>
        false;

    [HarmonyPatch(typeof(FloatSwallowed))]
    [HarmonyPatch(nameof(FloatSwallowed.DetectStriking))]
    [HarmonyPrefix]
    public static bool FloatDetectStriking() =>
        false;

    [HarmonyPatch(typeof(LureSwallowed))]
    [HarmonyPatch(nameof(LureSwallowed.DetectStriking))]
    [HarmonyPrefix]
    public static bool LureDetectStriking() =>
        false;
}