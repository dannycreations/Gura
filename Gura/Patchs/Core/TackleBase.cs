using Gura.Utils;
using Il2CppInterop.Runtime;
using ObjectModel;
using UnityEngine.InputSystem;

namespace Gura;

public class TackleBase
{
    private static readonly Key InstantCatchKeybind = Key.Numpad4;

    protected static bool FeederTackle(FeederStateBase __instance, ref Il2CppSystem.Type __result)
    {
        var rod = __instance.RodSlot;
        if (!rod.Tackle.IsFinishAttackRequested)
            if (rod.Tackle.IsBeingPulled)
                StateUtil.GameAdapter.FinishAttack(false, false, true, false, 0f);
            else
                StateUtil.GameAdapter.FinishAttack(true, false, false, true, 0f);

        var isInRangeCatch = rod.Line.FullLineLength <= rod.Line.MinLineLengthWithFish;
        if (rod.IsInHands &&
            rod.Tackle.Fish != null)
        {
            if (isInRangeCatch &&
                rod.Tackle.Fish.Behavior == FishBehavior.Undefind)
                rod.Tackle.Fish.Behavior = FishBehavior.Go;
            else if (!rod.Tackle.IsShowing &&
                rod.Tackle.Fish.Behavior == FishBehavior.Hook && (
                Keyboard.current[InstantCatchKeybind].wasPressedThisFrame ||
                Settings.InstantCatch ||
                isInRangeCatch
            ))
            {
                rod.Tackle.IsShowing = true;
                if (rod.Tackle.Fish.IsBig)
                    __result = Il2CppType.Of<FeederShowBigFish>();
                else
                    __result = Il2CppType.Of<FeederShowSmallFish>();
            }
        }

        if (!rod.Tackle.FishIsLoading && (
            rod.Tackle.Fish == null ||
            rod.Tackle.Fish.Behavior == FishBehavior.Go
        ))
        {
            if (isInRangeCatch ||
                Container.TackleState == nameof(FeederHanging))
            {
                rod.Tackle.EscapeFish();
                StateUtil.GameAdapter.FinishGameAction();
                __result = Il2CppType.Of<FeederOnTip>();
            }
            else
            {
                rod.Tackle.TackleIn(1f);
                __result = Il2CppType.Of<FeederFloating>();
            }
        }

        return false;
    }

    protected static bool FloatTackle(FloatStateBase __instance, ref Il2CppSystem.Type __result)
    {
        var rod = __instance.RodSlot;
        if (!rod.Tackle.IsFinishAttackRequested)
            if (rod.Tackle.IsBeingPulled)
                StateUtil.GameAdapter.FinishAttack(false, false, true, false, 0f);
            else
                StateUtil.GameAdapter.FinishAttack(true, false, false, true, 0f);

        var isInRangeCatch = rod.Line.FullLineLength <= rod.Line.MinLineLengthWithFish;
        if (rod.IsInHands &&
            rod.Tackle.Fish != null)
        {
            if (isInRangeCatch &&
                rod.Tackle.Fish.Behavior == FishBehavior.Undefind)
                rod.Tackle.Fish.Behavior = FishBehavior.Go;
            else if (!rod.Tackle.IsShowing &&
                rod.Tackle.Fish.Behavior == FishBehavior.Hook && (
                Keyboard.current[InstantCatchKeybind].wasPressedThisFrame ||
                Settings.InstantCatch ||
                isInRangeCatch
            ))
            {
                rod.Tackle.IsShowing = true;
                if (rod.Tackle.Fish.IsBig)
                    __result = Il2CppType.Of<FloatShowBigFish>();
                else
                    __result = Il2CppType.Of<FloatShowSmallFish>();
            }
        }

        if (!rod.Tackle.FishIsLoading && (
            rod.Tackle.Fish == null ||
            rod.Tackle.Fish.Behavior == FishBehavior.Go
        ))
        {
            if (isInRangeCatch ||
                Container.TackleState == nameof(FloatHanging))
            {
                rod.Tackle.EscapeFish();
                StateUtil.GameAdapter.FinishGameAction();
                __result = Il2CppType.Of<FloatOnTip>();
            }
            else
            {
                rod.Tackle.TackleIn(1f);
                __result = Il2CppType.Of<FloatFloating>();
            }
        }

        return false;
    }

    protected static bool LureTackle(LureStateBase __instance, ref Il2CppSystem.Type __result)
    {
        var rod = __instance.RodSlot;
        if (!rod.Tackle.IsFinishAttackRequested)
            if (rod.Tackle.IsBeingPulled)
                StateUtil.GameAdapter.FinishAttack(false, false, true, false, 0f);
            else
                StateUtil.GameAdapter.FinishAttack(true, false, false, true, 0f);

        var isInRangeCatch = rod.Line.FullLineLength <= rod.Line.MinLineLengthWithFish;
        if (rod.IsInHands &&
            rod.Tackle.Fish != null)
        {
            if (isInRangeCatch &&
                rod.Tackle.Fish.Behavior == FishBehavior.Undefind)
                rod.Tackle.Fish.Behavior = FishBehavior.Go;
            else if (!rod.Tackle.IsShowing &&
                rod.Tackle.Fish.Behavior == FishBehavior.Hook && (
                Keyboard.current[InstantCatchKeybind].wasPressedThisFrame ||
                Settings.InstantCatch ||
                isInRangeCatch
            ))
            {
                rod.Tackle.IsShowing = true;
                if (rod.Tackle.Fish.IsBig)
                    __result = Il2CppType.Of<LureShowBigFish>();
                else
                    __result = Il2CppType.Of<LureShowSmallFish>();
            }
        }

        if (!rod.Tackle.FishIsLoading && (
            rod.Tackle.Fish == null ||
            rod.Tackle.Fish.Behavior == FishBehavior.Go
        ))
        {
            if (isInRangeCatch ||
                Container.TackleState == nameof(LureHanging))
            {
                rod.Tackle.EscapeFish();
                StateUtil.GameAdapter.FinishGameAction();
                __result = Il2CppType.Of<LureOnTip>();
            }
            else
            {
                rod.Tackle.TackleIn(1f);
                __result = Il2CppType.Of<LureFloating>();
            }
        }

        return false;
    }
}