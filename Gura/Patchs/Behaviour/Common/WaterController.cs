using HarmonyLib;
using UnityEngine;

namespace Gura.Patchs;

[HarmonyPatch]
public class WaterControllerPatch
{
    [HarmonyPatch(typeof(WaterFlowController))]
    [HarmonyPatch(nameof(WaterFlowController.Start))]
    [HarmonyPrefix]
    public static bool Start() =>
        false;

    [HarmonyPatch(typeof(WaterController))]
    [HarmonyPatch(nameof(WaterController.AddWaterDisturb), [typeof(Vector3), typeof(float), typeof(float)])]
    [HarmonyPrefix]
    public static bool AddWaterDisturb() =>
        false;

    [HarmonyPatch(typeof(WaterController))]
    [HarmonyPatch(nameof(WaterController.ApplyRandomDisturbances))]
    [HarmonyPrefix]
    public static bool ApplyRandomDisturbances() =>
        false;
}