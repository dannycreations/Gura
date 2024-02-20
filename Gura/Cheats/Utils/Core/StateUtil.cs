using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using ObjectModel;

namespace Gura.Utils;

public static class StateUtil
{
    public static PlayerController Player =>
        GameFactory.Player;

    public static List<RodPodController> RodPods =>
        Player.RodPods;

    public static GameFactory.RodSlot[] RodSlots =>
        GameFactory.RodSlots;

    public static FishSpawner FishSpawner =>
        GameFactory.FishSpawner;

    public static Dictionary<Guid, IFishController> Fish =>
        FishSpawner.Fish;

    public static IPhotonServerConnection Conn =>
        PhotonConnectionFactory.Instance;

    public static Game Game =>
        Conn.Game;

    public static GameActionAdapter GameAdapter =>
        Game.Adapter;

    public static Profile Profile =>
        Conn.Profile;

    public static FishCageContents FishCage =>
        Profile.FishCage;

    public static Inventory Inventory =>
        Profile.Inventory;

    public static TravelManager Travel =>
        PhotonConnectionFactory.Travel;

    public static RewindHoursHandler RewindHours =>
        ShowHudElements.Instance.RewindHours;
}