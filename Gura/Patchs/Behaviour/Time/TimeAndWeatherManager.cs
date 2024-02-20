using HarmonyLib;
using ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Gura.Patchs;

[HarmonyPatch(typeof(TimeAndWeatherManager))]
public class TimeAndWeatherManagerPatch
{
    [HarmonyPatch(nameof(TimeAndWeatherManager.OnGotPondWeatherForecast))]
    [HarmonyPrefix]
    public static bool OnGotPondWeatherForecast(WeatherDesc[] weather)
    {
        var midDay = weather.Where(x => x.TimeOfDay == TimeOfDay.Midday.ToString()).ToList();
        var midNight = weather.Where(x => x.TimeOfDay == TimeOfDay.MidNight.ToString()).ToList();

        var list = new List<List<List<string>>>(2) { new(), new() };
        for (var i = 0; i < midDay.Count; i++)
        {
            var dayKey = midDay[i].FishingDiagramImageId.ToString();
            var dayExist = DayPeak.TryGetValue(dayKey, out string[] outDayPeak);
            if (!dayExist) Plugin.Log.LogWarning($"Weather day {i + 1} key {dayKey} not found!");

            var nightKey = midNight[i].FishingDiagramImageId.ToString();
            var nightExist = NightPeak.TryGetValue(nightKey, out string[] outNightPeak);
            if (!nightExist) Plugin.Log.LogWarning($"Weather night {i + 1} key {nightKey} not found!");

            list[0].Add(dayExist ? [.. outDayPeak] : null);
            list[1].Add(nightExist ? [.. outNightPeak] : null);
        }

        Container.WeatherPeakTime = list;
        return true;
    }

    private static readonly Dictionary<string, string[]> DayPeak = new()
    {
        ["600"] = ["6 AM", "6 PM"],
        ["601"] = ["6 AM", "6 PM"],
        ["602"] = ["6 AM", "6 PM"],
        ["603"] = ["6 PM"],
        ["604"] = ["1 PM"],
        ["605"] = ["6 PM"],
        ["606"] = ["8 AM", "4 PM"],
        ["607"] = ["6 PM"],
        ["608"] = ["5 AM", "7 PM"],
        ["609"] = ["8 AM", "4 PM"],
        ["610"] = ["2 PM"],
        ["611"] = ["3 PM"],
        ["612"] = ["11 AM"],
        ["1399"] = ["6 AM", "6 PM"],
        ["1408"] = ["2 PM", "3 PM", "4 PM"],
        ["2475"] = ["7 AM"],
        ["2476"] = ["8 PM"],
        ["2477"] = ["8 PM"],
        ["2478"] = ["5 AM", "6 AM"],
        ["2479"] = ["5 AM"],
    };

    private static readonly Dictionary<string, string[]> NightPeak = new()
    {
        ["3718"] = ["9 PM", "3 AM"],
        ["3719"] = ["12 AM"],
        ["3720"] = ["3 AM"],
        ["3721"] = ["9 PM"],
        ["3722"] = ["9 PM"],
        ["3723"] = ["3 AM"],
        ["3724"] = ["3 AM"],
    };
}