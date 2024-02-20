using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Ovr;
using UnityEngine;

public class OVRDisplay
{
	public OVRDisplay()
	{
		this.needsSetTexture = true;
		this.prevFullScreen = Screen.fullScreen;
		this.prevVirtualTextureScale = OVRManager.instance.virtualTextureScale;
		this.ConfigureEyeDesc(OVREye.Left);
		this.ConfigureEyeDesc(OVREye.Right);
		for (int i = 0; i < 2; i += 2)
		{
			this.ConfigureEyeTexture(i, OVREye.Left, OVRManager.instance.nativeTextureScale);
			this.ConfigureEyeTexture(i, OVREye.Right, OVRManager.instance.nativeTextureScale);
		}
	}

	public bool isPresent
	{
		get
		{
			return (OVRManager.capiHmd.GetTrackingState(0.0).StatusFlags & 128U) != 0U;
		}
	}

	public void Update()
	{
		if (OVRDisplay.frameCount < 2)
		{
			uint num = OVRManager.capiHmd.GetEnabledCaps();
			num ^= 128U;
			OVRManager.capiHmd.SetEnabledCaps(num);
		}
		this.UpdateViewport();
		this.UpdateTextures();
	}

	public void BeginFrame()
	{
		bool flag = !OVRManager.instance.timeWarp || !OVRManager.instance.freezeTimeWarp;
		if (flag)
		{
			OVRDisplay.frameCount++;
		}
		OVRPluginEvent.IssueWithData(RenderEventType.BeginFrame, OVRDisplay.frameCount);
	}

	public void EndFrame()
	{
		OVRPluginEvent.Issue(RenderEventType.EndFrame);
	}

	public OVRPose GetHeadPose(double predictionTime = 0.0)
	{
		double num = Hmd.GetTimeInSeconds() + predictionTime;
		return OVRManager.capiHmd.GetTrackingState(num).HeadPose.ThePose.ToPose(true);
	}

	public OVRPose GetEyePose(OVREye eye)
	{
		bool flag = !OVRManager.instance.timeWarp || !OVRManager.instance.freezeTimeWarp;
		if (flag)
		{
			this.eyePoses[(int)eye] = OVRDisplay.OVR_GetRenderPose(OVRDisplay.frameCount, (int)eye).ToPose(true);
		}
		return this.eyePoses[(int)eye];
	}

	public Matrix4x4 GetProjection(int eyeId, float nearClip, float farClip)
	{
		FovPort fovPort = OVRManager.capiHmd.GetDesc().DefaultEyeFov[eyeId];
		return Hmd.GetProjection(fovPort, nearClip, farClip, true).ToMatrix4x4();
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action RecenteredPose;

	public void RecenterPose()
	{
		OVRManager.capiHmd.RecenterPose();
		if (this.RecenteredPose != null)
		{
			this.RecenteredPose();
		}
	}

	public Vector3 acceleration
	{
		get
		{
			return OVRManager.capiHmd.GetTrackingState(0.0).HeadPose.LinearAcceleration.ToVector3(true);
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			return OVRManager.capiHmd.GetTrackingState(0.0).HeadPose.AngularVelocity.ToVector3(true);
		}
	}

	public OVRDisplay.EyeRenderDesc GetEyeRenderDesc(OVREye eye)
	{
		return this.eyeDescs[(int)eye];
	}

	public RenderTexture GetEyeTexture(OVREye eye)
	{
		return this.eyeTextures[(int)(this.currEyeTextureIdx + eye)];
	}

	public int GetEyeTextureId(OVREye eye)
	{
		return this.eyeTextureIds[(int)(this.currEyeTextureIdx + eye)];
	}

	public bool isDirectMode
	{
		get
		{
			uint hmdCaps = OVRManager.capiHmd.GetDesc().HmdCaps;
			uint num = hmdCaps & 8U;
			return num == 0U;
		}
	}

	public bool mirrorMode
	{
		get
		{
			uint enabledCaps = OVRManager.capiHmd.GetEnabledCaps();
			return (enabledCaps & 8192U) == 0U;
		}
		set
		{
			uint num = OVRManager.capiHmd.GetEnabledCaps();
			if ((num & 8192U) == 0U == value)
			{
				return;
			}
			if (value)
			{
				num &= 4294959103U;
			}
			else
			{
				num |= 8192U;
			}
			OVRManager.capiHmd.SetEnabledCaps(num);
		}
	}

	internal bool timeWarp
	{
		get
		{
			return (this.distortionCaps & 2U) != 0U;
		}
		set
		{
			if (value != this.timeWarp)
			{
				this.distortionCaps ^= 2U;
			}
		}
	}

	internal bool flipInput
	{
		get
		{
			return (this.distortionCaps & 32U) != 0U;
		}
		set
		{
			if (value != this.flipInput)
			{
				this.distortionCaps ^= 32U;
			}
		}
	}

	public uint distortionCaps
	{
		get
		{
			return this._distortionCaps;
		}
		set
		{
			if (value == this._distortionCaps)
			{
				return;
			}
			this._distortionCaps = value;
			OVRDisplay.OVR_SetDistortionCaps(value);
		}
	}

	public OVRDisplay.LatencyData latency
	{
		get
		{
			float[] array = new float[3];
			float[] floatArray = OVRManager.capiHmd.GetFloatArray("DK2Latency", array);
			return new OVRDisplay.LatencyData
			{
				render = floatArray[0],
				timeWarp = floatArray[1],
				postPresent = floatArray[2]
			};
		}
	}

	private void UpdateViewport()
	{
		this.needsSetViewport = this.needsSetViewport || Screen.width != this.prevScreenWidth || Screen.height != this.prevScreenHeight;
		if (this.needsSetViewport)
		{
			this.SetViewport(0, 0, Screen.width, Screen.height);
			this.prevScreenWidth = Screen.width;
			this.prevScreenHeight = Screen.height;
			this.needsSetViewport = false;
		}
	}

	private void UpdateTextures()
	{
		for (int i = 0; i < 2; i++)
		{
			if (!this.eyeTextures[i].IsCreated())
			{
				this.eyeTextures[i].Create();
				this.eyeTextureIds[i] = this.eyeTextures[i].GetNativeTextureID();
				this.needsSetTexture = true;
			}
		}
		this.needsSetTexture = this.needsSetTexture || OVRManager.instance.virtualTextureScale != this.prevVirtualTextureScale || Screen.fullScreen != this.prevFullScreen || OVRDisplay.OVR_UnityGetModeChange();
		if (this.needsSetTexture)
		{
			for (int j = 0; j < 2; j++)
			{
				if (this.eyeTextures[j].GetNativeTexturePtr() == IntPtr.Zero)
				{
					return;
				}
				OVRDisplay.OVR_SetTexture(j, this.eyeTextures[j].GetNativeTexturePtr(), OVRManager.instance.virtualTextureScale);
			}
			this.prevVirtualTextureScale = OVRManager.instance.virtualTextureScale;
			this.prevFullScreen = Screen.fullScreen;
			OVRDisplay.OVR_UnitySetModeChange(false);
			this.needsSetTexture = false;
		}
	}

	private void ConfigureEyeDesc(OVREye eye)
	{
		FovPort fovPort = OVRManager.capiHmd.GetDesc().DefaultEyeFov[(int)eye];
		fovPort.LeftTan = (fovPort.RightTan = Mathf.Max(fovPort.LeftTan, fovPort.RightTan));
		fovPort.UpTan = (fovPort.DownTan = Mathf.Max(fovPort.UpTan, fovPort.DownTan));
		float num = 1f;
		Sizei fovTextureSize = OVRManager.capiHmd.GetFovTextureSize((Eye)eye, fovPort, num);
		float num2 = 114.59156f * Mathf.Atan(fovPort.LeftTan);
		float num3 = 114.59156f * Mathf.Atan(fovPort.UpTan);
		this.eyeDescs[(int)eye] = new OVRDisplay.EyeRenderDesc
		{
			resolution = fovTextureSize.ToVector2(),
			fov = new Vector2(num2, num3)
		};
	}

	private void ConfigureEyeTexture(int eyeBufferIndex, OVREye eye, float scale)
	{
		int num = (int)(eyeBufferIndex + eye);
		OVRDisplay.EyeRenderDesc eyeRenderDesc = this.eyeDescs[(int)eye];
		int num2 = (int)(eyeRenderDesc.resolution.x * scale);
		int num3 = (int)(eyeRenderDesc.resolution.y * scale);
		this.eyeTextures[num] = new RenderTexture(num2, num3, OVRManager.instance.eyeTextureDepth, OVRManager.instance.eyeTextureFormat);
		this.eyeTextures[num].antiAliasing = ((QualitySettings.antiAliasing != 0) ? QualitySettings.antiAliasing : 1);
		this.eyeTextures[num].Create();
		this.eyeTextureIds[num] = this.eyeTextures[num].GetNativeTextureID();
	}

	public void ForceSymmetricProj(bool enabled)
	{
		OVRDisplay.OVR_ForceSymmetricProj(enabled);
	}

	public void SetViewport(int x, int y, int w, int h)
	{
		OVRDisplay.OVR_SetViewport(x, y, w, h);
	}

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void OVR_SetDistortionCaps(uint distortionCaps);

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool OVR_SetViewport(int x, int y, int w, int h);

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern Posef OVR_GetRenderPose(int frameIndex, int eyeId);

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool OVR_SetTexture(int id, IntPtr texture, float scale = 1f);

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool OVR_UnityGetModeChange();

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool OVR_UnitySetModeChange(bool isChanged);

	[DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
	private static extern void OVR_ForceSymmetricProj(bool isEnabled);

	private int prevScreenWidth;

	private int prevScreenHeight;

	private bool needsSetTexture;

	private float prevVirtualTextureScale;

	private bool prevFullScreen;

	private OVRPose[] eyePoses = new OVRPose[2];

	private OVRDisplay.EyeRenderDesc[] eyeDescs = new OVRDisplay.EyeRenderDesc[2];

	private RenderTexture[] eyeTextures = new RenderTexture[2];

	private int[] eyeTextureIds = new int[2];

	private int currEyeTextureIdx;

	private static int frameCount;

	private bool needsSetViewport;

	private const int eyeTextureCount = 2;

	private uint _distortionCaps = 201U;

	private const string LibOVR = "OculusPlugin";

	public struct EyeRenderDesc
	{
		public Vector2 resolution;

		public Vector2 fov;
	}

	public struct LatencyData
	{
		public float render;

		public float timeWarp;

		public float postPresent;
	}
}
