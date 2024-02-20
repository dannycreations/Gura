using System.Collections.Generic;

namespace Gura;

public static class Settings
{
    public static bool IsDebug = false;
    public static bool MenuVisible = true;

    // ================================================== //

    public static bool ESP = true;
    public static bool FreePremium = true;
    public static bool GameBooster = true;
    public static bool AntiHUD = true;
    public static bool AntiGoldCoins = true;

    // ================================================== //

    public static bool FishBot = false;
    public static bool FishBotLock = false;
    public static bool RepairAllEquipment = false;

    // ================================================== //

    public static bool FishSetting = false;
    public static bool StrongRod = false;
    public static bool InstantCatch = false;
    public static bool FishEventOnly = false;
    public static bool FishTrophyOnly = false;
    public static bool FishUniqueOnly = false;
    public static bool AutoCatch = true;

    // ================================================== //

    public static bool CastSetting = false;
    public static bool AutoCasting = true;
    public static bool AutoDragStyle = false;
    public static bool DragStyleStopNGo = true;

    public static bool DragPosSurface = false;
    public static bool DragPosMid = false;
    public static bool DragPosNearBottom = false;
    public static bool DragPosOnBottom = false;

    public static bool AutoReelClip = true;

    // ================================================== //

    public static bool TimeSetting = false;
    public static bool TimeSettingLock = false;
    public static bool DayPeakOnly = false;
    public static bool NightPeakOnly = true;
    public static bool RewindHours = false;
    public static bool RewindHoursStep = false;

    // ================================================== //

    public static string ReelClipTextField = "80 FT";
    public static string FishTimeoutTextField = "60000";

    // ================================================== //

    public static string RewindHoursMinField = "9 PM";
    public static string RewindHoursMaxField = "10 PM";
    public static string RewindHoursStepAField = "15"; // Triggger minutes
    public static string RewindHoursStepBField = "2"; // Total forward hours

    // ================================================== //

    public static List<int> BrokenRodSlotId = [];
    public static List<int> BlacklistRodSlotId = [6, 7];
}