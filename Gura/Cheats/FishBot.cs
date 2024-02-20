using Assets.Scripts.Common.Managers;
using Gura.Utils;
using ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitsNet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gura.Cheats;

public class FishBot : CheatBase
{
    public override void OnUpdate()
    {
        if (!Settings.FishBot ||
            !Safe.IsResult(() => StateUtil.Player) ||
            !Safe.IsResult(() => StateUtil.Profile) ||
            !Safe.IsResult(() => StateUtil.Fish) ||
            StateUtil.FishSpawner.IsGamePaused)
        {
            Container.FishState = null;
            Container.PlayerState = null;
            Container.TackleState = null;
            return;
        }

        Container.PlayerState = Safe.Result(() => StateUtil.Player.State.FullName);
        Container.TackleState = Safe.Result(() => StateUtil.Player.Tackle.State.FullName);
        Container.TackleStatus = Safe.Result(() => StateUtil.Player.Tackle.tackleStatus);

        TimeAndWeatherActivity();
        PlayerActivity();
        FishActivity();
    }

    private void FishActivity()
    {
        if (Settings.FishBotLock ||
            Container.IsDrawOut ||
            CommonUtil.IsHudBusy
        ) return;

        if (Settings.AutoCasting)
            PlayerUtil.TryCasting();

        if (Container.PlayerState == nameof(PlayerIdleThrown))
        {
            if (StateUtil.Player.Tackle.HasHitTheGround ||
                StateUtil.Player.Tackle.IsOutOfTerrain ||
                StateUtil.Player.Tackle.IsPitchTooShort
            ) return;

            if (Settings.AutoDragStyle)
            {
                if (TackleUtil.IsLure)
                    if (TackleUtil.IsFloating)
                    {
                        var isForce = false;
                        if (StateUtil.Fish.Count == 0 &&
                            StateUtil.Player.RodSlot.Line.FullLineLength <= Length.FromFeet(40).Meters)
                            isForce = true;

                        if (Settings.DragStyleStopNGo)
                            MouseSimulator.TryDrag(DragStyle.StopNGo, true, isForce);
                    }
                    else
                        MouseSimulator.TryDrag(null, false, true);
                else
                    Settings.AutoDragStyle = false;
            }
        }

        if (StateUtil.Fish.Count == 0)
        {
            Container.FishState = null;
            Container.FishInHand = null;
            Container.IgnoredFish.Clear();
            MouseSimulator.TryStrike(false);
            return;
        }

        foreach (var fish in StateUtil.Fish.Values)
        {
            Container.FishState = fish.State.FullName;

            if (fish.Behavior == FishBehavior.Go ||
                Container.IgnoredFish.Contains(fish.InstanceGuid))
            {
                if (fish.RodSlot.IsInHands)
                {
                    Container.FishState = null;
                    Container.FishInHand = null;
                    MouseSimulator.TryStrike(false);
                }

                fish.SafeEscape();
                continue;
            }

            if ((float)fish.FishTemplate.Weight >= StateUtil.FishCage.Cage.MaxFishWeight)
            {
                CommonUtil.LogFish("Overweight", fish.FishTemplate);
                Container.IgnoredFish.Add(fish.InstanceGuid);
                continue;
            }

            var fishType = fish.FishTemplate.GetFishType();
            if (Settings.FishEventOnly &&
                fishType != FishUtil.FishTypes.Event)
            {
                CommonUtil.LogFish("Not event fish", fish.FishTemplate);
                Container.IgnoredFish.Add(fish.InstanceGuid);
                continue;
            }
            else if (Settings.FishTrophyOnly &&
                fishType != FishUtil.FishTypes.Trophy &&
                fishType != FishUtil.FishTypes.Unique)
            {
                CommonUtil.LogFish("Not trophy fish", fish.FishTemplate);
                Container.IgnoredFish.Add(fish.InstanceGuid);
                continue;
            }
            else if (Settings.FishUniqueOnly &&
                fishType != FishUtil.FishTypes.Unique)
            {
                CommonUtil.LogFish("Not unique fish", fish.FishTemplate);
                Container.IgnoredFish.Add(fish.InstanceGuid);
                continue;
            }

            switch (Container.FishState)
            {
                case nameof(FishBite):
                case nameof(FishSwim):
                case nameof(FishAttack):
                case nameof(FishEscape):
                case nameof(FishPredatorSwim):
                case nameof(FishPredatorAttack):
                    break;

                case nameof(FishHooked):
                case nameof(FishSwimAway):
                    fish.ai.stopFightProbability = 0f;
                    fish.ai.allEscapesProbability = 0f;
                    fish.Tackle.StrikeTimeEnd = Time.time + 1f;

                    if (!fish.RodSlot.IsInHands)
                        PlayerUtil.TryToTakeRodFromSlot(fish.SlotId, TakeRodState.Replace);

                    if (fish.RodSlot.IsInHands && (
                        fish.isHooked ||
                        fish.Behavior == FishBehavior.Hook
                    ))
                    {
                        Container.FishInHand = fish;
                        if (!Settings.StrongRod)
                        {
                            fish.ai.Force = 0f;
                            fish.ai.CurrentForce = 0f;
                        }

                        if (Keyboard.current[Key.Numpad1].wasPressedThisFrame &&
                            !Container.IgnoredFish.Contains(fish.InstanceGuid))
                        {
                            CommonUtil.LogFish("Force escape", fish.FishTemplate);
                            Container.IgnoredFish.Add(fish.InstanceGuid);
                            StateUtil.GameAdapter.FinishGameAction();
                            fish.Tackle.EscapeFish();
                            break;
                        }

                        if (Settings.AutoDragStyle)
                            MouseSimulator.TryDrag(null, false, true);

                        MouseSimulator.TryStrike(true);
                    }
                    break;

                case nameof(FishShowBig):
                case nameof(FishShowSmall):
                    Container.FishInHand = null;
                    MouseSimulator.TryStrike(false);
                    break;

                default:
                    Plugin.Log.LogWarning($"FishActivity {Container.FishState} {fish.FishAIBehaviour}");
                    break;
            }
        }
    }

    private void PlayerActivity()
    {
        if (Settings.FishBotLock) return;

        //ShowLocationInfo.Instance._roomsTglsManager.SelectRoomWindow("private");

        //if (Settings.BrokenRodSlotId.Count > 0)
        //{
        //    foreach (var slotId in Settings.BrokenRodSlotId)
        //        if (!Safe.Result(() => StateUtil.RodSlots[slotId]?.Rod.RodAssembly.IsRodDisassembled ?? true))
        //            Settings.BrokenRodSlotId.Remove(slotId);
        //}

        if (!Safe.IsResult(() => StateUtil.FishCage.Cage.TotalWeight))
        {
            Settings.FishBotLock = true;
            KeysHandlerAction.InventoryHandler();
            PlayerStateBase._lastPage = MainMenuPage.Inventory;
            Plugin.Log.LogWarning("Fish cage maybe broken!");
            return;
        }

        if (StateUtil.Player.IsIdle &&
            StateUtil.Player.RodSlot.IsInHands)
        {
            var item = StaticUserData.RodInHand.Leader;
            if ((item.Durability / (float)item.MaxDurability.Value * 100f) <= 10f)
            {
                var getNewItem = (from r in RodUtil.GetItemsAtBackpack()
                                  where r.ItemId == item.ItemId
                                  select r).First();

                ItemUtil.MakeSplit(StaticUserData.RodInHand.Rod, getNewItem, item, 1);
            }

            var rodLength = StateUtil.Player.RodSlot.Rod.ModelLength * 0.85f * 3;
            if (StateUtil.Player.RodSlot.Line.MinLineLengthWithFish != rodLength)
                StateUtil.Player.RodSlot.Line.MinLineLengthWithFish = rodLength;

            var reelSpeedCurrent = StateUtil.Player.Reel.currentReelSpeedSection;
            var reelSpeedMax = StateUtil.Player.Reel.Owner.numReelSpeedSections - 1;
            if (reelSpeedCurrent != reelSpeedMax)
            {
                StateUtil.Player.Reel.currentReelSpeedSection = reelSpeedMax;
                StateUtil.Player.Reel.PersistSpeed();
            }

            var reelFrictionCurrent = StateUtil.Player.Reel.currentFrictionSection;
            var reelFrictionMax = StateUtil.Player.Reel.Owner.numFrictionSections - 1;
            if (reelFrictionCurrent != reelFrictionMax)
            {
                StateUtil.Player.Reel.currentFrictionSection = reelFrictionMax;
                StateUtil.Player.Reel.PersistFriction();
            }

            if (Settings.AutoReelClip &&
                StateUtil.Player.RodSlot.LineClips.Count == 0)
            {
                var maxLength = StateUtil.Player.Rod.MaxCastLength;
                var reelClip = Settings.ReelClipTextField.Split(" ");
                var clipLineLength = double.Parse(reelClip[0]);

                var metricValue = (float)clipLineLength;
                if (reelClip[1] == "FT")
                    metricValue = (float)Length.FromFeet(clipLineLength).Meters + 0.3f - StateUtil.Player.Rod.LineOnRodLength;

                if (metricValue > maxLength)
                    metricValue = maxLength;

                StateUtil.Player.RodSlot.Sim.CurrentLineLength = metricValue;
                StateUtil.Player.RodSlot.AddReelClip();
            }
        }
    }

    private bool RewindHoursMinLock;
    private bool RewindHoursMaxLock;
    private bool RewindHoursStepLock;
    private bool RewindHoursPeakLock;
    private long RewindHoursPeakCursor;
    private long RewindHoursCooldown;

    private void TimeAndWeatherActivity()
    {
        if (Settings.FishBotLock ||
            Settings.TimeSettingLock
        ) return;

        var inGameTime = TimeUtil.InGameTime;
        if (inGameTime == null) return;

        var inGameHours = inGameTime?.ToString("h tt");
        if (RewindHoursMinLock &&
            Date.Now >= RewindHoursCooldown
        ) RewindHoursMinLock = false;

        if (StateUtil.FishCage != null &&
            StateUtil.FishCage.Count > 0 &&
            StateUtil.FishCage.Weight.Value > StateUtil.FishCage.Cage.TotalWeight)
        {
            int? hours = null;
            bool isNextMorning = false;
            if (Settings.DayPeakOnly ||
                Settings.NightPeakOnly)
            {
                var finishTime = Settings.DayPeakOnly ? "5 AM" : "9 PM";
                hours = TimeUtil.FindHoursDiff(inGameHours, finishTime, true);
            }
            else if (!Settings.RewindHours)
                isNextMorning = true;

            hours ??= TimeUtil.FindHoursDiff(inGameHours, Settings.RewindHoursMinField, true);
            if (!RewindHoursMinLock &&
                TimeUtil.RewindHoursChangeTime(hours.Value, isNextMorning))
            {
                RewindHoursMinLock = true;
                RewindHoursCooldown = Date.Now + 60_000;
                Plugin.Log.LogInfo("RewindHoursMin");
            }
        }
        else if (Settings.DayPeakOnly ||
            Settings.NightPeakOnly)
        {
            new Action(() =>
            {
                if (RewindHoursPeakLock &&
                    Date.Now >= RewindHoursCooldown
                ) RewindHoursPeakLock = false;
                if (RewindHoursPeakLock) return;

                List<List<string>> timePeak = [];
                if (Settings.DayPeakOnly &&
                    Container.WeatherPeakTime.Count >= 1
                ) timePeak = Container.WeatherPeakTime[0];
                else if (Settings.NightPeakOnly &&
                    Container.WeatherPeakTime.Count == 2
                ) timePeak = Container.WeatherPeakTime[1];
                if (timePeak.Count == 0) return;

                for (var i = 0; i < 2; i++)
                    for (var j = 0; j < timePeak[i].Count; j++)
                    {
                        if (timePeak[i].Count > 1 &&
                            j < RewindHoursPeakCursor
                        ) continue;

                        var time = timePeak[i][j];
                        var timeFinal = time;
                        if (i == 1 &&
                            inGameHours[^2..] == time[^2..]
                        ) timeFinal = DateTime.Parse(time).AddDays(1).ToString();

                        var timeDiff = TimeUtil.FindHoursDiff(inGameHours, timeFinal);
                        if (i == 0)
                        {
                            if (timeDiff < 0) continue;
                            if (timeDiff == 0) return;
                        }

                        if (TimeUtil.RewindHoursChangeTime(timeDiff))
                        {
                            RewindHoursPeakCursor = i == 0 ? j : 0;
                            RewindHoursPeakLock = true;
                            RewindHoursCooldown = Date.Now + 60_000;

                            Plugin.Log.LogInfo($"RewindHoursPeak {timePeak[0].Stringify()} / {timePeak[1].Stringify()} / {timePeak[2].Stringify()}");
                            Plugin.Log.LogInfo($"RewindHoursPeak {inGameHours} to {(i == 1 ? "d2 " : null)}{time} / {timeDiff}");
                        }
                        return;
                    }
            })();
        }
        else if (Settings.RewindHours ||
            Settings.RewindHoursStep)
        {
            if (Settings.RewindHours)
            {
                if (RewindHoursMaxLock &&
                    Date.Now >= RewindHoursCooldown
                ) RewindHoursMaxLock = false;

                var rewindHoursMax = TimeUtil.FindHoursDiff(inGameHours, Settings.RewindHoursMaxField);
                if (rewindHoursMax <= 0)
                {
                    var hours = TimeUtil.FindHoursDiff(inGameHours, Settings.RewindHoursMinField, true);
                    if (!RewindHoursMaxLock &&
                        TimeUtil.RewindHoursChangeTime(hours))
                    {
                        RewindHoursMaxLock = true;
                        RewindHoursCooldown = Date.Now + 60_000;
                        Plugin.Log.LogInfo($"RewindHoursMax {inGameHours} to {Settings.RewindHoursMinField}");
                    }
                }
                else
                    RewindHoursMaxLock = false;
            }

            if (Settings.RewindHoursStep)
            {
                if (RewindHoursStepLock &&
                    Date.Now >= RewindHoursCooldown
                ) RewindHoursStepLock = false;

                var trigger = int.Parse(Settings.RewindHoursStepAField);
                if (inGameTime?.Minute >= trigger)
                {
                    var stepHours = int.Parse(Settings.RewindHoursStepBField);
                    var startTime = inGameTime?.AddHours(stepHours).ToString("h tt");
                    var rewindHoursMax = TimeUtil.FindHoursDiff(startTime, Settings.RewindHoursMaxField);
                    var hours = TimeUtil.FindHoursDiff(inGameHours, Settings.RewindHoursMinField, true);
                    if (!RewindHoursStepLock &&
                        TimeUtil.RewindHoursChangeTime(rewindHoursMax <= 0 ? hours : stepHours))
                    {
                        RewindHoursStepLock = true;
                        RewindHoursCooldown = Date.Now + 60_000;
                        Plugin.Log.LogInfo($"RewindHoursStep {inGameHours} {inGameTime?.Minute} to {startTime}");
                    }
                }
                else
                    RewindHoursStepLock = false;
            }
        }
    }
}