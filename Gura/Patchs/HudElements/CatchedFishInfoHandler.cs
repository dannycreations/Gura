using Gura.Utils;
using HarmonyLib;
using ObjectModel;

namespace Gura.Patchs;

[HarmonyPatch(typeof(CatchedFishInfoHandler))]
public class CatchedFishInfoHandlerPatch
{
    [HarmonyPatch(nameof(CatchedFishInfoHandler.OpenMessage))]
    [HarmonyPostfix]
    public static void OpenMessage(CatchedFishInfoHandler __instance)
    {
        CaughtFish = CatchedFishInfoHandler._caughtFish;
        if (!Settings.AutoCatch) return;

        if (StateUtil.FishCage.CanAdd(CaughtFish))
            __instance.TakeClick();
        else
            __instance.ReleaseClick();
        __instance.CloseMessage();
    }

    [HarmonyPatch(nameof(CatchedFishInfoHandler.TakeClick))]
    [HarmonyPostfix]
    public static void TakeClick() =>
        CommonUtil.LogFish("Take", CaughtFish);

    [HarmonyPatch(nameof(CatchedFishInfoHandler.ReleaseClick))]
    [HarmonyPostfix]
    public static void ReleaseClick() =>
        CommonUtil.LogFish("Release", CaughtFish);

    private static Fish CaughtFish;
}