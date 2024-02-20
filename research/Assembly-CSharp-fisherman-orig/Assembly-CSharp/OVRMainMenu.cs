using System;
using UnityEngine;

public class OVRMainMenu : MonoBehaviour
{
	private void Awake()
	{
		OVRCameraRig[] componentsInChildren = base.gameObject.GetComponentsInChildren<OVRCameraRig>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("OVRMainMenu: No OVRCameraRig attached.");
		}
		else if (componentsInChildren.Length > 1)
		{
			Debug.LogWarning("OVRMainMenu: More then 1 OVRCameraRig attached.");
		}
		else
		{
			this.CameraController = componentsInChildren[0];
		}
		OVRPlayerController[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<OVRPlayerController>();
		if (componentsInChildren2.Length == 0)
		{
			Debug.LogWarning("OVRMainMenu: No OVRPlayerController attached.");
		}
		else if (componentsInChildren2.Length > 1)
		{
			Debug.LogWarning("OVRMainMenu: More then 1 OVRPlayerController attached.");
		}
		else
		{
			this.PlayerController = componentsInChildren2[0];
		}
	}

	private void Start()
	{
		this.AlphaFadeValue = 1f;
		this.CurrentLevel = 0;
		this.PrevStartDown = false;
		this.PrevHatDown = false;
		this.PrevHatUp = false;
		this.ShowVRVars = false;
		this.OldSpaceHit = false;
		this.strFPS = "FPS: 0";
		this.LoadingLevel = false;
		this.ScenesVisible = false;
		this.GUIRenderObject = Object.Instantiate(Resources.Load("OVRGUIObjectMain")) as GameObject;
		if (this.GUIRenderObject != null)
		{
			this.GUIRenderObject.layer = LayerMask.NameToLayer(this.LayerName);
			if (this.GUIRenderTexture == null)
			{
				int width = Screen.width;
				int height = Screen.height;
				this.GUIRenderTexture = new RenderTexture(width, height, 0);
				this.GuiHelper.SetPixelResolution((float)width, (float)height);
				this.GuiHelper.SetDisplayResolution(1280f, 800f);
			}
		}
		if (this.GUIRenderTexture != null && this.GUIRenderObject != null)
		{
			this.GUIRenderObject.GetComponent<Renderer>().material.mainTexture = this.GUIRenderTexture;
			if (this.CameraController != null)
			{
				Vector3 localScale = this.GUIRenderObject.transform.localScale;
				Vector3 localPosition = this.GUIRenderObject.transform.localPosition;
				Quaternion localRotation = this.GUIRenderObject.transform.localRotation;
				this.GUIRenderObject.transform.parent = this.CameraController.centerEyeAnchor;
				this.GUIRenderObject.transform.localScale = localScale;
				this.GUIRenderObject.transform.localPosition = localPosition;
				this.GUIRenderObject.transform.localRotation = localRotation;
				this.GUIRenderObject.SetActive(false);
			}
		}
		if (!Application.isEditor)
		{
			Cursor.visible = false;
			Screen.lockCursor = true;
		}
		if (this.CameraController != null)
		{
			this.GridCube = base.gameObject.AddComponent<OVRGridCube>();
			this.GridCube.SetOVRCameraController(ref this.CameraController);
			this.VisionGuide = base.gameObject.AddComponent<OVRVisionGuide>();
			this.VisionGuide.SetOVRCameraController(ref this.CameraController);
			this.VisionGuide.SetFadeTexture(ref this.FadeInTexture);
			this.VisionGuide.SetVisionGuideLayer(ref this.LayerName);
		}
		this.Crosshair.Init();
		this.Crosshair.SetCrosshairTexture(ref this.CrosshairImage);
		this.Crosshair.SetOVRCameraController(ref this.CameraController);
		this.Crosshair.SetOVRPlayerController(ref this.PlayerController);
		this.CheckIfRiftPresent();
	}

	private void Update()
	{
		if (this.LoadingLevel)
		{
			return;
		}
		this.UpdateFPS();
		if (this.CameraController != null)
		{
			this.UpdateIPD();
			this.UpdateRecenterPose();
			this.UpdateVisionMode();
			this.UpdateFOV();
			this.UpdateEyeHeightOffset();
			this.UpdateResolutionEyeTexture();
			this.UpdateLatencyValues();
		}
		if (this.PlayerController != null)
		{
			this.UpdateSpeedAndRotationScaleMultiplier();
			this.UpdatePlayerControllerMovement();
		}
		this.UpdateSelectCurrentLevel();
		this.UpdateDeviceDetection();
		this.Crosshair.UpdateCrosshair();
		if (Input.GetKeyDown(292))
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
		if (Input.GetKeyDown(109))
		{
			OVRManager.display.mirrorMode = !OVRManager.display.mirrorMode;
		}
		if (Input.GetKeyDown(this.QuitKey))
		{
			Application.Quit();
		}
	}

	private void OnGUI()
	{
		if (Event.current.type != 7)
		{
			return;
		}
		if (this.AlphaFadeValue > 0f)
		{
			this.AlphaFadeValue -= Mathf.Clamp01(Time.deltaTime / this.FadeInTime);
			if (this.AlphaFadeValue >= 0f)
			{
				GUI.color = new Color(0f, 0f, 0f, this.AlphaFadeValue);
				GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.FadeInTexture);
				return;
			}
			this.AlphaFadeValue = 0f;
		}
		if (this.GUIRenderObject != null)
		{
			if (this.ScenesVisible || this.ShowVRVars || this.Crosshair.IsCrosshairVisible() || this.RiftPresentTimeout > 0f || this.VisionGuide.GetFadeAlphaValue() > 0f)
			{
				this.GUIRenderObject.SetActive(true);
			}
			else
			{
				this.GUIRenderObject.SetActive(false);
			}
		}
		Vector3 one = Vector3.one;
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, one);
		RenderTexture active = RenderTexture.active;
		if (this.GUIRenderTexture != null && this.GUIRenderObject.activeSelf)
		{
			RenderTexture.active = this.GUIRenderTexture;
			GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
		}
		this.GuiHelper.SetFontReplace(this.FontReplace);
		if (!this.GUIShowRiftDetected())
		{
			this.GUIShowLevels();
			this.GUIShowVRVariables();
		}
		this.Crosshair.OnGUICrosshair();
		this.VisionGuide.OnGUIVisionGuide();
		if (this.GUIRenderObject.activeSelf)
		{
			RenderTexture.active = active;
		}
		GUI.matrix = matrix;
	}

	private void UpdateFPS()
	{
		this.TimeLeft -= Time.deltaTime;
		this.Accum += Time.timeScale / Time.deltaTime;
		this.Frames++;
		if ((double)this.TimeLeft <= 0.0)
		{
			float num = this.Accum / (float)this.Frames;
			if (this.ShowVRVars)
			{
				this.strFPS = string.Format("FPS: {0:F2}", num);
			}
			this.TimeLeft += this.UpdateInterval;
			this.Accum = 0f;
			this.Frames = 0;
		}
	}

	private void UpdateIPD()
	{
		if (this.ShowVRVars)
		{
			this.strIPD = string.Format("IPD (mm): {0:F4}", OVRManager.profile.ipd * 1000f);
		}
	}

	private void UpdateRecenterPose()
	{
		if (Input.GetKeyDown(114))
		{
			OVRManager.display.RecenterPose();
		}
	}

	private void UpdateVisionMode()
	{
		if (Input.GetKeyDown(283))
		{
			this.VisionMode = !this.VisionMode;
			OVRManager.tracker.isEnabled = this.VisionMode;
		}
	}

	private void UpdateFOV()
	{
		if (this.ShowVRVars)
		{
			this.strFOV = string.Format("FOV (deg): {0:F3}", OVRManager.display.GetEyeRenderDesc(OVREye.Left).fov.y);
		}
	}

	private void UpdateResolutionEyeTexture()
	{
		if (this.ShowVRVars)
		{
			OVRDisplay.EyeRenderDesc eyeRenderDesc = OVRManager.display.GetEyeRenderDesc(OVREye.Left);
			OVRDisplay.EyeRenderDesc eyeRenderDesc2 = OVRManager.display.GetEyeRenderDesc(OVREye.Right);
			float num = OVRManager.instance.nativeTextureScale * OVRManager.instance.virtualTextureScale;
			float num2 = (float)((int)(num * (eyeRenderDesc.resolution.x + eyeRenderDesc2.resolution.x)));
			float num3 = (float)((int)(num * Mathf.Max(eyeRenderDesc.resolution.y, eyeRenderDesc2.resolution.y)));
			this.strResolutionEyeTexture = string.Format("Resolution : {0} x {1}", num2, num3);
		}
	}

	private void UpdateLatencyValues()
	{
		if (this.ShowVRVars)
		{
			OVRDisplay.LatencyData latency = OVRManager.display.latency;
			if (latency.render < 1E-06f && latency.timeWarp < 1E-06f && latency.postPresent < 1E-06f)
			{
				this.strLatencies = string.Format("Ren : N/A TWrp: N/A PostPresent: N/A", new object[0]);
			}
			else
			{
				this.strLatencies = string.Format("Ren : {0:F3} TWrp: {1:F3} PostPresent: {2:F3}", latency.render, latency.timeWarp, latency.postPresent);
			}
		}
	}

	private void UpdateEyeHeightOffset()
	{
		if (this.ShowVRVars)
		{
			float eyeHeight = OVRManager.profile.eyeHeight;
			this.strHeight = string.Format("Eye Height (m): {0:F3}", eyeHeight);
		}
	}

	private void UpdateSpeedAndRotationScaleMultiplier()
	{
		float num = 0f;
		this.PlayerController.GetMoveScaleMultiplier(ref num);
		if (Input.GetKeyDown(55))
		{
			num -= this.SpeedRotationIncrement;
		}
		else if (Input.GetKeyDown(56))
		{
			num += this.SpeedRotationIncrement;
		}
		this.PlayerController.SetMoveScaleMultiplier(num);
		float num2 = 0f;
		this.PlayerController.GetRotationScaleMultiplier(ref num2);
		if (Input.GetKeyDown(57))
		{
			num2 -= this.SpeedRotationIncrement;
		}
		else if (Input.GetKeyDown(48))
		{
			num2 += this.SpeedRotationIncrement;
		}
		this.PlayerController.SetRotationScaleMultiplier(num2);
		if (this.ShowVRVars)
		{
			this.strSpeedRotationMultipler = string.Format("Spd.X: {0:F2} Rot.X: {1:F2}", num, num2);
		}
	}

	private void UpdatePlayerControllerMovement()
	{
		if (this.PlayerController != null)
		{
			this.PlayerController.SetHaltUpdateMovement(this.ScenesVisible);
		}
	}

	private void UpdateSelectCurrentLevel()
	{
		this.ShowLevels();
		if (!this.ScenesVisible)
		{
			return;
		}
		this.CurrentLevel = this.GetCurrentLevel();
		if (this.Scenes.Length != 0 && (OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.A) || Input.GetKeyDown(13)))
		{
			this.LoadingLevel = true;
			Application.LoadLevelAsync(this.Scenes[this.CurrentLevel]);
		}
	}

	private bool ShowLevels()
	{
		if (this.Scenes.Length == 0)
		{
			this.ScenesVisible = false;
			return this.ScenesVisible;
		}
		bool flag = OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Start);
		bool flag2 = (flag && !this.PrevStartDown) || Input.GetKeyDown(303);
		this.PrevStartDown = flag;
		if (flag2)
		{
			this.ScenesVisible = !this.ScenesVisible;
		}
		return this.ScenesVisible;
	}

	private int GetCurrentLevel()
	{
		bool flag = false;
		if (OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Down))
		{
			flag = true;
		}
		bool flag2 = false;
		if (OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Down))
		{
			flag2 = true;
		}
		if ((!this.PrevHatDown && flag) || Input.GetKeyDown(274))
		{
			this.CurrentLevel = (this.CurrentLevel + 1) % this.SceneNames.Length;
		}
		else if ((!this.PrevHatUp && flag2) || Input.GetKeyDown(273))
		{
			this.CurrentLevel--;
			if (this.CurrentLevel < 0)
			{
				this.CurrentLevel = this.SceneNames.Length - 1;
			}
		}
		this.PrevHatDown = flag;
		this.PrevHatUp = flag2;
		return this.CurrentLevel;
	}

	private void GUIShowLevels()
	{
		if (this.ScenesVisible)
		{
			GUI.color = new Color(0f, 0f, 0f, 0.5f);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.FadeInTexture);
			GUI.color = Color.white;
			if (this.LoadingLevel)
			{
				string text = "LOADING...";
				this.GuiHelper.StereoBox(this.StartX, this.StartY, this.WidthX, this.WidthY, ref text, Color.yellow);
				return;
			}
			for (int i = 0; i < this.SceneNames.Length; i++)
			{
				Color color;
				if (i == this.CurrentLevel)
				{
					color = Color.yellow;
				}
				else
				{
					color = Color.black;
				}
				int num = this.StartY + i * this.StepY;
				this.GuiHelper.StereoBox(this.StartX, num, this.WidthX, this.WidthY, ref this.SceneNames[i], color);
			}
		}
	}

	private void GUIShowVRVariables()
	{
		bool key = Input.GetKey(this.MenuKey);
		if (!this.OldSpaceHit && key)
		{
			if (this.ShowVRVars)
			{
				this.ShowVRVars = false;
			}
			else
			{
				this.ShowVRVars = true;
			}
		}
		this.OldSpaceHit = key;
		if (!this.ShowVRVars)
		{
			return;
		}
		int num = this.VRVarsSY;
		this.GuiHelper.StereoBox(this.VRVarsSX, num += this.StepY, this.VRVarsWidthX, this.VRVarsWidthY, ref this.strFPS, Color.green);
		if (this.CameraController != null)
		{
			this.GuiHelper.StereoBox(this.VRVarsSX, num += this.StepY, this.VRVarsWidthX, this.VRVarsWidthY, ref this.strPrediction, Color.white);
			this.GuiHelper.StereoBox(this.VRVarsSX, num += this.StepY, this.VRVarsWidthX, this.VRVarsWidthY, ref this.strIPD, Color.yellow);
			this.GuiHelper.StereoBox(this.VRVarsSX, num += this.StepY, this.VRVarsWidthX, this.VRVarsWidthY, ref this.strFOV, Color.white);
			this.GuiHelper.StereoBox(this.VRVarsSX, num += this.StepY, this.VRVarsWidthX, this.VRVarsWidthY, ref this.strResolutionEyeTexture, Color.white);
			this.GuiHelper.StereoBox(this.VRVarsSX, num += this.StepY, this.VRVarsWidthX, this.VRVarsWidthY, ref this.strLatencies, Color.white);
		}
		if (this.PlayerController != null)
		{
			this.GuiHelper.StereoBox(this.VRVarsSX, num += this.StepY, this.VRVarsWidthX, this.VRVarsWidthY, ref this.strHeight, Color.yellow);
			this.GuiHelper.StereoBox(this.VRVarsSX, num + this.StepY, this.VRVarsWidthX, this.VRVarsWidthY, ref this.strSpeedRotationMultipler, Color.white);
		}
	}

	private void CheckIfRiftPresent()
	{
		this.HMDPresent = OVRManager.display.isPresent;
		if (!this.HMDPresent)
		{
			this.RiftPresentTimeout = 15f;
			if (!this.HMDPresent)
			{
				this.strRiftPresent = "NO HMD DETECTED";
			}
		}
	}

	private bool GUIShowRiftDetected()
	{
		if (this.RiftPresentTimeout > 0f)
		{
			this.GuiHelper.StereoBox(this.StartX, this.StartY, this.WidthX, this.WidthY, ref this.strRiftPresent, Color.white);
			return true;
		}
		return false;
	}

	private void UpdateDeviceDetection()
	{
		if (this.RiftPresentTimeout > 0f)
		{
			this.RiftPresentTimeout -= Time.deltaTime;
		}
	}

	private void ShowRiftPresentGUI()
	{
	}

	public float FadeInTime = 2f;

	public Texture FadeInTexture;

	public Font FontReplace;

	public KeyCode MenuKey = 32;

	public KeyCode QuitKey = 27;

	public string[] SceneNames;

	public string[] Scenes;

	private bool ScenesVisible;

	private int StartX = 490;

	private int StartY = 250;

	private int WidthX = 300;

	private int WidthY = 23;

	private int VRVarsSX = 553;

	private int VRVarsSY = 250;

	private int VRVarsWidthX = 175;

	private int VRVarsWidthY = 23;

	private int StepY = 25;

	private OVRCameraRig CameraController;

	private OVRPlayerController PlayerController;

	private bool PrevStartDown;

	private bool PrevHatDown;

	private bool PrevHatUp;

	private bool ShowVRVars;

	private bool OldSpaceHit;

	private float UpdateInterval = 0.5f;

	private float Accum;

	private int Frames;

	private float TimeLeft;

	private string strFPS = "FPS: 0";

	public float IPDIncrement = 0.0025f;

	private string strIPD = "IPD: 0.000";

	public float PredictionIncrement = 0.001f;

	private string strPrediction = "Pred: OFF";

	public float FOVIncrement = 0.2f;

	private string strFOV = "FOV: 0.0f";

	public float HeightIncrement = 0.01f;

	private string strHeight = "Height: 0.0f";

	public float SpeedRotationIncrement = 0.05f;

	private string strSpeedRotationMultipler = "Spd. X: 0.0f Rot. X: 0.0f";

	private bool LoadingLevel;

	private float AlphaFadeValue = 1f;

	private int CurrentLevel;

	private bool HMDPresent;

	private float RiftPresentTimeout;

	private string strRiftPresent = string.Empty;

	private OVRGUI GuiHelper = new OVRGUI();

	private GameObject GUIRenderObject;

	private RenderTexture GUIRenderTexture;

	public string LayerName = "Default";

	public Texture CrosshairImage;

	private OVRCrosshair Crosshair = new OVRCrosshair();

	private string strResolutionEyeTexture = "Resolution: 0 x 0";

	private string strLatencies = "Ren: 0.0f TWrp: 0.0f PostPresent: 0.0f";

	private bool VisionMode = true;

	private OVRGridCube GridCube;

	private OVRVisionGuide VisionGuide;
}
