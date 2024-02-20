using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class OVRGamepadController : MonoBehaviour
{
	static OVRGamepadController()
	{
		OVRGamepadController.SetAxisNames(OVRGamepadController.DefaultAxisNames);
		OVRGamepadController.SetButtonNames(OVRGamepadController.DefaultButtonNames);
	}

	public static void SetAxisNames(string[] axisNames)
	{
		OVRGamepadController.AxisNames = axisNames;
	}

	public static void SetButtonNames(string[] buttonNames)
	{
		OVRGamepadController.ButtonNames = buttonNames;
	}

	public static bool GPC_Initialize()
	{
		return OVRManager.instance.isSupportedPlatform && OVRGamepadController.OVR_GamepadController_Initialize();
	}

	public static bool GPC_Destroy()
	{
		return OVRManager.instance.isSupportedPlatform && OVRGamepadController.OVR_GamepadController_Destroy();
	}

	public static bool GPC_Update()
	{
		return OVRManager.instance.isSupportedPlatform && OVRGamepadController.OVR_GamepadController_Update();
	}

	public static float DefaultReadAxis(OVRGamepadController.Axis axis)
	{
		return OVRGamepadController.OVR_GamepadController_GetAxis((int)axis);
	}

	public static float GPC_GetAxis(OVRGamepadController.Axis axis)
	{
		if (OVRGamepadController.ReadAxis == null)
		{
			return 0f;
		}
		return OVRGamepadController.ReadAxis(axis);
	}

	public static void SetReadAxisDelegate(OVRGamepadController.ReadAxisDelegate del)
	{
		OVRGamepadController.ReadAxis = del;
	}

	public static bool DefaultReadButton(OVRGamepadController.Button button)
	{
		return OVRGamepadController.OVR_GamepadController_GetButton((int)button);
	}

	public static bool GPC_GetButton(OVRGamepadController.Button button)
	{
		return OVRGamepadController.ReadButton != null && OVRGamepadController.ReadButton(button);
	}

	public static void SetReadButtonDelegate(OVRGamepadController.ReadButtonDelegate del)
	{
		OVRGamepadController.ReadButton = del;
	}

	public static bool GPC_IsAvailable()
	{
		return OVRGamepadController.GPC_Available;
	}

	private void GPC_Test()
	{
		Debug.Log(string.Format("LT:{0:F3} RT:{1:F3} LX:{2:F3} LY:{3:F3} RX:{4:F3} RY:{5:F3}", new object[]
		{
			OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftTrigger),
			OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.RightTrigger),
			OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftXAxis),
			OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftYAxis),
			OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.RightXAxis),
			OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.RightYAxis)
		}));
		Debug.Log(string.Format("A:{0} B:{1} X:{2} Y:{3} U:{4} D:{5} L:{6} R:{7} SRT:{8} BK:{9} LS:{10} RS:{11} L1:{12} R1:{13}", new object[]
		{
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.A),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.B),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.X),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Y),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Up),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Down),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Left),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Right),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Start),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Back),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.LStick),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.RStick),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.LeftShoulder),
			OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.RightShoulder)
		}));
	}

	private void Start()
	{
		OVRGamepadController.GPC_Available = OVRGamepadController.GPC_Initialize();
	}

	private void Update()
	{
		OVRGamepadController.GPC_Available = OVRGamepadController.GPC_Update();
	}

	private void OnDestroy()
	{
		OVRGamepadController.GPC_Destroy();
		OVRGamepadController.GPC_Available = false;
	}

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool OVR_GamepadController_Initialize();

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool OVR_GamepadController_Destroy();

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool OVR_GamepadController_Update();

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	public static extern float OVR_GamepadController_GetAxis(int axis);

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool OVR_GamepadController_GetButton(int button);

	public static string[] DefaultAxisNames = new string[] { "Left_X_Axis", "Left_Y_Axis", "Right_X_Axis", "Right_Y_Axis", "LeftTrigger", "RightTrigger" };

	public static string[] DefaultButtonNames = new string[]
	{
		"Button A", "Button B", "Button X", "Button Y", "Up", "Down", "Left", "Right", "Start", "Back",
		"LStick", "RStick", "LeftShoulder", "RightShoulder"
	};

	public static int[] DefaultButtonIds = new int[]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13
	};

	public static string[] AxisNames = null;

	public static string[] ButtonNames = null;

	public static OVRGamepadController.ReadAxisDelegate ReadAxis = new OVRGamepadController.ReadAxisDelegate(OVRGamepadController.DefaultReadAxis);

	public static OVRGamepadController.ReadButtonDelegate ReadButton = new OVRGamepadController.ReadButtonDelegate(OVRGamepadController.DefaultReadButton);

	private static bool GPC_Available = false;

	public const string LibOVR = "OculusPlugin";

	public enum Axis
	{
		None = -1,
		LeftXAxis,
		LeftYAxis,
		RightXAxis,
		RightYAxis,
		LeftTrigger,
		RightTrigger,
		Max
	}

	public enum Button
	{
		None = -1,
		A,
		B,
		X,
		Y,
		Up,
		Down,
		Left,
		Right,
		Start,
		Back,
		LStick,
		RStick,
		LeftShoulder,
		RightShoulder,
		Max
	}

	public delegate float ReadAxisDelegate(OVRGamepadController.Axis axis);

	public delegate bool ReadButtonDelegate(OVRGamepadController.Button button);
}
