using Gura.Utils;
using HarmonyLib;
using ObjectModel;

namespace Gura.Patchs;

[HarmonyPatch]
public class RodBehaviourPatch
{
    [HarmonyPatch(typeof(Line1stBehaviour))]
    [HarmonyPatch($"get_{nameof(Line1stBehaviour.IsTensioned)}")]
    [HarmonyPostfix]
    public static void IsTensioned(Line1stBehaviour __instance, ref bool __result)
    {
        if (!(__instance.Tackle.Fish != null && (
            (TackleUtil.IsLure &&
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook) ||
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook
        ))) return;

        __result = true;
    }

    [HarmonyPatch(typeof(ReelBehaviour))]
    [HarmonyPatch($"get_{nameof(ReelBehaviour.CurrentFrictionForce)}")]
    [HarmonyPostfix]
    public static void CurrentFrictionForce(ReelBehaviour __instance, ref float __result)
    {
        if (!(Settings.StrongRod &&
            __instance.Tackle.Fish != null && (
            (TackleUtil.IsLure &&
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook) ||
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook
        ))) return;

        __result = float.MaxValue;
    }

    [HarmonyPatch(typeof(Rod1stBehaviour))]
    [HarmonyPatch(nameof(Rod1stBehaviour.CalculateAppliedForce))]
    [HarmonyPostfix]
    public static void RodCalculateAppliedForce(Rod1stBehaviour __instance)
    {
        if (!(Settings.StrongRod &&
            __instance.Tackle.Fish != null && (
            (TackleUtil.IsLure &&
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook) ||
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook
        ))) return;

        __instance.AppliedForce = 0f;
    }

    [HarmonyPatch(typeof(Reel1stBehaviour))]
    [HarmonyPatch(nameof(Reel1stBehaviour.CalculateAppliedForce))]
    [HarmonyPostfix]
    public static void ReelCalculateAppliedForce(Reel1stBehaviour __instance)
    {
        if (!(Settings.StrongRod &&
            __instance.Tackle.Fish != null && (
            (TackleUtil.IsLure &&
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook) ||
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook
        ))) return;

        __instance.AppliedForce = 0f;
    }

    [HarmonyPatch(typeof(Line1stBehaviour))]
    [HarmonyPatch(nameof(Line1stBehaviour.CalculateAppliedForce))]
    [HarmonyPostfix]
    public static void LineCalculateAppliedForce(Line1stBehaviour __instance)
    {
        if (!(Settings.StrongRod &&
            __instance.Tackle.Fish != null && (
            (TackleUtil.IsLure &&
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook) ||
            __instance.Tackle.Fish.Behavior == FishBehavior.Hook
        ))) return;

        __instance.AppliedForce = 0f;
    }
}