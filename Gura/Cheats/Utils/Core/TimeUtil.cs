using Gura.Patchs;
using System;

namespace Gura.Utils;

public static class TimeUtil
{
    public static bool RewindHoursChangeTime(int hours, bool toNextMorning = false)
    {
        if (StateUtil.Player.IsInteractionWithRodStand) return false;
        if (Settings.AntiGoldCoins &&
            StateUtil.RewindHours.CooldownPanel.activeSelf)
        {
            Container.IsDrawOut = true;
            Settings.TimeSettingLock = true;
            return false;
        }

        if (StateUtil.Fish.Count > 0)
        {
            Container.IsDrawOut = false;
            if (StateUtil.RewindHours.RewindBackgroundPanel.activeSelf)
                StateUtil.RewindHours.CancelRewind();

            return false;
        }

        if (!StateUtil.Player.IsIdle)
        {
            if (!Container.IsDrawOut)
            {
                Container.IsDrawOut = true;
                Plugin.Log.LogInfo("RewindHours PlayerDrawOut");
                PlayerIdleThrownPatch.RequestDrawOut = true;
            }

            return false;
        }

        if (!StateUtil.RewindHours.RewindBackgroundPanel.activeSelf)
        {
            if (!StateUtil.RewindHours._alreadyRequested)
            {
                StateUtil.RewindHours._alreadyRequested = true;
                Plugin.Log.LogInfo("RewindHours RequestCooldownAndShowPanel");
                StateUtil.RewindHours.RequestCooldownAndShowPanel();
            }

            return false;
        }

        Container.IsDrawOut = true;
        if (hours < 1)
            hours = 1;
        if (hours > 24)
            hours = 24;

        if (toNextMorning)
            StateUtil.RewindHours.HoursValue = -1;
        else
            StateUtil.RewindHours.HoursValue = hours;

        StateUtil.RewindHours.StartChangeTime();
        StateUtil.RewindHours.CancelRewind();
        Container.IsDrawOut = false;
        return true;
    }

    public static int FindHoursDiff(string start, string finish, bool forward = false)
    {
        var startTime = DateTime.Parse(start);
        var finishTime = DateTime.Parse(finish);

        if (startTime.Day == finishTime.Day &&
            startTime.ToString("tt") == "PM" &&
            finishTime.ToString("tt") == "AM")
            finishTime = finishTime.AddDays(1);

        var result = -(int)(startTime - finishTime).TotalHours;
        if (forward)
        {
            if (result < 0) result = 24 + result;
            result = result == 0 ? 24 : result;
        }
        return result;
    }

    public static DateTime? InGameTime
    {
        get
        {
            if (!TimeAndWeatherManager._timeInited) return null;

            var format = "MM/dd/yyyy HH:mm:ss";
            return DateTime.ParseExact(TimeAndWeatherManager.CurrentInGameTime().ToString(format), format, null);
        }
    }
}