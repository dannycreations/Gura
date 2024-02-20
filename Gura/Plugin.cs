using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gura;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        Log ??= base.Log;
        GameObject ??= new GameObject(GUID) { hideFlags = HideFlags.HideAndDontSave };

        if (!ClassInjector.IsTypeRegisteredInIl2Cpp<Main>())
            ClassInjector.RegisterTypeInIl2Cpp<Main>();

        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var key = type.FullName;
            if (CheatBase.Pools.ContainsKey(key)) continue;
            if (!typeof(CheatBase).IsAssignableFrom(type)) continue;

            CheatBase.Pools.Add(key, (CheatBase)AccessTools.CreateInstance(type));
        }

        GameObject.AddComponent<Main>();
        Object.DontDestroyOnLoad(GameObject);

        var harmony = new Harmony(GUID);
        harmony.PatchAll();

        Log.LogInfo($"[#] Menu toggle keybind set to: {MenuKeybind.Value}");
    }

    public override bool Unload()
    {
        Harmony.UnpatchAll();
        Object.Destroy(GameObject);
        return base.Unload();
    }

    private const string NAME = "Gura";
    private const string VERSION = "1.0.0";
    private const string GUID = "dnycts.Gura";

    internal static readonly ConfigFile ConfigFile = new(Path.Combine(Paths.ConfigPath, $"{GUID}.cfg"), true);
    internal static readonly ConfigEntry<Key> MenuKeybind = ConfigFile.Bind("Hotkeys", "Toggle", Key.F10, "Enables or disables the Menu");

    internal static GameObject GameObject;
    internal new static ManualLogSource Log;
}