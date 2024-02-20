using HarmonyLib;

namespace Gura.Patchs;

[HarmonyPatch(typeof(ActionDragStyleAnalyzer))]
public class ActionDragStyleAnalyzerPatch
{
    [HarmonyPatch(nameof(ActionDragStyleAnalyzer.CalculateQuality), [typeof(ActionDragStyleAnalyzer.ComplexDrag)])]
    [HarmonyPostfix]
    public static void CalculateQuality(ref float __result) =>
        __result = 1f;
}