using ObjectModel;
using System;

namespace Gura.Utils;

public static class CommonUtil
{
    public static void Pause()
    {
        CursorManager.ShowCursor();
        ControlsController.Instance?.BlockInput(null);
        PhotonConnectionFactory.Instance?.Game?.Pause();
        GameFactory.ChatInGameController?.SetActive(false);
        GameFactory.FishSpawner?.OnPondStayFinish(new PondStayFinish());
    }

    public static void Resume()
    {
        CursorManager.HideCursor();
        ControlsController.Instance?.UnBlockInput();
        PhotonConnectionFactory.Instance?.Game?.Resume(true);
        GameFactory.ChatInGameController?.SetActive(true);
        GameFactory.FishSpawner?.OnPondStayProlonged(new ProlongInfo());
    }

    public static void LogFish(string action, Fish fish)
    {
        try
        {
            var fishWeight = $"W({Safe.Result(() => MeasuringSystemManager.FishWeight(fish.Weight)):N2})";
            var fishSilver = $"S({Safe.Result(() => fish.SilverCost.Value):N0})";
            var fishExp = $"E({Safe.Result(() => fish.Experience.Value):N0})";
            Plugin.Log.LogMessage($"{action} {fishWeight} {fishSilver} {fishExp} {fish.Name}");
        }
        catch { Plugin.Log.LogError($"FishError W(0) S(0) E(0)"); }
    }

    public static void LogCage(string action)
    {
        try
        {
            if (Container.FishInCage == null ||
                Container.FishInCage.Count == 0
            ) return;

            var fishWeight = $"W({Safe.Result(() => MeasuringSystemManager.FishWeight(Container.FishInCage.Weight.Value)):N2})";
            var fishSilver = $"S({Safe.Result(() => Container.FishInCage.GetSilverCost().Value):N0})";
            var fishExp = $"E({Safe.Result(() => Container.FishInCage.SumFishExperience()):N0})";
            Plugin.Log.LogMessage($"{action} {fishWeight} {fishSilver} {fishExp}");
        }
        catch { Plugin.Log.LogError($"CageError W(0) S(0) E(0)"); }
    }

    public static bool IsHudBusy =>
        Safe.IsResult(() =>
            IsWindowBusy ||
            CursorManager.IsShowCursor ||
            ShowHudElements.Instance.IsEquipmentChangeBusy()
        , true);

    public static bool IsWindowBusy =>
        CursorManager.IsModalWindow() ||
        CursorManager.IsRewindHoursActive;
}