using System;
using System.Linq;
using System.Text.RegularExpressions;
using InControl;
using UnityEngine;

public static class SettingsManager
{
	static SettingsManager()
	{
		SettingsManager.Initialize();
	}

	public static bool IsLowLevelSystem(string gDeviceName)
	{
		bool flag = Application.platform == 2 && IntPtr.Size == 4;
		bool flag2 = SystemInfo.systemMemorySize != 0 && SystemInfo.systemMemorySize <= 4100;
		if (flag || flag2)
		{
			LogHelper.Log("Your system is weak by memory parameter. isWin32 == {0}, isLessThen4GbMemory == {1}", new object[] { flag, flag2 });
			return true;
		}
		string text = gDeviceName.ToLower();
		if (text.Contains("intel"))
		{
			LogHelper.Log("Your system is weak - intel video card");
			return true;
		}
		if (text.Contains("nvidia"))
		{
			if (text.Contains("rtx"))
			{
				return false;
			}
			MatchCollection matchCollection = Regex.Matches(text, "nvidia.*\\b(\\d{3,})");
			if (matchCollection.Count <= 0)
			{
				return true;
			}
			string value = matchCollection[0].Groups[1].Value;
			if (value.Length == 4)
			{
				bool flag3 = value[2] - '0' <= '\u0004';
				if (flag3)
				{
					LogHelper.Log("Your system is possibly weak - low-end xxxx nvidia video card");
				}
				return flag3;
			}
			if (value.Length == 3)
			{
				bool flag4 = value[1] - '0' <= '\u0004';
				if (flag4)
				{
					LogHelper.Log("Your system is weak - low-end nvidia card X_X where _ < 5");
				}
				return flag4;
			}
			LogHelper.Log("Your system is possibly weak - unknown nvidia card");
			return true;
		}
		else
		{
			if (!text.Contains("amd"))
			{
				LogHelper.Log("Your system is possibly weak - unknown brand video card");
				return true;
			}
			if (text.Contains("vega") || text.Contains("rx"))
			{
				return false;
			}
			MatchCollection matchCollection2 = Regex.Matches(text, "amd radeon r(\\d)\\s");
			if (matchCollection2.Count > 0)
			{
				string value2 = matchCollection2[0].Groups[1].Value;
				bool flag5 = value2[0] - '0' < '\a';
				if (flag5)
				{
					LogHelper.Log("Your system is weak - non R7+ ATI card");
				}
				return flag5;
			}
			LogHelper.Log("Your system is possibly weak - non R ATI card");
			return true;
		}
	}

	private static void TestCaseIsLowLevelSystem(string gDeviceName, bool isExpectedLowResult)
	{
		bool flag = SettingsManager.IsLowLevelSystem(gDeviceName);
		if (flag != isExpectedLowResult)
		{
			LogHelper.Error("{0} - FAILED. Expected result {1}, got {2}", new object[] { gDeviceName, isExpectedLowResult, flag });
		}
	}

	public static void Initialize()
	{
		string text = SystemInfo.graphicsDeviceName.ToLower();
		if (SettingsManager.IsLowLevelSystem(SystemInfo.graphicsDeviceName))
		{
			SettingsManager._defaultDynWater = (SettingsManager._dynWater = ((!text.Contains("amd")) ? DynWaterValue.Low : DynWaterValue.Off));
			SettingsManager._renderQuality = RenderQualities.Fast;
			SettingsManager._ssao = false;
		}
		Resolution resolution;
		if (!text.Contains("intel"))
		{
			resolution = Screen.resolutions.OrderByDescending((Resolution x) => x.width).ThenByDescending((Resolution y) => y.height).First<Resolution>();
		}
		else
		{
			resolution = (from x in Screen.resolutions
				where x.width < 2000
				orderby x.width descending
				select x).ThenByDescending((Resolution y) => y.height).First<Resolution>();
		}
		Resolution resolution2 = resolution;
		SettingsManager._defaultResolution.height = resolution2.height;
		SettingsManager._defaultResolution.width = resolution2.width;
		if (CustomPlayerPrefs.HasKey("ScreenResolutionHeigth") && CustomPlayerPrefs.GetInt("ScreenResolutionHeigth") >= 600)
		{
			SettingsManager._resolution.height = CustomPlayerPrefs.GetInt("ScreenResolutionHeigth");
		}
		else
		{
			SettingsManager._resolution.height = resolution2.height;
			CustomPlayerPrefs.SetInt("ScreenResolutionHeigth", SettingsManager._resolution.height);
		}
		if (CustomPlayerPrefs.HasKey("ScreenResolutionWidth") && CustomPlayerPrefs.GetInt("ScreenResolutionWidth") >= 800)
		{
			SettingsManager._resolution.width = CustomPlayerPrefs.GetInt("ScreenResolutionWidth");
		}
		else
		{
			SettingsManager._resolution.width = resolution2.width;
			CustomPlayerPrefs.SetInt("ScreenResolutionWidth", SettingsManager._resolution.width);
		}
		if (CustomPlayerPrefs.HasKey("ScreenResolutionRefreshRate"))
		{
			SettingsManager._resolution.refreshRate = CustomPlayerPrefs.GetInt("ScreenResolutionRefreshRate");
		}
		else
		{
			SettingsManager._resolution.refreshRate = Screen.currentResolution.refreshRate;
			CustomPlayerPrefs.SetInt("ScreenResolutionRefreshRate", SettingsManager._resolution.refreshRate);
		}
		if (CustomPlayerPrefs.HasKey("IsFullScreen"))
		{
			SettingsManager._isFullScreen = CustomPlayerPrefs.GetInt("IsFullScreen") == 1;
		}
		else
		{
			SettingsManager._isFullScreen = true;
			SettingsManager.IsFullScreen = SettingsManager._isFullScreen;
		}
		if (CustomPlayerPrefs.HasKey("ShowTips"))
		{
			SettingsManager._showTips = CustomPlayerPrefs.GetInt("ShowTips") == 1;
		}
		else
		{
			SettingsManager.ShowTips = SettingsManager._defaultShowTips;
		}
		if (CustomPlayerPrefs.HasKey("ShowSlides"))
		{
			SettingsManager._showSlides = CustomPlayerPrefs.GetInt("ShowSlides") == 1;
		}
		else
		{
			SettingsManager.ShowSlides = SettingsManager._defaultShowSlides;
		}
		if (CustomPlayerPrefs.HasKey("ShowCharacters"))
		{
			SettingsManager._showCharacters = CustomPlayerPrefs.GetInt("ShowCharacters") == 1;
		}
		else
		{
			SettingsManager.ShowCharacters = SettingsManager._defaultShowCharacters;
		}
		if (CustomPlayerPrefs.HasKey("ShowCharactersBubble"))
		{
			SettingsManager._showCharactersBubble = CustomPlayerPrefs.GetInt("ShowCharactersBubble") == 1;
		}
		else
		{
			SettingsManager.ShowCharacterBubble = SettingsManager._defaultShowCharactersBubble;
		}
		if (CustomPlayerPrefs.HasKey("UseOnlyXboxHelps"))
		{
			SettingsManager._useOnlyXboxHelps = CustomPlayerPrefs.GetInt("UseOnlyXboxHelps") == 1;
		}
		else
		{
			SettingsManager.UseOnlyXboxHelps = SettingsManager._defaultUseOnlyXboxHelps;
		}
		if (CustomPlayerPrefs.HasKey("UseOnlyPs4Helps"))
		{
			SettingsManager._useOnlyPs4Helps = CustomPlayerPrefs.GetInt("UseOnlyPs4Helps") == 1;
		}
		else
		{
			SettingsManager.UseOnlyPs4Helps = SettingsManager._defaultUseOnlyPs4Helps;
		}
		if (CustomPlayerPrefs.HasKey("ReverseBoatBackwardsMoving"))
		{
			SettingsManager._reverseBoatBackwardsMoving = CustomPlayerPrefs.GetInt("ReverseBoatBackwardsMoving") == 1;
		}
		else
		{
			SettingsManager.ReverseBoatBackwardsMoving = SettingsManager._defaultReverseBoatBackwardsMoving;
		}
		if (CustomPlayerPrefs.HasKey("SnagsWarnings"))
		{
			SettingsManager._snagsWarnings = CustomPlayerPrefs.GetInt("SnagsWarnings") == 1;
		}
		else
		{
			SettingsManager.SnagsWarnings = SettingsManager._defaultSnagsWarnings;
		}
		if (CustomPlayerPrefs.HasKey("FishingIndicator"))
		{
			SettingsManager._fishingIndicator = CustomPlayerPrefs.GetInt("FishingIndicator") == 1;
		}
		else
		{
			SettingsManager.FishingIndicator = SettingsManager._defaultFishingIndicator;
		}
		if (CustomPlayerPrefs.HasKey("BobberBiteSound"))
		{
			SettingsManager._bobberBiteSound = CustomPlayerPrefs.GetInt("BobberBiteSound") == 1;
		}
		else
		{
			SettingsManager.BobberBiteSound = SettingsManager._defaultBobberBiteSound;
		}
		if (CustomPlayerPrefs.HasKey("RightHandedLayout"))
		{
			SettingsManager._rightHandedLayout = CustomPlayerPrefs.GetInt("RightHandedLayout") == 1;
		}
		else
		{
			SettingsManager.RightHandedLayout = SettingsManager._defaultRightHandedLayout;
		}
		SettingsManager.CurrentResolution = SettingsManager._resolution;
		if (CustomPlayerPrefs.HasKey("VoiceChatMuted"))
		{
			SettingsManager._voiceChatMuted = CustomPlayerPrefs.GetInt("VoiceChatMuted") == 1;
		}
		else
		{
			SettingsManager.VoiceChatMuted = SettingsManager._defaultVoiceChatMuted;
		}
		if (CustomPlayerPrefs.HasKey("SoundVolume"))
		{
			SettingsManager.SoundVolume = CustomPlayerPrefs.GetFloat("SoundVolume");
		}
		else
		{
			SettingsManager.SoundVolume = SettingsManager._soundVolume;
		}
		if (CustomPlayerPrefs.HasKey("InterfaceVolume"))
		{
			SettingsManager.InterfaceVolume = CustomPlayerPrefs.GetFloat("InterfaceVolume");
		}
		else
		{
			SettingsManager.InterfaceVolume = SettingsManager._interfaceVolume;
		}
		if (CustomPlayerPrefs.HasKey("VoiceChatVolume"))
		{
			SettingsManager.VoiceChatVolume = CustomPlayerPrefs.GetFloat("VoiceChatVolume");
		}
		else
		{
			SettingsManager.VoiceChatVolume = SettingsManager._voiceChatVolume;
		}
		if (CustomPlayerPrefs.HasKey("SoundMusicVolume"))
		{
			SettingsManager._soundMusicVolume = CustomPlayerPrefs.GetFloat("SoundMusicVolume");
		}
		else
		{
			CustomPlayerPrefs.SetFloat("SoundMusicVolume", SettingsManager._soundMusicVolume);
		}
		if (CustomPlayerPrefs.HasKey("EnvironmentVolume"))
		{
			SettingsManager.EnvironmentVolume = CustomPlayerPrefs.GetFloat("EnvironmentVolume");
		}
		else
		{
			SettingsManager.EnvironmentVolume = SettingsManager._soundEnvironmentVolume;
		}
		if (CustomPlayerPrefs.HasKey("MouseSensitivity"))
		{
			SettingsManager._mouseSensitivity = CustomPlayerPrefs.GetFloat("MouseSensitivity");
		}
		else
		{
			CustomPlayerPrefs.SetFloat("MouseSensitivity", SettingsManager._mouseSensitivity);
		}
		if (CustomPlayerPrefs.HasKey("ControllerSensitivity"))
		{
			SettingsManager._controllerSensitivity = CustomPlayerPrefs.GetFloat("ControllerSensitivity");
		}
		else
		{
			CustomPlayerPrefs.SetFloat("ControllerSensitivity", SettingsManager._controllerSensitivity);
		}
		if (CustomPlayerPrefs.HasKey("AntialiasingValue"))
		{
			SettingsManager.Antialiasing = (AntialiasingValue)CustomPlayerPrefs.GetInt("AntialiasingValue");
			DebugUtility.Settings.Trace("Get antialiasing: {0}", new object[] { CustomPlayerPrefs.GetInt("AntialiasingValue") });
		}
		else
		{
			SettingsManager.Antialiasing = AntialiasingValue.Off;
		}
		if (CustomPlayerPrefs.HasKey("RenderQuality"))
		{
			SettingsManager.RenderQuality = (RenderQualities)CustomPlayerPrefs.GetInt("RenderQuality");
		}
		else
		{
			SettingsManager.RenderQuality = SettingsManager._renderQuality;
			CustomPlayerPrefs.SetInt("RenderQuality", (int)SettingsManager._renderQuality);
		}
		if (CustomPlayerPrefs.HasKey("SpeakerMode"))
		{
			SettingsManager.SpeakerMode = SettingsManager._defaultSpeakerMode;
		}
		else
		{
			SettingsManager.SpeakerMode = SettingsManager._speakerMode;
			CustomPlayerPrefs.SetInt("SpeakerMode", SettingsManager._speakerMode);
		}
		if (CustomPlayerPrefs.HasKey("DynWaterValue"))
		{
			SettingsManager.DynWater = (DynWaterValue)CustomPlayerPrefs.GetInt("DynWaterValue");
		}
		else
		{
			SettingsManager.DynWater = SettingsManager._dynWater;
		}
		if (CustomPlayerPrefs.HasKey("SSAO"))
		{
			SettingsManager.SSAO = CustomPlayerPrefs.GetInt("SSAO") > 0;
		}
		else
		{
			switch (SettingsManager.RenderQuality)
			{
			case RenderQualities.Fastest:
			case RenderQualities.Fast:
			case RenderQualities.Simple:
			case RenderQualities.Good:
				CustomPlayerPrefs.SetFloat("SSAO", 0f);
				SettingsManager.SSAO = false;
				break;
			case RenderQualities.Beautiful:
			case RenderQualities.Fantastic:
			case RenderQualities.Ultra:
				CustomPlayerPrefs.SetFloat("SSAO", 1f);
				SettingsManager.SSAO = true;
				break;
			}
		}
		if (CustomPlayerPrefs.HasKey("VSync"))
		{
			SettingsManager.VSync = CustomPlayerPrefs.GetInt("VSync") > 0;
		}
		else
		{
			switch (SettingsManager.RenderQuality)
			{
			case RenderQualities.Fastest:
			case RenderQualities.Fast:
			case RenderQualities.Simple:
			case RenderQualities.Good:
				CustomPlayerPrefs.SetFloat("VSync", 0f);
				SettingsManager.VSync = false;
				break;
			case RenderQualities.Beautiful:
			case RenderQualities.Fantastic:
			case RenderQualities.Ultra:
				CustomPlayerPrefs.SetFloat("VSync", 1f);
				SettingsManager.VSync = true;
				break;
			}
		}
		if (CustomPlayerPrefs.HasKey("InvertMouse"))
		{
			SettingsManager.InvertMouse = CustomPlayerPrefs.GetInt("InvertMouse") == 1;
		}
		else
		{
			SettingsManager.InvertMouse = false;
		}
		if (CustomPlayerPrefs.HasKey("InvertController"))
		{
			SettingsManager.InvertController = CustomPlayerPrefs.GetInt("InvertController") == 1;
		}
		else
		{
			SettingsManager.InvertController = false;
		}
		if (CustomPlayerPrefs.HasKey("Vibrate"))
		{
			SettingsManager.Vibrate = CustomPlayerPrefs.GetInt("Vibrate") == 1;
		}
		else
		{
			SettingsManager.Vibrate = false;
		}
		if (CustomPlayerPrefs.HasKey("VibrateIfNotTensioned"))
		{
			SettingsManager.VibrateIfNotTensioned = CustomPlayerPrefs.GetInt("VibrateIfNotTensioned") == 1;
		}
		else
		{
			SettingsManager.VibrateIfNotTensioned = false;
		}
		if (CustomPlayerPrefs.HasKey("HideWhatsNew"))
		{
			SettingsManager.HideWhatsNew = CustomPlayerPrefs.GetInt("HideWhatsNew") == 1;
		}
		else
		{
			SettingsManager.HideWhatsNew = false;
		}
		if (CustomPlayerPrefs.HasKey("FightIndicator"))
		{
			SettingsManager.FightIndicator = (FightIndicator)CustomPlayerPrefs.GetInt("FightIndicator");
		}
		else
		{
			SettingsManager.FightIndicator = SettingsManager._fightIndicator;
		}
		if (CustomPlayerPrefs.HasKey("MouseWheelValue"))
		{
			SettingsManager.MouseWheel = (MouseWheelValue)CustomPlayerPrefs.GetInt("MouseWheelValue");
		}
		else
		{
			SettingsManager.MouseWheel = SettingsManager._mouseWheel;
		}
		if (CustomPlayerPrefs.HasKey("InputType"))
		{
			SettingsManager.InputType = (InputModuleManager.InputType)CustomPlayerPrefs.GetInt("InputType");
		}
		else
		{
			SettingsManager.InputType = SettingsManager._inputType;
		}
		SettingsManager.BobberScale = ((!CustomPlayerPrefs.HasKey("BobberScale")) ? SettingsManager._bobberScale : CustomPlayerPrefs.GetFloat("BobberScale"));
	}

	public static float SoundVolume
	{
		get
		{
			return SettingsManager._soundVolume;
		}
		set
		{
			SettingsManager._soundVolume = value;
			AudioListener.volume = SettingsManager._soundVolume;
			CustomPlayerPrefs.SetFloat("SoundVolume", SettingsManager._soundVolume);
		}
	}

	public static float InterfaceVolume
	{
		get
		{
			return SettingsManager._interfaceVolume;
		}
		set
		{
			SettingsManager._interfaceVolume = value;
			CustomPlayerPrefs.SetFloat("InterfaceVolume", SettingsManager._interfaceVolume);
		}
	}

	public static float VoiceChatVolume
	{
		get
		{
			return SettingsManager._voiceChatVolume;
		}
		set
		{
			SettingsManager._voiceChatVolume = value;
			CustomPlayerPrefs.SetFloat("VoiceChatVolume", SettingsManager._voiceChatVolume);
		}
	}

	public static Resolution CurrentResolution
	{
		get
		{
			return SettingsManager._resolution;
		}
		set
		{
			SettingsManager._resolution = value;
			Screen.SetResolution(SettingsManager._resolution.width, SettingsManager._resolution.height, SettingsManager._isFullScreen, SettingsManager._resolution.refreshRate);
			CustomPlayerPrefs.SetInt("ScreenResolutionHeigth", SettingsManager._resolution.height);
			CustomPlayerPrefs.SetInt("ScreenResolutionWidth", SettingsManager._resolution.width);
			CustomPlayerPrefs.SetInt("ScreenResolutionRefreshRate", SettingsManager._resolution.refreshRate);
		}
	}

	public static RenderQualities RenderQuality
	{
		get
		{
			return SettingsManager._renderQuality;
		}
		set
		{
			SettingsManager._renderQuality = value;
			QualitySettings.SetQualityLevel((int)SettingsManager._renderQuality);
			SettingsManager.Antialiasing = SettingsManager._antialiasing;
			CustomPlayerPrefs.SetInt("RenderQuality", (int)SettingsManager._renderQuality);
		}
	}

	public static AudioSpeakerMode SpeakerMode
	{
		get
		{
			return SettingsManager._speakerMode;
		}
		set
		{
			if (value != SettingsManager._speakerMode)
			{
				SettingsManager._speakerMode = value;
				AudioConfiguration configuration = AudioSettings.GetConfiguration();
				configuration.speakerMode = SettingsManager._defaultSpeakerMode;
				AudioSettings.Reset(configuration);
			}
		}
	}

	public static AntialiasingValue Antialiasing
	{
		get
		{
			return SettingsManager._antialiasing;
		}
		set
		{
			SettingsManager._antialiasing = value;
			QualitySettings.antiAliasing = (int)SettingsManager._antialiasing;
			CustomPlayerPrefs.SetInt("AntialiasingValue", (int)SettingsManager._antialiasing);
		}
	}

	public static DynWaterValue DynWater
	{
		get
		{
			return SettingsManager._dynWater;
		}
		set
		{
			SettingsManager._dynWater = value;
			CustomPlayerPrefs.SetInt("DynWaterValue", (int)SettingsManager._dynWater);
		}
	}

	public static MouseWheelValue MouseWheel
	{
		get
		{
			return SettingsManager._mouseWheel;
		}
		set
		{
			SettingsManager._mouseWheel = value;
			CustomPlayerPrefs.SetInt("MouseWheelValue", (int)SettingsManager._mouseWheel);
		}
	}

	public static InputModuleManager.InputType InputTypeSaved
	{
		get
		{
			return SettingsManager._inputType;
		}
	}

	public static InputModuleManager.InputType InputType
	{
		get
		{
			return (InputManager.Devices != null && InputManager.Devices.Count > 0) ? SettingsManager._inputType : InputModuleManager.InputType.Mouse;
		}
		set
		{
			SettingsManager._inputType = value;
			CustomPlayerPrefs.SetInt("InputType", (int)SettingsManager._inputType);
			InputModuleManager.SetInputType(SettingsManager._inputType);
		}
	}

	public static InputModuleManager.InputType SavedInputType
	{
		get
		{
			return (InputModuleManager.InputType)CustomPlayerPrefs.GetInt("InputType");
		}
	}

	public static bool VibrateIfNotTensioned
	{
		get
		{
			return SettingsManager._vibrateIfNotTensioned;
		}
		set
		{
			SettingsManager._vibrateIfNotTensioned = value;
			CustomPlayerPrefs.SetInt("VibrateIfNotTensioned", (!SettingsManager._vibrateIfNotTensioned) ? 0 : 1);
		}
	}

	public static bool Vibrate
	{
		get
		{
			return SettingsManager._vibrate;
		}
		set
		{
			SettingsManager._vibrate = value;
			CustomPlayerPrefs.SetInt("Vibrate", (!SettingsManager._vibrate) ? 0 : 1);
			DebugUtility.Settings.Trace("Vibrate: {0}", new object[] { CustomPlayerPrefs.GetInt("Vibrate") });
		}
	}

	public static bool SSAO
	{
		get
		{
			return SettingsManager._ssao;
		}
		set
		{
			SettingsManager._ssao = value;
			CustomPlayerPrefs.SetInt("SSAO", (!SettingsManager._ssao) ? 0 : 1);
		}
	}

	public static bool VSync
	{
		get
		{
			return SettingsManager._vsync;
		}
		set
		{
			SettingsManager._vsync = value;
			QualitySettings.vSyncCount = ((!SettingsManager._vsync) ? 0 : 1);
			CustomPlayerPrefs.SetInt("VSync", (!SettingsManager._vsync) ? 0 : 1);
		}
	}

	public static bool InvertMouse
	{
		get
		{
			return SettingsManager._invertMouse;
		}
		set
		{
			SettingsManager._invertMouse = value;
			CustomPlayerPrefs.SetInt("InvertMouse", (!SettingsManager._invertMouse) ? 0 : 1);
		}
	}

	public static bool InvertController
	{
		get
		{
			return SettingsManager._invertController;
		}
		set
		{
			SettingsManager._invertController = value;
			CustomPlayerPrefs.SetInt("InvertController", (!SettingsManager._invertController) ? 0 : 1);
		}
	}

	public static bool RightHandedLayout
	{
		get
		{
			return SettingsManager._rightHandedLayout;
		}
		set
		{
			if (ControlsController.ControlsActions != null)
			{
				ControlsController.ControlsActions.ChangeToRightHanded(value);
			}
			SettingsManager._rightHandedLayout = value;
			CustomPlayerPrefs.SetInt("RightHandedLayout", (!SettingsManager._rightHandedLayout) ? 0 : 1);
		}
	}

	public static float ClampedMusicVolume
	{
		get
		{
			return SettingsManager._soundMusicVolume * 0.25f;
		}
	}

	public static float MusicVolume
	{
		get
		{
			return SettingsManager._soundMusicVolume;
		}
		set
		{
			SettingsManager._soundMusicVolume = value;
			CustomPlayerPrefs.SetFloat("SoundMusicVolume", SettingsManager._soundMusicVolume);
		}
	}

	public static float EnvironmentForcedVolume
	{
		get
		{
			if (SettingsManager._soundEnvironmentVolume < 0.5f)
			{
				return SettingsManager._soundEnvironmentVolume * 2f * 0.8f;
			}
			return Mathf.Lerp(0.8f, 1f, (SettingsManager._soundEnvironmentVolume - 0.5f) * 2f);
		}
	}

	public static float EnvironmentVolume
	{
		get
		{
			return SettingsManager._soundEnvironmentVolume;
		}
		set
		{
			SettingsManager._soundEnvironmentVolume = value;
			CustomPlayerPrefs.SetFloat("EnvironmentVolume", SettingsManager._soundEnvironmentVolume);
			GlobalConsts.BgVolume = SettingsManager.EnvironmentForcedVolume;
			if (GameFactory.AudioController != null)
			{
				GameFactory.AudioController.OnEnvironmentVolumeChanged(SettingsManager.EnvironmentForcedVolume);
			}
			WeatherController.UpdateEnvironmentVolume();
		}
	}

	public static float MouseSensitivity
	{
		get
		{
			return SettingsManager._mouseSensitivity;
		}
		set
		{
			SettingsManager._mouseSensitivity = value;
			CustomPlayerPrefs.SetFloat("MouseSensitivity", SettingsManager._mouseSensitivity);
		}
	}

	public static float ControllerSensitivity
	{
		get
		{
			return SettingsManager._controllerSensitivity;
		}
		set
		{
			SettingsManager._controllerSensitivity = value;
			CustomPlayerPrefs.SetFloat("ControllerSensitivity", SettingsManager._controllerSensitivity);
		}
	}

	public static float BobberScale
	{
		get
		{
			return SettingsManager._bobberScale;
		}
		set
		{
			SettingsManager._bobberScale = value;
			Shader.SetGlobalFloat("_BobberScale", Mathf.Lerp(0.7f, 1.3f, SettingsManager._bobberScale));
			GlobalConsts.BobberScale = Mathf.Lerp(0.7f, 1.3f, SettingsManager._bobberScale);
			CustomPlayerPrefs.SetFloat("BobberScale", SettingsManager._bobberScale);
		}
	}

	public static bool IsFullScreen
	{
		get
		{
			return SettingsManager._isFullScreen;
		}
		set
		{
			SettingsManager._isFullScreen = value;
			CustomPlayerPrefs.SetInt("IsFullScreen", (!SettingsManager._isFullScreen) ? 0 : 1);
		}
	}

	public static bool ShowTips
	{
		get
		{
			return SettingsManager._showTips;
		}
		set
		{
			SettingsManager._showTips = value;
			CustomPlayerPrefs.SetInt("ShowTips", (!SettingsManager._showTips) ? 0 : 1);
		}
	}

	public static bool ShowSlides
	{
		get
		{
			return SettingsManager._showSlides;
		}
		set
		{
			SettingsManager._showSlides = value;
			CustomPlayerPrefs.SetInt("ShowSlides", (!SettingsManager._showSlides) ? 0 : 1);
		}
	}

	public static bool ShowCharacters
	{
		get
		{
			return SettingsManager._showCharacters;
		}
		set
		{
			SettingsManager._showCharacters = value;
			CustomPlayerPrefs.SetInt("ShowCharacters", (!SettingsManager._showCharacters) ? 0 : 1);
		}
	}

	public static bool ShowCharacterBubble
	{
		get
		{
			return SettingsManager._showCharactersBubble;
		}
		set
		{
			SettingsManager._showCharactersBubble = value;
			CustomPlayerPrefs.SetInt("ShowCharactersBubble", (!SettingsManager._showCharactersBubble) ? 0 : 1);
		}
	}

	public static bool ReverseBoatBackwardsMoving
	{
		get
		{
			return SettingsManager._reverseBoatBackwardsMoving;
		}
		set
		{
			SettingsManager._reverseBoatBackwardsMoving = value;
			CustomPlayerPrefs.SetInt("ReverseBoatBackwardsMoving", (!SettingsManager._reverseBoatBackwardsMoving) ? 0 : 1);
		}
	}

	public static bool SnagsWarnings
	{
		get
		{
			return SettingsManager._snagsWarnings;
		}
		set
		{
			SettingsManager._snagsWarnings = value;
			CustomPlayerPrefs.SetInt("SnagsWarnings", (!SettingsManager._snagsWarnings) ? 0 : 1);
		}
	}

	public static bool FishingIndicator
	{
		get
		{
			return SettingsManager._fishingIndicator;
		}
		set
		{
			SettingsManager._fishingIndicator = value;
			CustomPlayerPrefs.SetInt("FishingIndicator", (!SettingsManager._fishingIndicator) ? 0 : 1);
		}
	}

	public static bool BobberBiteSound
	{
		get
		{
			return SettingsManager._bobberBiteSound;
		}
		set
		{
			SettingsManager._bobberBiteSound = value;
			CustomPlayerPrefs.SetInt("BobberBiteSound", (!SettingsManager._bobberBiteSound) ? 0 : 1);
		}
	}

	public static bool VoiceChatMuted
	{
		get
		{
			return SettingsManager._voiceChatMuted;
		}
		set
		{
			SettingsManager._voiceChatMuted = value;
			CustomPlayerPrefs.SetInt("VoiceChatMuted", (!SettingsManager._voiceChatMuted) ? 0 : 1);
		}
	}

	public static bool UseOnlyXboxHelps
	{
		get
		{
			return SettingsManager._useOnlyXboxHelps;
		}
		set
		{
			SettingsManager._useOnlyXboxHelps = value;
			if (SettingsManager._useOnlyXboxHelps)
			{
				SettingsManager.InputType = InputModuleManager.InputType.Mouse;
			}
			CustomPlayerPrefs.SetInt("UseOnlyXboxHelps", (!SettingsManager._useOnlyXboxHelps) ? 0 : 1);
		}
	}

	public static bool UseOnlyPs4Helps
	{
		get
		{
			return SettingsManager._useOnlyPs4Helps;
		}
		set
		{
			SettingsManager._useOnlyPs4Helps = value;
			if (SettingsManager._useOnlyPs4Helps)
			{
				SettingsManager.InputType = InputModuleManager.InputType.Mouse;
			}
			CustomPlayerPrefs.SetInt("UseOnlyPs4Helps", (!SettingsManager._useOnlyPs4Helps) ? 0 : 1);
		}
	}

	public static FightIndicator FightIndicator
	{
		get
		{
			return SettingsManager._fightIndicator;
		}
		set
		{
			SettingsManager._fightIndicator = value;
			CustomPlayerPrefs.SetInt("FightIndicator", (int)SettingsManager._fightIndicator);
		}
	}

	public static bool HideWhatsNew
	{
		get
		{
			return SettingsManager._hideWhatsNew;
		}
		set
		{
			SettingsManager._hideWhatsNew = value;
			CustomPlayerPrefs.SetInt("HideWhatsNew", (!SettingsManager._hideWhatsNew) ? 0 : 1);
		}
	}

	public static void SetDefaultVideo()
	{
		SettingsManager.CurrentResolution = SettingsManager._defaultResolution;
		SettingsManager.SSAO = SettingsManager._defaultSsao;
		SettingsManager.VSync = SettingsManager._defaultVsync;
		SettingsManager.RenderQuality = SettingsManager._defaultRenderQuality;
		SettingsManager.Antialiasing = SettingsManager._defaultAntialiasing;
		SettingsManager.DynWater = SettingsManager._defaultDynWater;
		SettingsManager.IsFullScreen = SettingsManager._defaultIsFullScreen;
	}

	public static void SetDefaultAudio()
	{
		SettingsManager.SpeakerMode = SettingsManager._defaultSpeakerMode;
		SettingsManager.VoiceChatVolume = SettingsManager._defaultVoiceChatVolume;
		SettingsManager.InterfaceVolume = SettingsManager._defaultInterfaceVolume;
		SettingsManager.SoundVolume = SettingsManager._defaultSoundVolume;
		SettingsManager.MusicVolume = SettingsManager._defaultSoundMusicVolume;
		SettingsManager.EnvironmentVolume = SettingsManager._defaultSoundEnvironmentVolume;
	}

	public static void SetDefaultMouse()
	{
		SettingsManager.MouseSensitivity = SettingsManager._defaultMouseSensitivity;
		SettingsManager.InvertMouse = SettingsManager._defaultInvertMouse;
		SettingsManager.MouseWheel = SettingsManager._defaultMouseWheel;
	}

	public static void SetDefaultGameplay()
	{
		SettingsManager.FightIndicator = SettingsManager._defaultfightIndicator;
		SettingsManager.BobberScale = SettingsManager._defaultBobberScale;
		SettingsManager.ShowTips = SettingsManager._defaultShowTips;
		SettingsManager.ShowSlides = SettingsManager._defaultShowSlides;
		SettingsManager.ShowCharacters = SettingsManager._defaultShowCharacters;
		SettingsManager.ShowCharacterBubble = SettingsManager._defaultShowCharactersBubble;
		SettingsManager.UseOnlyPs4Helps = SettingsManager._useOnlyPs4Helps;
		SettingsManager.UseOnlyXboxHelps = SettingsManager._useOnlyXboxHelps;
		SettingsManager.ReverseBoatBackwardsMoving = SettingsManager._defaultReverseBoatBackwardsMoving;
		SettingsManager.SnagsWarnings = SettingsManager._defaultSnagsWarnings;
		SettingsManager.FishingIndicator = SettingsManager._defaultFishingIndicator;
	}

	public static void SetDefaultController()
	{
		SettingsManager.InputType = SettingsManager._defaultInputType;
		SettingsManager.RightHandedLayout = SettingsManager._defaultRightHandedLayout;
		SettingsManager.Vibrate = SettingsManager._defaultVibrate;
		SettingsManager.VibrateIfNotTensioned = SettingsManager._defaultvibrateIfNotTensioned;
		SettingsManager.InvertController = SettingsManager._defaultInvertController;
		SettingsManager.ControllerSensitivity = SettingsManager._defaultControllerSensitivity;
	}

	private const string NVIDIA_PATTERN = "nvidia.*\\b(\\d{3,})";

	private const string AMD_R_PATTERN = "amd radeon r(\\d)\\s";

	private static float _defaultSoundVolume = 0.5f;

	private static float _defaultInterfaceVolume = 0.5f;

	private static float _defaultVoiceChatVolume = 0.5f;

	private static float _defaultSoundMusicVolume = 0.25f;

	private static float _defaultSoundEnvironmentVolume = 0.5f;

	private static float _defaultMouseSensitivity = 0.5f;

	private static float _defaultControllerSensitivity = 0.5f;

	private static AntialiasingValue _defaultAntialiasing = AntialiasingValue.Off;

	private static bool _defaultSsao = false;

	private static bool _defaultVibrate = true;

	private static bool _defaultvibrateIfNotTensioned = true;

	private static bool _defaultVsync = true;

	private static bool _defaultInvertMouse = false;

	private static bool _defaultInvertController = false;

	private static Resolution _defaultResolution = default(Resolution);

	private static bool _defaultIsFullScreen = true;

	private static DynWaterValue _defaultDynWater = DynWaterValue.Low;

	private static AudioSpeakerMode _defaultSpeakerMode = 2;

	private static RenderQualities _defaultRenderQuality = RenderQualities.Good;

	private static float _defaultBobberScale = 0.5f;

	private static bool _defaultShowTips = true;

	private static bool _defaultShowSlides = false;

	private static bool _defaultShowCharacters = true;

	private static bool _defaultShowCharactersBubble = true;

	private static bool _defaultReverseBoatBackwardsMoving = false;

	private static bool _defaultSnagsWarnings = false;

	private static bool _defaultFishingIndicator = true;

	private static bool _defaultBobberBiteSound = false;

	private static bool _defaultVoiceChatMuted = false;

	private static bool _defaultUseOnlyPs4Helps = false;

	private static bool _defaultUseOnlyXboxHelps = false;

	private static bool _defaultRightHandedLayout = true;

	private static FightIndicator _defaultfightIndicator = FightIndicator.OneBand;

	private static MouseWheelValue _defaultMouseWheel = MouseWheelValue.Reel;

	private static InputModuleManager.InputType _defaultInputType = InputModuleManager.InputType.GamePad;

	private static float _soundVolume = SettingsManager._defaultSoundVolume;

	private static float _interfaceVolume = SettingsManager._defaultInterfaceVolume;

	private static float _voiceChatVolume = SettingsManager._defaultVoiceChatVolume;

	private static float _soundMusicVolume = SettingsManager._defaultSoundMusicVolume;

	private static float _soundEnvironmentVolume = SettingsManager._defaultSoundEnvironmentVolume;

	private static float _mouseSensitivity = SettingsManager._defaultMouseSensitivity;

	private static float _controllerSensitivity = SettingsManager._defaultControllerSensitivity;

	private static AntialiasingValue _antialiasing = SettingsManager._defaultAntialiasing;

	private static bool _ssao = SettingsManager._defaultSsao;

	private static bool _vibrate = SettingsManager._defaultVibrate;

	private static bool _vibrateIfNotTensioned = SettingsManager._defaultvibrateIfNotTensioned;

	private static bool _vsync = SettingsManager._defaultVsync;

	private static bool _invertMouse;

	private static bool _invertController;

	private static bool _rightHandedLayout = SettingsManager._defaultRightHandedLayout;

	private static Resolution _resolution = SettingsManager._defaultResolution;

	private static bool _isFullScreen = SettingsManager._defaultIsFullScreen;

	private static bool _showTips = SettingsManager._defaultShowTips;

	private static bool _showCharacters = SettingsManager._defaultShowCharacters;

	private static bool _showCharactersBubble = SettingsManager._defaultShowCharactersBubble;

	private static bool _reverseBoatBackwardsMoving = SettingsManager._defaultReverseBoatBackwardsMoving;

	private static bool _snagsWarnings = SettingsManager._defaultSnagsWarnings;

	private static bool _fishingIndicator = SettingsManager._defaultFishingIndicator;

	private static bool _bobberBiteSound = SettingsManager._defaultBobberBiteSound;

	private static bool _voiceChatMuted = SettingsManager._defaultVoiceChatMuted;

	private static bool _useOnlyPs4Helps = SettingsManager._defaultUseOnlyPs4Helps;

	private static bool _useOnlyXboxHelps = SettingsManager._defaultUseOnlyXboxHelps;

	private static bool _showSlides = SettingsManager._defaultShowSlides;

	private static DynWaterValue _dynWater = SettingsManager._defaultDynWater;

	private static RenderQualities _renderQuality = SettingsManager._defaultRenderQuality;

	private static AudioSpeakerMode _speakerMode = SettingsManager._defaultSpeakerMode;

	private static float _bobberScale = SettingsManager._defaultBobberScale;

	private static bool _hideWhatsNew;

	private static FightIndicator _fightIndicator = SettingsManager._defaultfightIndicator;

	private static MouseWheelValue _mouseWheel = SettingsManager._defaultMouseWheel;

	private static InputModuleManager.InputType _inputType = SettingsManager._defaultInputType;

	private static WinFullScreenModeValue _winFullscreenMode;

	private static OSXFullScreenModeValue _osxFullscreenMode;

	private const float ENVIRONMENT_VOLUME_AT_50PRC = 0.8f;
}
