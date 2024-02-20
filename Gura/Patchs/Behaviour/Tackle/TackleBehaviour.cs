using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch]
public class TackleBehaviourPatch
{
    [HarmonyPatch(typeof(TackleBehaviour))]
    [HarmonyPatch(nameof(TackleBehaviour.ShowGripInAnimDuration))]
    [HarmonyPostfix]
    public static void ShowGripInAnimDuration(ref float __result) =>
        __result = 0.5f;

    [HarmonyPatch(typeof(TackleBehaviour))]
    [HarmonyPatch($"get_{nameof(TackleBehaviour.IsHitched)}")]
    [HarmonyPostfix]
    public static void IsHitched(ref bool __result) =>
        __result = false;

    [HarmonyPatch(typeof(Feeder1stBehaviour))]
    [HarmonyPatch(nameof(Feeder1stBehaviour.Hitch))]
    [HarmonyPrefix]
    public static bool FeederHitch() =>
        false;

    [HarmonyPatch(typeof(Float1stBehaviour))]
    [HarmonyPatch(nameof(Float1stBehaviour.Hitch))]
    [HarmonyPrefix]
    public static bool FloatHitch() =>
        false;

    [HarmonyPatch(typeof(Lure1stBehaviour))]
    [HarmonyPatch(nameof(Lure1stBehaviour.Hitch))]
    [HarmonyPrefix]
    public static bool LureHitch() =>
        false;
}