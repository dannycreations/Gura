using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch(typeof(InfoServerMessagesHandler))]
public class InfoServerMessagesHandlerPatch
{
    [HarmonyPatch(nameof(InfoServerMessagesHandler.Awake))]
    [HarmonyPrefix]
    public static bool Awake(InfoServerMessagesHandler __instance)
    {
        Container.HUDInstance ??= __instance;
        return !Settings.AntiHUD;
    }

    [HarmonyPatch(nameof(InfoServerMessagesHandler.Start))]
    [HarmonyPrefix]
    public static bool Start() =>
        !Settings.AntiHUD;

    [HarmonyPatch(nameof(InfoServerMessagesHandler.Update))]
    [HarmonyPrefix]
    public static bool Update() =>
        !Settings.AntiHUD;
}