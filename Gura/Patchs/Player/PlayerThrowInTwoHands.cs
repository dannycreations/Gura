using HarmonyLib;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace Gura.Patchs;

[HarmonyPatch(typeof(PlayerThrowInTwoHands))]
public class PlayerThrowInTwoHandsPatch
{
    [HarmonyPatch(nameof(PlayerThrowInTwoHands.onUpdate))]
    [HarmonyPrefix]
    public static bool onUpdate(PlayerThrowInTwoHands __instance, ref Il2CppSystem.Type __result)
    {
        if (!Settings.AutoCasting)
            return true;

        if (__instance.canThrow != __instance.Player.CanThrow &&
            __instance.Player.CanThrow)
        {
            __instance.canThrow = true;
            __instance.Player.BeginThrowPowerGainProcess();
        }

        float num;
        if (__instance._throwPowerGainProgress._isForwardProgress)
        {
            num = __instance.Player.GetThrowPowerGainProgress(4f, 12f);
            if (Mathf.Approximately(num, 1f))
            {
                __instance._throwPowerGainProgress._isForwardProgress = false;
                __instance.Player.BeginThrowTime = Time.time;
            }
        }
        else
        {
            num = __instance.Player.GetThrowPowerGainProgressBack(12f, 4f);
            if (Mathf.Approximately(num, 0f))
            {
                __instance._throwPowerGainProgress._isForwardProgress = true;
                __instance.Player.BeginThrowTime = Time.time;
            }
        }

        var currentValue = num * __instance.Player.Rod.MaxCastLength;
        if (currentValue < PreviousValue)
        {
            IsMaxCast = true;
            num = PreviousForce;
            currentValue = PreviousValue;
        }
        else
            __instance.Player.HudFishingHandler.CastSimpleHandler.CurrentValue = currentValue;

        var lineLength = __instance.Player.Rod.LineOnRodLength + __instance.Player.Line.MaxLineLength;
        if ((currentValue >= lineLength) ||
            IsMaxCast)
        {
            IsMaxCast = false;
            PreviousForce = 0;
            PreviousValue = 0;

            __instance.Player.Tackle.ThrowData.CastLength = Mathf.Max((float)__instance.Player.Rod.AssembledRod.Rod.Length.Value, currentValue);
            __instance.Player.Tackle.ThrowData.ThrowForce = num;
            __instance.Player.Tackle.ThrowData.AccuracyRatio = 0f;
            __result = Il2CppType.Of<PlayerThrowOutTwoHands>();
        }
        else
        {
            PreviousForce = num;
            PreviousValue = currentValue;
        }

        return false;
    }

    private static bool IsMaxCast;
    private static float PreviousForce;
    private static float PreviousValue;
}