using Assets.Scripts.Common.Managers;
using Gura.Patchs;
using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;

namespace Gura.Utils;

public static class PlayerUtil
{
    private static long TakeRodNext;

    public static void TryToTakeRodFromSlot(int slot, TakeRodState state = TakeRodState.None)
    {
        if (Date.Now < TakeRodNext ||
            StateUtil.Player.IsPulling ||
            StateUtil.Player.IsReeling ||
            StateUtil.Player.IsThrowing ||
            StateUtil.Player.IsStriking ||
            StateUtil.Player.IsCatchedSomething ||
            StateUtil.Player.IsInteractionWithRodStand
        ) return;

        TakeRodNext = Date.Now + 2_500;
        if (StaticUserData.RodInHand.IsRodDisassembled)
        {
            if (Settings.BrokenRodSlotId.Contains(slot))
            {
                Settings.FishBot = false;
                KeysHandlerAction.InventoryHandler();
                PlayerStateBase._lastPage = MainMenuPage.Inventory;
                Plugin.Log.LogWarning("All rod maybe broken!");
                return;
            }

            Settings.BrokenRodSlotId.Add(slot);
            Plugin.Log.LogInfo($"TryToTakeRodFromSlot PlayerDrawOut {slot}");
            PlayerIdleThrownPatch.RequestDrawOut = true;
        }
        else if (state == TakeRodState.Change)
        {
            Plugin.Log.LogInfo($"TryToTakeRodFromSlot TryToTakeRodFromSlot {slot}");
            StateUtil.Player.TryToTakeRodFromSlot(slot, false);
        }
        else if (PodUtil.HasRodPods)
        {
            if (Container.PlayerState == nameof(PlayerIdleThrown))
            {
                if (state == TakeRodState.Put &&
                    StateUtil.Player.TryToPutRodOnPod(slot))
                {
                    Plugin.Log.LogInfo($"TryToTakeRodFromSlot PutRodOnPodIn {slot}");
                    StateUtil.Player.fsm.EnterState(Il2CppType.Of<PutRodOnPodIn>());
                }
                else if (state == TakeRodState.Replace &&
                    StateUtil.Player.TryToReplaceRod(slot))
                {
                    Plugin.Log.LogInfo($"TryToTakeRodFromSlot ReplaceRodOnPodLean {slot}");
                    StateUtil.Player.fsm.EnterState(Il2CppType.Of<ReplaceRodOnPodLean>());
                }
            }
            else if (Container.PlayerState == nameof(PlayerEmpty))
            {
                if (state == TakeRodState.Take &&
                    StateUtil.Player.CanTakeOrReplaceRodOnStand(true))
                {
                    Plugin.Log.LogInfo($"TryToTakeRodFromSlot TakeRodFromPodIn {slot}");
                    StateUtil.Player.fsm.EnterState(Il2CppType.Of<TakeRodFromPodIn>());
                }
            }
        }
    }

    public static void TryCasting()
    {
        if (!RodUtil.GetFreeRodSlot(out List<int> slots)) return;

        foreach (var slot in slots)
        {
            if (Settings.BrokenRodSlotId.Contains(slot)) continue;
            if (Settings.BlacklistRodSlotId.Contains(slot)) continue;

            switch (Container.PlayerState)
            {
                case nameof(PlayerIdle):
                    MouseSimulator.TryStrike(false);
                    StateUtil.Player.CastType = CastTypes.Simple;
                    StateUtil.Player.fsm.EnterState(Il2CppType.Of<PlayerThrowInTwoHands>());
                    return;

                case nameof(PlayerEmpty):
                    if (PodUtil.HasRodPods)
                        TryToTakeRodFromSlot(slot, TakeRodState.Change);
                    return;

                case nameof(PlayerIdlePitch):
                    if (!StateUtil.Player.IsPitching) return;

                    StateUtil.Player.IsPitching = false;
                    StateUtil.Player.fsm.EnterState(Il2CppType.Of<PlayerIdlePitchToIdle>());
                    return;

                case nameof(PlayerIdleThrown):
                    if (PodUtil.GetFreeRodSlot(out int podSlot))
                        TryToTakeRodFromSlot(podSlot, TakeRodState.Put);
                    return;
            }
        }
    }
}

public enum TakeRodState
{
    None,
    Change,
    Put,
    Replace,
    Take
}