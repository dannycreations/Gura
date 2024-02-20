using Gura.Utils;
using HarmonyLib;
using System;

namespace Gura.Patchs;

[HarmonyPatch(typeof(ProlongationOfStay))]
public class ProlongationOfStayPatch
{
    [HarmonyPatch(nameof(ProlongationOfStay.OpenProlongationWindow))]
    [HarmonyPrefix]
    public static bool OpenProlongationWindow(ProlongationOfStay __instance)
    {
        Container.IsDrawOut = true;
        __instance._isShowWindow = true;
        Plugin.Log.LogInfo($"{nameof(ProlongationOfStay)} {nameof(ProlongationOfStay.OpenProlongationWindow)}");
        __instance.BuyProlongationClick();
        return false;
    }

    [HarmonyPatch(nameof(ProlongationOfStay.BuyProlongationClick))]
    [HarmonyPrefix]
    public static void BuyProlongationClick(ProlongationOfStay __instance)
    {
        Safe.IsResult(() => CatchedFishInfoHandler.Instance.CloseMessage());
        Safe.IsResult(() => CatchedFishInfoHandler.Instance.CloseMessage());

        Plugin.Log.LogInfo($"{nameof(ProlongationOfStay)} {nameof(ProlongationOfStay.BuyProlongationClick)} {__instance.CurrentPondStayCost}");

        CommonUtil.LogCage("EndOfDay");
        Container.FishInCage.Clear();
        Container.IsDrawOut = false;
    }
}