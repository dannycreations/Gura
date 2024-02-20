using Gura.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace Gura.Patchs;

[HarmonyPatch]
public class FloatingPatch
{
    [HarmonyPatch(typeof(LureFloating))]
    [HarmonyPatch(nameof(LureFloating.onUpdate))]
    [HarmonyPrefix]
    public static bool OnUpdate(LureFloating __instance, ref Il2CppSystem.Type __result)
    {
        if (__instance.AssembledRod.IsRodDisassembled)
        {
            __result = Il2CppType.Of<LureBroken>();
            return false;
        }

        var rod = __instance.RodSlot;
        if (rod.Tackle.HasHitTheGround ||
            rod.Tackle.IsOutOfTerrain)
        {
            StateUtil.GameAdapter.FinishGameAction();
            __result = Il2CppType.Of<LureOnTip>();
            return false;
        }
        else
        {
            if (rod.Tackle.Fish != null)
            {
                __result = Il2CppType.Of<LureSwallowed>();
                return false;
            }

            if (__instance.IsInHands)
            {
                if (rod.Line.SecuredLineLength <= rod.Line.MinLineLength)
                {
                    StateUtil.GameAdapter.FinishGameAction();
                    __result = Il2CppType.Of<LureOnTip>();
                    return false;
                }

                if (__instance.timeSpent < 2f)
                {
                    __instance.timeSpent += Time.deltaTime;
                    if (__instance.timeSpent > 2f)
                        rod.Reel.IsIndicatorOn = true;
                }
                rod.Tackle.UpdateLureDepthStatus();
            }
        }

        rod.Tackle.CheckSurfaceCollisions();
        StateUtil.GameAdapter.Move(false);
        return false;
    }

    [HarmonyPatch(typeof(FeederFloating))]
    [HarmonyPatch(nameof(FeederFloating.DetectEarlyStriking))]
    [HarmonyPrefix]
    public static bool FeederDetectEarlyStriking() =>
        false;

    [HarmonyPatch(typeof(FeederFloating))]
    [HarmonyPatch(nameof(FeederFloating.CheckFishEscape))]
    [HarmonyPrefix]
    public static bool FeederCheckFishEscape() =>
        false;

    [HarmonyPatch(typeof(FloatFloating))]
    [HarmonyPatch(nameof(FloatFloating.DetectEarlyStriking))]
    [HarmonyPrefix]
    public static bool FloatDetectEarlyStriking() =>
        false;

    [HarmonyPatch(typeof(FloatFloating))]
    [HarmonyPatch(nameof(FloatFloating.CheckFishEscape))]
    [HarmonyPrefix]
    public static bool FloatCheckFishEscape() =>
        false;
}