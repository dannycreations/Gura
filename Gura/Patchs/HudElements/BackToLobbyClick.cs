using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch(typeof(BackToLobbyClick))]
public class BackToLobbyClickPatch
{
    [HarmonyPatch(nameof(BackToLobbyClick.SendToHome_ActionCalled))]
    [HarmonyPrefix]
    public static void SendToHome_ActionCalled()
    {
        if (Settings.AntiHUD)
        {
            Settings.AntiHUD = false;
            Container.HUDInstance.Start();
        }
    }
}