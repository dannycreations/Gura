using System;

namespace Gura.Utils;

public static class TackleUtil
{
    public static bool IsFeeder =>
        Safe.IsResult(() => Container.TackleState.Find("feeder"));

    public static bool IsFloat =>
        Safe.IsResult(() => Container.TackleState.Find("float"));

    public static bool IsLure =>
        Safe.IsResult(() => Container.TackleState.Find("lure"));

    public static bool IsFloating =>
        Safe.IsResult(() => Container.TackleState.Find("floating"));
}