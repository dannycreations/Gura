using FishRefactoring;
using HarmonyLib;
using Il2CppInterop.Runtime;
using ObjectModel;

namespace Gura.Patchs;

[HarmonyPatch]
public class FishBehaviourPatch
{
    [HarmonyPatch(typeof(FishHooked))]
    [HarmonyPatch(nameof(FishHooked.onUpdate))]
    [HarmonyPrefix]
    public static bool OnUpdateHooked(FishHooked __instance, ref Il2CppSystem.Type __result)
    {
        if (__instance.Fish.Tackle == null ||
            __instance.Fish.Behavior == FishBehavior.Go)
            __result = Il2CppType.Of<FishEscape>();
        else if (__instance.Fish.Tackle.IsShowing)
            if (__instance.Fish.IsBig)
                __result = Il2CppType.Of<FishShowBig>();
            else
                __result = Il2CppType.Of<FishShowSmall>();

        return false;
    }

    [HarmonyPatch(typeof(FishAttack))]
    [HarmonyPatch(nameof(FishAttack.onUpdate))]
    [HarmonyPrefix]
    public static bool OnUpdateAttack(FishAttack __instance, ref Il2CppSystem.Type __result)
    {
        if (__instance.Fish.Tackle == null ||
            __instance.Fish.Behavior == FishBehavior.Go)
            __result = Il2CppType.Of<FishEscape>();
        else if (__instance.Fish.IsPathCompleted)
            __result = Il2CppType.Of<FishHooked>();

        return false;
    }

    [HarmonyPatch(typeof(FishEscape))]
    [HarmonyPatch(nameof(FishEscape.onUpdate))]
    [HarmonyPrefix]
    public static bool OnUpdateEscape(ref Il2CppSystem.Type __result)
    {
        __result = Il2CppType.Of<FishDestroy>();
        return false;
    }

    [HarmonyPatch(typeof(Fish1stBehaviour))]
    [HarmonyPatch(nameof(Fish1stBehaviour.Start))]
    [HarmonyPrefix]
    public static void Start(Fish1stBehaviour __instance)
    {
        __instance.FishTemplate.Portrait = null;
        __instance.Portrait = null;

        var fishVector = new Il2CppSystem.Nullable<FishAttackStyle>(FishAttackStyle.Fore);
        __instance.FishTemplate.AttackVector = fishVector;

        var fishActivity = new Il2CppSystem.Nullable<float>(1f);
        __instance.FishTemplate.Activity = fishActivity;
        __instance.Activity = fishActivity.Value;

        var fishLure = new Il2CppSystem.Nullable<float>(0f);
        __instance.FishTemplate.AttackLure = fishLure;
        __instance.AttackLure = fishLure.Value;

        if (__instance.FishTemplate.BiteTime != null)
        {
            var fishBite = new Il2CppSystem.Nullable<float>(0f);
            __instance.FishTemplate.BiteTime = fishBite;
            __instance.BiteTime = fishBite.Value;
        }
    }
}