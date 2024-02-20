using ObjectModel;
using SharpHook;
using SharpHook.Native;
using System;

namespace Gura.Utils;

public static class MouseSimulator
{
    private static bool IsStrike;
    private static long StrikePressNext;

    public static void TryStrike(bool isPress)
    {
        if (isPress && (Date.Now >= StrikePressNext || !IsStrike))
        {
            IsStrike = true;
            StrikePressNext = Date.Now + 2_500;
            TryPress(MouseButton.Button2, true);
            TryPress(MouseButton.Button1, true);
        }
        else if (!isPress && IsStrike)
        {
            TryPress(MouseButton.Button1, false);
            TryPress(MouseButton.Button2, false);
            IsStrike = false;
        }
    }

    private static bool IsDrag;
    private static long DragPressNext;
    private static long DragReleaseNext;

    public static void TryDrag(DragStyle? style, bool isPress, bool isForce)
    {
        var isMatch = false;
        if (!isForce)
        {
            if (Container.TackleStatus == TackleStatus.OnSurface && (
                Settings.DragPosSurface ||
                Settings.DragPosMid ||
                Settings.DragPosNearBottom ||
                Settings.DragPosOnBottom
            )) isPress = false;
            else if (Container.TackleStatus == TackleStatus.NearSurface && (
                Settings.DragPosMid ||
                Settings.DragPosNearBottom ||
                Settings.DragPosOnBottom
            )) isPress = false;
            else if (Container.TackleStatus == TackleStatus.MidWater && (
                Settings.DragPosNearBottom ||
                Settings.DragPosOnBottom
            )) isPress = false;
            else if (Container.TackleStatus == TackleStatus.NearBottom &&
                Settings.DragPosOnBottom
            ) isPress = false;

            if (Settings.DragPosSurface &&
                Container.TackleStatus <= TackleStatus.NearSurface
            ) isMatch = true;
            else if (Settings.DragPosMid &&
                Container.TackleStatus <= TackleStatus.MidWater
            ) isMatch = true;
            else if (Settings.DragPosNearBottom &&
                Container.TackleStatus <= TackleStatus.NearBottom
            ) isMatch = true;
            else if (Settings.DragPosOnBottom &&
                Container.TackleStatus == TackleStatus.OnBottom
            ) isMatch = true;
        }

        var pressDelay = 0;
        var releaseDelay = 0;
        var buttonInput = MouseButton.Button1;
        if (style == DragStyle.StopNGo)
        {
            isMatch = false;
            pressDelay = 600;
            releaseDelay = 500;
        }

        if (!IsDrag && (
            (isForce && isPress) ||
            (Date.Now >= DragPressNext && isPress) ||
            isMatch
        ))
        {
            IsDrag = true;
            DragReleaseNext = Date.Now + releaseDelay;
            TryPress(buttonInput, true);
        }
        else if (IsDrag && (
            (isForce && !isPress) ||
            Date.Now >= DragReleaseNext || !isPress)
        )
        {
            IsDrag = false;
            DragPressNext = Date.Now + pressDelay;
            TryPress(buttonInput, false);
        }
    }

    private static UioHookResult TryPress(MouseButton button, bool isPress) =>
        isPress ? Event.SimulateMousePress(button) : Event.SimulateMouseRelease(button);

    private static readonly EventSimulator Event = new();
}