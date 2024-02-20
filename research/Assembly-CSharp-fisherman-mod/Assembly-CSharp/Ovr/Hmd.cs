using System;
using System.Runtime.InteropServices;

namespace Ovr
{
	public class Hmd
	{
		public Hmd(IntPtr hmdPtr)
		{
			float[] array = new float[7];
			array[3] = 1f;
			this.OVR_DEFAULT_CAMERA_POSITION = array;
			this.LatencyTestRgb = new byte[3];
			base..ctor();
			this.HmdPtr = hmdPtr;
		}

		public static void InitializeRenderingShim()
		{
			Hmd.ovr_InitializeRenderingShim();
		}

		public static bool Initialize()
		{
			return (int)Hmd.ovr_Initialize() != 0;
		}

		public static void Shutdown()
		{
			Hmd.ovr_Shutdown();
		}

		public static string GetVersionString()
		{
			return Marshal.PtrToStringAnsi(Hmd.ovr_GetVersionString());
		}

		public static int Detect()
		{
			return Hmd.ovrHmd_Detect();
		}

		public static Hmd Create(int index)
		{
			IntPtr intPtr = Hmd.ovrHmd_Create(index);
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			return new Hmd(intPtr);
		}

		public static Hmd CreateDebug(HmdType type)
		{
			IntPtr intPtr = Hmd.ovrHmd_CreateDebug(type);
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			return new Hmd(intPtr);
		}

		public static Matrix4f GetProjection(FovPort fov, float znear, float zfar, bool rightHanded)
		{
			return new Matrix4f(Hmd.ovrMatrix4f_Projection(fov, znear, zfar, rightHanded));
		}

		public static Matrix4f GetOrthoSubProjection(Matrix4f projection, Vector2f orthoScale, float orthoDistance, float hmdToEyeViewOffsetX)
		{
			return new Matrix4f(Hmd.ovrMatrix4f_OrthoSubProjection(projection, orthoScale, orthoDistance, hmdToEyeViewOffsetX));
		}

		public static double GetTimeInSeconds()
		{
			return Hmd.ovr_GetTimeInSeconds();
		}

		public static double WaitTillTime(double absTime)
		{
			return Hmd.ovr_WaitTillTime(absTime);
		}

		public string GetLastError()
		{
			return Hmd.ovrHmd_GetLastError(this.HmdPtr);
		}

		public uint GetEnabledCaps()
		{
			return Hmd.ovrHmd_GetEnabledCaps(this.HmdPtr);
		}

		public void SetEnabledCaps(uint capsBits)
		{
			Hmd.ovrHmd_SetEnabledCaps(this.HmdPtr, capsBits);
		}

		public HmdDesc GetDesc()
		{
			HmdDesc_Raw hmdDesc_Raw = (HmdDesc_Raw)Marshal.PtrToStructure(this.HmdPtr, typeof(HmdDesc_Raw));
			return new HmdDesc(hmdDesc_Raw);
		}

		public bool ConfigureTracking(uint supportedTrackingCaps, uint requiredTrackingCaps)
		{
			return (int)Hmd.ovrHmd_ConfigureTracking(this.HmdPtr, supportedTrackingCaps, requiredTrackingCaps) != 0;
		}

		public void RecenterPose()
		{
			Hmd.ovrHmd_RecenterPose(this.HmdPtr);
		}

		public TrackingState GetTrackingState(double absTime = 0.0)
		{
			return Hmd.ovrHmd_GetTrackingState(this.HmdPtr, absTime);
		}

		public Sizei GetFovTextureSize(Eye eye, FovPort fov, float pixelsPerDisplayPixel = 1f)
		{
			return Hmd.ovrHmd_GetFovTextureSize(this.HmdPtr, eye, fov, pixelsPerDisplayPixel);
		}

		public EyeRenderDesc[] ConfigureRendering(ref RenderAPIConfig renderAPIConfig, FovPort[] eyeFovIn, uint distortionCaps)
		{
			EyeRenderDesc[] array = new EyeRenderDesc[]
			{
				default(EyeRenderDesc),
				default(EyeRenderDesc)
			};
			RenderAPIConfig_Raw renderAPIConfig_Raw = renderAPIConfig.ToRaw();
			bool flag = (int)Hmd.ovrHmd_ConfigureRendering(this.HmdPtr, ref renderAPIConfig_Raw, distortionCaps, eyeFovIn, array) != 0;
			if (flag)
			{
				return array;
			}
			return null;
		}

		public FrameTiming BeginFrame(uint frameIndex = 0U)
		{
			FrameTiming_Raw frameTiming_Raw = Hmd.ovrHmd_BeginFrame(this.HmdPtr, frameIndex);
			return new FrameTiming(frameTiming_Raw);
		}

		public void EndFrame(Posef[] renderPose, Texture[] eyeTexture)
		{
			Texture_Raw[] array = new Texture_Raw[eyeTexture.Length];
			for (int i = 0; i < eyeTexture.Length; i++)
			{
				array[i] = eyeTexture[i].ToRaw();
			}
			Hmd.ovrHmd_EndFrame(this.HmdPtr, renderPose, array);
		}

		public Posef[] GetEyePoses(uint frameIndex)
		{
			FovPort fovPort = this.GetDesc().DefaultEyeFov[0];
			FovPort fovPort2 = this.GetDesc().DefaultEyeFov[1];
			EyeRenderDesc renderDesc = this.GetRenderDesc(Eye.Left, fovPort);
			EyeRenderDesc renderDesc2 = this.GetRenderDesc(Eye.Right, fovPort2);
			TrackingState trackingState = default(TrackingState);
			Vector3f[] array = new Vector3f[] { renderDesc.HmdToEyeViewOffset, renderDesc2.HmdToEyeViewOffset };
			Posef[] array2 = new Posef[]
			{
				default(Posef),
				default(Posef)
			};
			Hmd.ovrHmd_GetEyePoses(this.HmdPtr, frameIndex, array, array2, ref trackingState);
			return array2;
		}

		public Posef GetHmdPosePerEye(Eye eye)
		{
			return Hmd.ovrHmd_GetHmdPosePerEye(this.HmdPtr, eye);
		}

		public EyeRenderDesc GetRenderDesc(Eye eyeType, FovPort fov)
		{
			return Hmd.ovrHmd_GetRenderDesc(this.HmdPtr, eyeType, fov);
		}

		public DistortionMesh? CreateDistortionMesh(Eye eye, FovPort fov, uint distortionCaps)
		{
			DistortionMesh_Raw distortionMesh_Raw = default(DistortionMesh_Raw);
			if ((int)Hmd.ovrHmd_CreateDistortionMesh(this.HmdPtr, eye, fov, distortionCaps, out distortionMesh_Raw) == 0)
			{
				return null;
			}
			DistortionMesh distortionMesh = new DistortionMesh(distortionMesh_Raw);
			Hmd.ovrHmd_DestroyDistortionMesh(ref distortionMesh_Raw);
			return new DistortionMesh?(distortionMesh);
		}

		public Vector2f[] GetRenderScaleAndOffset(FovPort fov, Sizei textureSize, Recti renderViewport)
		{
			Vector2f[] array = new Vector2f[]
			{
				default(Vector2f),
				default(Vector2f)
			};
			Hmd.ovrHmd_GetRenderScaleAndOffset(fov, textureSize, renderViewport, array);
			return array;
		}

		public FrameTiming GetFrameTiming(uint frameIndex)
		{
			FrameTiming_Raw frameTiming_Raw = Hmd.ovrHmd_GetFrameTiming(this.HmdPtr, frameIndex);
			return new FrameTiming(frameTiming_Raw);
		}

		public FrameTiming BeginFrameTiming(uint frameIndex)
		{
			FrameTiming_Raw frameTiming_Raw = Hmd.ovrHmd_BeginFrameTiming(this.HmdPtr, frameIndex);
			return new FrameTiming(frameTiming_Raw);
		}

		public void EndFrameTiming()
		{
			Hmd.ovrHmd_EndFrameTiming(this.HmdPtr);
		}

		public void ResetFrameTiming(uint frameIndex)
		{
			Hmd.ovrHmd_ResetFrameTiming(this.HmdPtr, frameIndex);
		}

		public Matrix4f[] GetEyeTimewarpMatrices(Eye eye, Posef renderPose)
		{
			Matrix4f_Raw[] array = new Matrix4f_Raw[]
			{
				default(Matrix4f_Raw),
				default(Matrix4f_Raw)
			};
			Hmd.ovrHmd_GetEyeTimewarpMatrices(this.HmdPtr, eye, renderPose, array);
			return new Matrix4f[]
			{
				new Matrix4f(array[0]),
				new Matrix4f(array[1])
			};
		}

		public byte[] ProcessLatencyTest()
		{
			if ((int)Hmd.ovrHmd_ProcessLatencyTest(this.HmdPtr, this.LatencyTestRgb) != 0)
			{
				return this.LatencyTestRgb;
			}
			return null;
		}

		public string GetLatencyTestResult()
		{
			IntPtr intPtr = Hmd.ovrHmd_GetLatencyTestResult(this.HmdPtr);
			return (!(intPtr == IntPtr.Zero)) ? Marshal.PtrToStringAnsi(intPtr) : null;
		}

		public byte[] GetLatencyTest2DrawColor()
		{
			if ((int)Hmd.ovrHmd_GetLatencyTest2DrawColor(this.HmdPtr, this.LatencyTestRgb) != 0)
			{
				return this.LatencyTestRgb;
			}
			return null;
		}

		public HSWDisplayState GetHSWDisplayState()
		{
			HSWDisplayState hswdisplayState;
			Hmd.ovrHmd_GetHSWDisplayState(this.HmdPtr, out hswdisplayState);
			return hswdisplayState;
		}

		public bool DismissHSWDisplay()
		{
			return (int)Hmd.ovrHmd_DismissHSWDisplay(this.HmdPtr) != 0;
		}

		public bool GetBool(string propertyName, bool defaultVal = false)
		{
			return (int)Hmd.ovrHmd_GetBool(this.HmdPtr, propertyName, defaultVal) != 0;
		}

		public bool SetBool(string propertyName, bool val)
		{
			return (int)Hmd.ovrHmd_SetBool(this.HmdPtr, propertyName, val) != 0;
		}

		public int GetInt(string propertyName, int defaultVal = 0)
		{
			return Hmd.ovrHmd_GetInt(this.HmdPtr, propertyName, defaultVal);
		}

		public bool SetInt(string propertyName, int val)
		{
			return (int)Hmd.ovrHmd_SetInt(this.HmdPtr, propertyName, val) != 0;
		}

		public float GetFloat(string propertyName, float defaultVal = 0f)
		{
			return Hmd.ovrHmd_GetFloat(this.HmdPtr, propertyName, defaultVal);
		}

		public bool SetFloat(string propertyName, float val)
		{
			return (int)Hmd.ovrHmd_SetFloat(this.HmdPtr, propertyName, val) != 0;
		}

		public float[] GetFloatArray(string propertyName, float[] values)
		{
			if (values == null)
			{
				return null;
			}
			Hmd.ovrHmd_GetFloatArray(this.HmdPtr, propertyName, values, (uint)values.Length);
			return values;
		}

		public bool SetFloatArray(string propertyName, float[] values)
		{
			if (values == null)
			{
				values = new float[0];
			}
			return (int)Hmd.ovrHmd_SetFloatArray(this.HmdPtr, propertyName, values, (uint)values.Length) != 0;
		}

		public string GetString(string propertyName, string defaultVal = null)
		{
			IntPtr intPtr = Hmd.ovrHmd_GetString(this.HmdPtr, propertyName, null);
			if (intPtr == IntPtr.Zero)
			{
				return defaultVal;
			}
			return Marshal.PtrToStringAnsi(intPtr);
		}

		public bool SetString(string propertyName, string val)
		{
			return (int)Hmd.ovrHmd_SetString(this.HmdPtr, propertyName, val) != 0;
		}

		public bool StartPerfLog(string fileName, string userData1)
		{
			return (int)Hmd.ovrHmd_StartPerfLog(this.HmdPtr, fileName, userData1) != 0;
		}

		public bool StopPerfLog()
		{
			return (int)Hmd.ovrHmd_StopPerfLog(this.HmdPtr) != 0;
		}

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovr_InitializeRenderingShim();

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovr_Initialize();

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovr_Shutdown();

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr ovr_GetVersionString();

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern int ovrHmd_Detect();

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr ovrHmd_Create(int index);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_Destroy(IntPtr hmd);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr ovrHmd_CreateDebug(HmdType type);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern string ovrHmd_GetLastError(IntPtr hmd);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_AttachToWindow(IntPtr hmd, IntPtr window, Recti destMirrorRect, Recti sourceRenderTargetRect);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern uint ovrHmd_GetEnabledCaps(IntPtr hmd);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_SetEnabledCaps(IntPtr hmd, uint capsBits);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_ConfigureTracking(IntPtr hmd, uint supportedTrackingCaps, uint requiredTrackingCaps);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_RecenterPose(IntPtr hmd);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern TrackingState ovrHmd_GetTrackingState(IntPtr hmd, double absTime);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern Sizei ovrHmd_GetFovTextureSize(IntPtr hmd, Eye eye, FovPort fov, float pixelsPerDisplayPixel);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_ConfigureRendering(IntPtr hmd, ref RenderAPIConfig_Raw apiConfig, uint distortionCaps, [In] FovPort[] eyeFovIn, [In] [Out] EyeRenderDesc[] eyeRenderDescOut);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern FrameTiming_Raw ovrHmd_BeginFrame(IntPtr hmd, uint frameIndex);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_EndFrame(IntPtr hmd, [In] Posef[] renderPose, [In] Texture_Raw[] eyeTexture);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_GetEyePoses(IntPtr hmd, uint frameIndex, [In] Vector3f[] hmdToEyeViewOffset, [In] [Out] Posef[] eyePosesOut, [In] [Out] ref TrackingState hmdTrackingStateOut);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern Posef ovrHmd_GetHmdPosePerEye(IntPtr hmd, Eye eye);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern EyeRenderDesc ovrHmd_GetRenderDesc(IntPtr hmd, Eye eye, FovPort fov);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_CreateDistortionMesh(IntPtr hmd, Eye eye, FovPort fov, uint distortionCaps, out DistortionMesh_Raw meshData);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_DestroyDistortionMesh(ref DistortionMesh_Raw meshData);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_GetRenderScaleAndOffset(FovPort fov, Sizei textureSize, Recti renderViewport, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] [Out] Vector2f[] uvScaleOffsetOut);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern FrameTiming_Raw ovrHmd_GetFrameTiming(IntPtr hmd, uint frameIndex);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern FrameTiming_Raw ovrHmd_BeginFrameTiming(IntPtr hmd, uint frameIndex);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_EndFrameTiming(IntPtr hmd);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_ResetFrameTiming(IntPtr hmd, uint frameIndex);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_GetEyeTimewarpMatrices(IntPtr hmd, Eye eye, Posef renderPose, [MarshalAs(UnmanagedType.LPArray, SizeConst = 2)] [Out] Matrix4f_Raw[] twnOut);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern Matrix4f_Raw ovrMatrix4f_Projection(FovPort fov, float znear, float zfar, bool rightHanded);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern Matrix4f_Raw ovrMatrix4f_OrthoSubProjection(Matrix4f projection, Vector2f orthoScale, float orthoDistance, float hmdToEyeViewOffsetX);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern double ovr_GetTimeInSeconds();

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern double ovr_WaitTillTime(double absTime);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_ProcessLatencyTest(IntPtr hmd, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] [Out] byte[] rgbColorOut);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr ovrHmd_GetLatencyTestResult(IntPtr hmd);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_GetLatencyTest2DrawColor(IntPtr hmd, [MarshalAs(UnmanagedType.LPArray, SizeConst = 3)] [Out] byte[] rgbColorOut);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ovrHmd_GetHSWDisplayState(IntPtr hmd, out HSWDisplayState hasWarningState);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_DismissHSWDisplay(IntPtr hmd);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_GetBool(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, bool defaultVal);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_SetBool(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, bool val);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern int ovrHmd_GetInt(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, int defaultVal);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_SetInt(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, int val);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern float ovrHmd_GetFloat(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, float defaultVal);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_SetFloat(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, float val);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern uint ovrHmd_GetFloatArray(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, float[] values, uint arraySize);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_SetFloatArray(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, float[] values, uint arraySize);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr ovrHmd_GetString(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, [MarshalAs(UnmanagedType.LPStr)] string defaultVal);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_SetString(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string propertyName, [MarshalAs(UnmanagedType.LPStr)] string val);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_StartPerfLog(IntPtr hmd, [MarshalAs(UnmanagedType.LPStr)] string fileName, [MarshalAs(UnmanagedType.LPStr)] string userData1);

		[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte ovrHmd_StopPerfLog(IntPtr hmd);

		public const string OVR_VERSION_STRING = "0.4.3";

		public const string OVR_KEY_USER = "User";

		public const string OVR_KEY_NAME = "Name";

		public const string OVR_KEY_GENDER = "Gender";

		public const string OVR_KEY_PLAYER_HEIGHT = "PlayerHeight";

		public const string OVR_KEY_EYE_HEIGHT = "EyeHeight";

		public const string OVR_KEY_IPD = "IPD";

		public const string OVR_KEY_NECK_TO_EYE_DISTANCE = "NeckEyeDistance";

		public const string OVR_KEY_EYE_RELIEF_DIAL = "EyeReliefDial";

		public const string OVR_KEY_EYE_TO_NOSE_DISTANCE = "EyeToNoseDist";

		public const string OVR_KEY_MAX_EYE_TO_PLATE_DISTANCE = "MaxEyeToPlateDist";

		public const string OVR_KEY_EYE_CUP = "EyeCup";

		public const string OVR_KEY_CUSTOM_EYE_RENDER = "CustomEyeRender";

		public const string OVR_KEY_CAMERA_POSITION = "CenteredFromWorld";

		public const string OVR_DEFAULT_GENDER = "Unknown";

		public const float OVR_DEFAULT_PLAYER_HEIGHT = 1.778f;

		public const float OVR_DEFAULT_EYE_HEIGHT = 1.675f;

		public const float OVR_DEFAULT_IPD = 0.064f;

		public const float OVR_DEFAULT_NECK_TO_EYE_HORIZONTAL = 0.0805f;

		public const float OVR_DEFAULT_NECK_TO_EYE_VERTICAL = 0.075f;

		public const float OVR_DEFAULT_EYE_RELIEF_DIAL = 3f;

		public readonly float[] OVR_DEFAULT_CAMERA_POSITION;

		private IntPtr HmdPtr;

		private byte[] LatencyTestRgb;

		public const string LibFile = "OculusPlugin";
	}
}
