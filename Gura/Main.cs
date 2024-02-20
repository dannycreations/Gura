using Gura.Utils;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gura;

public class Main : MonoBehaviour
{
    public void Awake()
    {
        foreach (var cheat in CheatBase.Pools.Values)
            cheat.OnAwake();
    }

    private bool IsPause;

    public void Update()
    {
        if (Keyboard.current[Plugin.MenuKeybind.Value].wasPressedThisFrame)
            Settings.MenuVisible = !Settings.MenuVisible;

        if (Keyboard.current[Key.Insert].wasPressedThisFrame)
        {
            IsPause = !IsPause;
            Plugin.Log.LogWarning($"{(IsPause ? "Paused" : "Resumed")}!");
        }
        if (IsPause) return;

        foreach (var cheat in CheatBase.Pools.Values)
        {
            if (!Safe.IsResult(() => StateUtil.Conn.IsAuthenticated) ||
                !Safe.IsResult(() => StateUtil.Conn.IsConnectedToGame) ||
                !Safe.IsResult(() => StateUtil.Travel.IsInRoom)
            ) continue;

            cheat.OnUpdate();
        }

        GameBooster();
    }

    public void OnGUI()
    {
        foreach (var cheat in CheatBase.Pools.Values)
        {
            if (cheat is not Cheats.UIManager)
            {
                if (!Safe.IsResult(() => StateUtil.Conn.IsAuthenticated) ||
                    !Safe.IsResult(() => StateUtil.Conn.IsConnectedToGame) ||
                    !Safe.IsResult(() => StateUtil.Travel.IsInRoom)
                ) continue;
            }

            cheat.OnDisplay();
        }
    }

    private bool IsFirstTime;

    private void GameBooster()
    {
        if (!Settings.GameBooster) return;
        if (!Safe.IsResult(() => StateUtil.Conn.IsAuthenticated) ||
            !Safe.IsResult(() => StateUtil.Conn.IsConnectedToGame) ||
            !Safe.IsResult(() => StateUtil.Travel.IsInRoom))
        {
            IsFirstTime = false;
            return;
        }
        if (IsFirstTime) return;

        IsFirstTime = true;
        Application.targetFrameRate = 30;
        SettingsManager.RenderQuality = RenderQualities.FastestMobile;
        QualitySettings.SetQualityLevel((int)RenderQualities.FastestMobile);

        QualitySettings.lodBias = 0f;
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;
        QualitySettings.shadowDistance = 0f;
        QualitySettings.pixelLightCount = 0;
        QualitySettings.softParticles = false;
        QualitySettings.softVegetation = false;
        QualitySettings.masterTextureLimit = 0;
        QualitySettings.particleRaycastBudget = 0;
        QualitySettings.streamingMipmapsActive = false;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.resolutionScalingFixedDPIFactor = 0f;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;

        GameFactory.Water.FishWaterBaseInstance.dynWaterQuality = FishDynWaterQuality.Low;

        foreach (var camera in Camera.allCameras)
        {
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = false;
            camera.GetComponent<SSAOPro>().enabled = false;
            camera.GetComponent<SSAOPro>().Downsampling = 3;
            camera.GetComponent<SSAOPro>().NoiseTexture = null;
            camera.GetComponent<SSAOPro>().Samples = SSAOPro.SampleCount.VeryLow;
        }

        Plugin.Log.LogMessage("Gamebooster enabled!");
    }
}