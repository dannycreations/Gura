using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch]
public class HangingPatch : TackleBase
{
    [HarmonyPatch(typeof(FeederHanging))]
    [HarmonyPatch(nameof(FeederHanging.onUpdate))]
    [HarmonyPrefix]
    public static bool FeederOnUpdate(FeederHanging __instance, ref Il2CppSystem.Type __result) =>
        FeederTackle(__instance, ref __result);

    [HarmonyPatch(typeof(FloatHanging))]
    [HarmonyPatch(nameof(FloatHanging.onUpdate))]
    [HarmonyPrefix]
    public static bool FloatOnUpdate(FloatHanging __instance, ref Il2CppSystem.Type __result) =>
        FloatTackle(__instance, ref __result);

    [HarmonyPatch(typeof(LureHanging))]
    [HarmonyPatch(nameof(LureHanging.onUpdate))]
    [HarmonyPrefix]
    public static bool LureOnUpdate(LureHanging __instance, ref Il2CppSystem.Type __result) =>
        LureTackle(__instance, ref __result);
}