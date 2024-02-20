using HarmonyLib;
using Il2CppSystem;
using ObjectModel;

namespace Gura.Patchs;

[HarmonyPatch(typeof(Profile))]
public class ProfilePatch
{
    [HarmonyPatch($"get_{nameof(Profile.HasPremium)}")]
    [HarmonyPrefix]
    public static bool HasPremium(Profile __instance, ref bool __result)
    {
        if (Settings.FreePremium)
        {
            __instance.SubscriptionId = new Nullable<int>(1);
            __instance.ExpMultiplier = new Nullable<float>(1f);
            __instance.MoneyMultiplier = new Nullable<float>(1f);
            __instance.SubscriptionEndDate = new Nullable<DateTime>(DateTime.Now.AddDays(360 * 2));
            __result = true;
            return false;
        }

        return true;
    }
}