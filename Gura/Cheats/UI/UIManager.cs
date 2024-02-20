using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using Gura.Utils;
using Il2CppInterop.Runtime;
using ObjectModel;
using System;
using UnityEngine;

namespace Gura.Cheats;

public class UIManager : CheatBase
{
    public override void OnDisplay()
    {
        if (!Settings.MenuVisible) return;

        GUI.color = Color.white;

        CommonWindow();

        FishingWindow();
        if (Settings.FishSetting)
            FishSettingWindow();
        else if (Settings.CastSetting)
            CastSettingWindow();
        else if (Settings.TimeSetting)
            TimeSettingWindow();

        ActivityWindow();
    }

    private void CommonWindow()
    {
        UIHelper.Begin("COMMONS", 10f, 10f, 165f, 240f);
        Settings.ESP = UIHelper.ButtonState("ESP: ", Settings.ESP);
        Settings.FreePremium = UIHelper.ButtonState("Free Premium: ", Settings.FreePremium);
        Settings.GameBooster = UIHelper.ButtonState("Game Booster: ", Settings.GameBooster);

        if (UIHelper.Button("Anti-HUD: ", Settings.AntiHUD))
        {
            Settings.AntiHUD = !Settings.AntiHUD;
            if (Settings.AntiHUD)
                Container.HUDInstance.OnDestroy();
            else
                Container.HUDInstance.Start();
        }

        Settings.AntiGoldCoins = UIHelper.ButtonState("Anti-GoldCoins: ", Settings.AntiGoldCoins);

        if (UIHelper.Button("Back To Lobby"))
        {
            var lobby = ShowLocationInfo.Instance.GetComponent<BackToLobbyClick>();
            lobby.SendToHome_ActionCalled(null, null);
        }

        if (UIHelper.Button("Force Restart Game"))
            MenuHelpers.Instance.RestartGame();
    }

    private void FishingWindow()
    {
        UIHelper.Begin("FISHING", 180f, 10f, 165f, 240f);
        if (Settings.FishBotLock)
        {
            if (UIHelper.Button($"Fish Bot Locked: ", Settings.FishBotLock))
            {
                Container.IsDrawOut = false;
                Settings.FishBotLock = false;
            }
            return;
        }

        Settings.FishBot = UIHelper.ButtonState("Fish Bot: ", Settings.FishBot);

        if (UIHelper.Button("Show Fish Setting"))
        {
            Settings.CastSetting = false;
            Settings.TimeSetting = false;
            Settings.FishSetting = !Settings.FishSetting;
        }

        if (UIHelper.Button("Show Cast Setting"))
        {
            Settings.FishSetting = false;
            Settings.TimeSetting = false;
            Settings.CastSetting = !Settings.CastSetting;
        }

        if (UIHelper.Button("Show Time Setting"))
        {
            Settings.FishSetting = false;
            Settings.CastSetting = false;
            Settings.TimeSetting = !Settings.TimeSetting;
        }

        UIHelper.Label("====================");
        if (UIHelper.Button("Reset Player State"))
        {
            Container.IsDrawOut = false;
            StateUtil.Player.fsm.EnterState(Il2CppType.Of<PlayerDrawOut>());
        }

        if (UIHelper.Button("Reset Rod Pods"))
            StateUtil.Player.CleanRodPods();

        if (UIHelper.Button("Repair All Equipment"))
        {
            var i = 1;
            var costTotal = 0f;
            foreach (var item in StateUtil.Inventory.Items)
            {
                if (!item.IsRepairable || item.Storage == StoragePlaces.Storage) continue;

                var maxDurability = item.MaxDurability;
                var num = (maxDurability == null) ? null : new float?(maxDurability.Value);
                var num2 = (num == null) ? null : new float?(num.GetValueOrDefault());
                if (item.Durability < num2 && item.Durability > 0f)
                {
                    StateUtil.Inventory.PreviewRepair(item, out int damaged, out float cost, out string currency);
                    if (Settings.AntiGoldCoins && currency == "GC") continue;
                    if (damaged > 0 && (double)cost < StateUtil.Profile.GetBalance(currency))
                    {
                        costTotal += cost;
                        StateUtil.Conn.RepairItem(item);
                        Plugin.Log.LogInfo($"{i++}. {item.Name.Humanize()} ({damaged:N0}/{cost:N0})");
                    }
                }
            }
            if (costTotal > 0f)
                Plugin.Log.LogMessage($"Total repair cost {costTotal:N0}");
            else
                Plugin.Log.LogMessage($"No equipment needs to be repaired");
        }
    }

    private void FishSettingWindow()
    {
        UIHelper.Begin("FISHING SETTING", 350f, 10f, 165f, 240f);
        Settings.StrongRod = UIHelper.ButtonState("Strong Rod: ", Settings.StrongRod);
        Settings.InstantCatch = UIHelper.ButtonState("Instant Catch: ", Settings.InstantCatch);
        Settings.FishEventOnly = UIHelper.ButtonState("Event Only: ", Settings.FishEventOnly);
        Settings.FishTrophyOnly = UIHelper.ButtonState("Trophy Only: ", Settings.FishTrophyOnly);
        Settings.FishUniqueOnly = UIHelper.ButtonState("Unique Only: ", Settings.FishUniqueOnly);

        if (Settings.FishTrophyOnly || Settings.FishUniqueOnly)
            Settings.FishEventOnly = false;

        Settings.AutoCatch = UIHelper.ButtonState("Auto Catch: ", Settings.AutoCatch);
    }

    private void CastSettingWindow()
    {
        UIHelper.Begin("CASTING SETTING", 350f, 10f, 165f, 240f);
        Settings.AutoCasting = UIHelper.ButtonState("Auto Casting: ", Settings.AutoCasting);
        Settings.AutoDragStyle = UIHelper.ButtonState("Auto Drag Style: ", Settings.AutoDragStyle);

        Settings.DragStyleStopNGo = UIHelper.ButtonState("Drag Stop&Go: ", Settings.DragStyleStopNGo);

        if (UIHelper.Button("Pos Surface: ", Settings.DragPosSurface))
        {
            Settings.DragPosMid = false;
            Settings.DragPosNearBottom = false;
            Settings.DragPosOnBottom = false;
            Settings.DragPosSurface = !Settings.DragPosSurface;
        }

        if (UIHelper.Button("Pos Mid: ", Settings.DragPosMid))
        {
            Settings.DragPosSurface = false;
            Settings.DragPosNearBottom = false;
            Settings.DragPosOnBottom = false;
            Settings.DragPosMid = !Settings.DragPosMid;
        }

        if (UIHelper.Button("Pos NearBottom: ", Settings.DragPosNearBottom))
        {
            Settings.DragPosSurface = false;
            Settings.DragPosMid = false;
            Settings.DragPosOnBottom = false;
            Settings.DragPosNearBottom = !Settings.DragPosNearBottom;
        }

        if (UIHelper.Button("Pos OnBottom: ", Settings.DragPosOnBottom))
        {
            Settings.DragPosSurface = false;
            Settings.DragPosMid = false;
            Settings.DragPosNearBottom = false;
            Settings.DragPosOnBottom = !Settings.DragPosOnBottom;
        }

        UIHelper.Label($"Auto Line Clip: ", Settings.AutoReelClip);
        if (UIHelper.TextFieldWithApply(ref Settings.ReelClipTextField))
        {
            foreach (var rod in StateUtil.RodSlots)
                if (rod.LineClips.Count > 0)
                    rod.LineClips.Clear();
            Settings.AutoReelClip = !Settings.AutoReelClip;
        }
    }

    private void TimeSettingWindow()
    {
        UIHelper.Begin("TIME SETTING", 350f, 10f, 165f, 240f);
        if (Settings.TimeSettingLock)
        {
            if (UIHelper.Button($"Time Setting Locked: ", Settings.TimeSettingLock))
            {
                Container.IsDrawOut = false;
                Settings.TimeSettingLock = false;
            }
            return;
        }

        if (UIHelper.Button($"Day Peak Only: ", Settings.DayPeakOnly))
        {
            Settings.RewindHours = false;
            Settings.RewindHoursStep = false;
            Settings.NightPeakOnly = false;
            Settings.DayPeakOnly = !Settings.DayPeakOnly;
        }

        if (UIHelper.Button($"Night Peak Only: ", Settings.NightPeakOnly))
        {
            Settings.RewindHours = false;
            Settings.RewindHoursStep = false;
            Settings.DayPeakOnly = false;
            Settings.NightPeakOnly = !Settings.NightPeakOnly;
        }

        UIHelper.Label($"Rewind Hours: ", Settings.RewindHours);
        if (UIHelper.TextField2WithApply(ref Settings.RewindHoursMinField, ref Settings.RewindHoursMaxField))
        {
            Settings.DayPeakOnly = false;
            Settings.NightPeakOnly = false;
            Settings.RewindHours = !Settings.RewindHours;
        }

        UIHelper.Label($"Rewind Hours Step: ", Settings.RewindHoursStep);
        if (UIHelper.TextField2WithApply(ref Settings.RewindHoursStepAField, ref Settings.RewindHoursStepBField))
        {
            Settings.DayPeakOnly = false;
            Settings.NightPeakOnly = false;
            Settings.RewindHoursStep = !Settings.RewindHoursStep;
        }
    }

    private void ActivityWindow()
    {
        if (Settings.FishSetting ||
            Settings.CastSetting ||
            Settings.TimeSetting)
            UIHelper.Begin("ACTIVITY", 520f, 10f, 300f, 240f);
        else
            UIHelper.Begin("ACTIVITY", 350f, 10f, 300f, 240f);

        UIHelper.Label($"Player: {Container.PlayerState ?? "None"}");
        UIHelper.Label($"Tackle: {Container.TackleState ?? "None"} / {Safe.Result(() => StateUtil.Player.Tackle.tackleStatus.ToString(), "None")}");
        if (Container.FishInHand == null)
            UIHelper.Label($"Fish: {Container.FishState ?? "None"}");
        else
        {
            UIHelper.Label($"Fish {Container.FishInHand.SlotId}: {Container.FishState}");
            UIHelper.Label($"Name: {Container.FishInHand.ToFormatName()}");
            UIHelper.Label($"Weight: {Container.FishInHand.ToFormatWeight()}");
            UIHelper.Label($"Length: {Container.FishInHand.ToFormatLength()}");
        }

        if (UIHelper.Button("Show Rewind Panel"))
            ShowHudElements.Instance.RewindHours.RequestCooldownAndShowPanel();
        if (UIHelper.Button("Show Inventory Menu"))
        {
            KeysHandlerAction.InventoryHandler();
            PlayerStateBase._lastPage = MainMenuPage.Inventory;
        }

        if (UIHelper.Button("Debug Button"))
        {
            //Plugin.Log.LogInfo(Container.WeatherPeakTime[1].Stringify());

            //StaticUserData.CurrentPond.StringifyToFile("currentpond", false);
            //LocalizationManager.Sources.StringifyToFile("localization", false);
            //StateUtil.Inventory.Items.StringifyToFile("inventory", false);

            //var writer = new StringWriter();
            //foreach (var entry in StateUtil.Conn.globalVariables)
            //    writer.WriteLine("Key = '{0}'; Value = '{0}'", entry.Key.Stringify(), entry.Value.Stringify());
            //writer.ToString().ToFile("globalvariables", false);
        }
    }
}