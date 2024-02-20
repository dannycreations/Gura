using HarmonyLib;
using Il2CppInterop.Runtime;

namespace Gura.Patchs;

[HarmonyPatch(typeof(PlayerIdleThrown))]
public class PlayerIdleThrownPatch
{
    public static bool RequestDrawOut;

    [HarmonyPatch(nameof(PlayerIdleThrown.onUpdate))]
    [HarmonyPrefix]
    public static bool OnUpdate_Prefix(PlayerIdleThrown __instance, ref Il2CppSystem.Type __result)
    {
        if (RequestDrawOut)
        {
            RequestDrawOut = false;
            __instance.Player.RequestedRod = StaticUserData.RodInHand.Rod;
            __result = Il2CppType.Of<PlayerDrawOut>();
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(PlayerIdleThrown.onEnter))]
    [HarmonyPostfix]
    public static void OnEnter_Prefix()
    {
        if (!Container.IsDrawOut)
            RequestDrawOut = false;
    }
}