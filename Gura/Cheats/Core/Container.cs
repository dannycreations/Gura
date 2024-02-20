using ObjectModel;
using Photon.Interfaces;
using Photon.Interfaces.Game;
using System.Collections.Generic;

namespace Gura;

public static class Container
{
    public static bool IsDrawOut;

    public static string FishState;
    public static string PlayerState;
    public static string TackleState;
    public static TackleStatus? TackleStatus;
    public static IFishController FishInHand;
    public static FishCageContents FishInCage = new();

    public static EventCode GameEventCode;
    public static GameActionCode GameActionCode;

    public static InfoServerMessagesHandler HUDInstance;

    public static List<Il2CppSystem.Guid> IgnoredFish = [];
    public static List<List<List<string>>> WeatherPeakTime = [];
}