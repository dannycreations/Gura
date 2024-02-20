using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Ovr;
using UnityEngine;

public class OVRManager : MonoBehaviour
{
	public static OVRManager instance { get; private set; }

	public static Hmd capiHmd
	{
		get
		{
			if (OVRManager._capiHmd == null)
			{
				IntPtr zero = IntPtr.Zero;
				OVRManager.OVR_GetHMD(ref zero);
				OVRManager._capiHmd = ((!(zero != IntPtr.Zero)) ? null : new Hmd(zero));
			}
			return OVRManager._capiHmd;
		}
	}

	public static OVRDisplay display { get; private set; }

	public static OVRTracker tracker { get; private set; }

	public static OVRManager.Profile profile
	{
		get
		{
			if (!OVRManager._profileIsCached)
			{
				float @float = OVRManager.capiHmd.GetFloat("IPD", 0.064f);
				float float2 = OVRManager.capiHmd.GetFloat("EyeHeight", 1.675f);
				float[] array = new float[] { 0.0805f, 0.075f };
				float[] floatArray = OVRManager.capiHmd.GetFloatArray("NeckEyeDistance", array);
				float num = float2 - floatArray[1];
				OVRManager._profile = new OVRManager.Profile
				{
					ipd = @float,
					eyeHeight = float2,
					eyeDepth = floatArray[0],
					neckHeight = num
				};
				OVRManager._profileIsCached = true;
			}
			return OVRManager._profile;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action HMDAcquired;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action HMDLost;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action TrackingAcquired;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action TrackingLost;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event Action HSWDismissed;

	public static bool isHSWDisplayed
	{
		get
		{
			return OVRManager.capiHmd.GetHSWDisplayState().Displayed;
		}
	}

	public static void DismissHSWDisplay()
	{
		OVRManager.capiHmd.DismissHSWDisplay();
	}

	public static float batteryLevel
	{
		get
		{
			return 1f;
		}
	}

	public static float batteryTemperature
	{
		get
		{
			return 0f;
		}
	}

	public static int batteryStatus
	{
		get
		{
			return 0;
		}
	}

	public bool isSupportedPlatform { get; private set; }

	public static bool isPaused
	{
		get
		{
			return OVRManager._isPaused;
		}
		set
		{
			OVRManager._isPaused = value;
		}
	}

	private void Awake()
	{
		if (OVRManager.instance != null)
		{
			base.enabled = false;
			Object.DestroyImmediate(this);
			return;
		}
		OVRManager.instance = this;
		Version version = new Version("0.4.3");
		Version version2 = new Version(Hmd.GetVersionString());
		if (version > version2)
		{
			Debug.LogWarning("Using an older version of LibOVR.");
		}
		RuntimePlatform platform = Application.platform;
		this.isSupportedPlatform |= platform == 11;
		this.isSupportedPlatform |= platform == 13;
		this.isSupportedPlatform |= platform == 0;
		this.isSupportedPlatform |= platform == 1;
		this.isSupportedPlatform |= platform == 7;
		this.isSupportedPlatform |= platform == 2;
		if (!this.isSupportedPlatform)
		{
			Debug.LogWarning("This platform is unsupported");
			return;
		}
		OVRManager.SetEditorPlay(Application.isEditor);
		if (OVRManager.display == null)
		{
			OVRManager.display = new OVRDisplay();
		}
		if (OVRManager.tracker == null)
		{
			OVRManager.tracker = new OVRTracker();
		}
		if (this.resetTrackerOnLoad)
		{
			OVRManager.display.RecenterPose();
		}
		if (this.timeWarp)
		{
			bool flag = SystemInfo.graphicsDeviceVersion.Contains("Direct3D 9");
			QualitySettings.vSyncCount = ((!flag) ? 0 : 1);
		}
	}

	private void Start()
	{
		Camera camera = base.GetComponent<Camera>();
		if (camera == null)
		{
			camera = base.gameObject.AddComponent<Camera>();
			camera.cullingMask = 0;
			camera.clearFlags = 4;
			camera.renderingPath = 1;
			camera.orthographic = true;
			camera.useOcclusionCulling = false;
		}
		bool flag = SystemInfo.graphicsDeviceVersion.Contains("Direct3D") || (Application.platform == 7 && SystemInfo.graphicsDeviceVersion.Contains("emulated"));
		OVRManager.display.flipInput = flag;
		base.StartCoroutine(this.CallbackCoroutine());
	}

	private void Update()
	{
		if (this.usePositionTracking != OVRManager.usingPositionTracking)
		{
			OVRManager.tracker.isEnabled = this.usePositionTracking;
			OVRManager.usingPositionTracking = this.usePositionTracking;
		}
		if (OVRManager.HMDLost != null && OVRManager.wasHmdPresent && !OVRManager.display.isPresent)
		{
			OVRManager.HMDLost();
		}
		if (OVRManager.HMDAcquired != null && !OVRManager.wasHmdPresent && OVRManager.display.isPresent)
		{
			OVRManager.HMDAcquired();
		}
		OVRManager.wasHmdPresent = OVRManager.display.isPresent;
		if (OVRManager.TrackingLost != null && OVRManager.wasPositionTracked && !OVRManager.tracker.isPositionTracked)
		{
			OVRManager.TrackingLost();
		}
		if (OVRManager.TrackingAcquired != null && !OVRManager.wasPositionTracked && OVRManager.tracker.isPositionTracked)
		{
			OVRManager.TrackingAcquired();
		}
		OVRManager.wasPositionTracked = OVRManager.tracker.isPositionTracked;
		if (OVRManager.isHSWDisplayed && Input.anyKeyDown)
		{
			OVRManager.DismissHSWDisplay();
			if (OVRManager.HSWDismissed != null)
			{
				OVRManager.HSWDismissed();
			}
		}
		OVRManager.display.timeWarp = this.timeWarp;
		OVRManager.display.Update();
	}

	private void LateUpdate()
	{
		OVRManager.display.BeginFrame();
	}

	private IEnumerator CallbackCoroutine()
	{
		for (;;)
		{
			yield return OVRManager.waitForEndOfFrame;
			OVRManager.display.EndFrame();
		}
		yield break;
	}

	public static void SetEditorPlay(bool isEditor)
	{
		OVRManager.OVR_SetEditorPlay(isEditor);
	}

	public static void SetDistortionCaps(uint distortionCaps)
	{
		OVRManager.OVR_SetDistortionCaps(distortionCaps);
	}

	public static void SetInitVariables(IntPtr activity, IntPtr vrActivityClass)
	{
	}

	public static void PlatformUIConfirmQuit()
	{
	}

	public static void PlatformUIGlobalMenu()
	{
	}

	public static void DoTimeWarp(int timeWarpViewNumber)
	{
	}

	public static void EndEye(OVREye eye, int eyeTextureId)
	{
	}

	public static void InitRenderThread()
	{
	}

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void OVR_GetHMD(ref IntPtr hmdPtr);

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void OVR_SetEditorPlay(bool isEditorPlay);

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void OVR_SetDistortionCaps(uint distortionCaps);

	[DllImport("user32", CharSet = CharSet.Ansi, EntryPoint = "MessageBoxA")]
	public static extern bool MessageBox(int hWnd, [MarshalAs(UnmanagedType.LPStr)] string text, [MarshalAs(UnmanagedType.LPStr)] string caption, uint type);

	private static Hmd _capiHmd;

	private static bool _profileIsCached = false;

	private static OVRManager.Profile _profile;

	public float nativeTextureScale = 1f;

	public float virtualTextureScale = 1f;

	public bool usePositionTracking = true;

	public RenderTextureFormat eyeTextureFormat = 7;

	public int eyeTextureDepth = 24;

	public bool timeWarp = true;

	public bool freezeTimeWarp;

	public bool resetTrackerOnLoad = true;

	public bool monoscopic;

	private static bool usingPositionTracking = false;

	private static bool wasHmdPresent = false;

	private static bool wasPositionTracked = false;

	private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	private static bool _isPaused;

	private const string LibOVR = "OculusPlugin";

	public struct Profile
	{
		public float ipd;

		public float eyeHeight;

		public float eyeDepth;

		public float neckHeight;
	}
}
