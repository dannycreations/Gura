using Gura.Utils;
using HarmonyLib;
using System;
using System.Linq;

namespace Gura.Patchs;

[HarmonyPatch]
public class LicensesExpirationCheckerPatch
{
    [HarmonyPatch(typeof(LicensesExpirationChecker))]
    [HarmonyPatch(nameof(LicensesExpirationChecker.Update))]
    [HarmonyPostfix]
    public static void Update(LicensesExpirationChecker __instance)
    {
        if (!Safe.IsResult(() => StateUtil.Player) ||
            __instance._currentTime != 0f
        ) return;

        var stateId = 0;
        if (!Safe.IsResult(() => stateId = StaticUserData.CurrentPond.State.StateId)) return;
        if (!BuyLicenseLock && (
            !StateUtil.Player.HasAdvancedLicense ||
            IsAboutToExpire
        ))
        {
            BuyLicenseLock = true;
            IsAutoRenewLicense = true;

            var allLicenses = CacheLibrary.MapCache.AllLicenses.ToArray();
            var pondLicense = allLicenses.First(r => r.StateId == stateId && r.IsAdvanced);
            var costLicense = pondLicense.Costs.Where(r => r.Term == 1).First();
            StateUtil.Conn.BuyLicense(pondLicense, 1);
            Plugin.Log.LogMessage($"BuyLicense {pondLicense.Name} {costLicense.ResidentCost:N0}");

            BuyLicenseLock = false;
            IsAboutToExpire = false;
        }
        else
        {
            var pondLicense = StateUtil.Profile.Licenses.ToArray().Where(r => r.StateId == stateId && r.IsAdvanced).First();
            if (pondLicense.End.Value <=
                TimeHelper.UtcTime().AddMinutes(5)
            ) IsAboutToExpire = true;
        }
    }

    [HarmonyPatch(typeof(CompleteMessage))]
    [HarmonyPatch(nameof(CompleteMessage.OnLicenseBought))]
    [HarmonyPrefix]
    public static bool OnLicenseBought()
    {
        if (!IsAutoRenewLicense)
            return true;

        IsAutoRenewLicense = false;
        return false;
    }

    private static bool BuyLicenseLock;
    private static bool IsAboutToExpire;
    private static bool IsAutoRenewLicense;
}