using System;
using UnityEngine;

[ExecuteInEditMode]
public class GrabTexture : MonoBehaviour
{
	private void Start()
	{
	}

	private void CameraSetup()
	{
		this.MainCamera = Camera.main;
		this.secondaryCamera = GameObject.Find("Secondary Camera");
		if (!this.secondaryCamera)
		{
			this.secondaryCamera = new GameObject("Secondary Camera");
			this.secondaryCamera.AddComponent<Camera>();
		}
		this.secondaryCamera.GetComponent<Camera>().renderingPath = 2;
		this.secondaryCamera.GetComponent<Camera>().allowHDR = this.MainCamera.allowHDR;
		this.secondaryCamera.GetComponent<Camera>().enabled = false;
		this.secondaryCamera.GetComponent<Camera>().nearClipPlane = this.MainCamera.nearClipPlane;
		this.secondaryCamera.GetComponent<Camera>().farClipPlane = this.MainCamera.farClipPlane;
		this.secondaryCamera.GetComponent<Camera>().fieldOfView = this.MainCamera.fieldOfView;
		this.secondaryCamera.hideFlags = 61;
	}

	private void OnEnable()
	{
		this.CameraSetup();
	}

	private void OnWillRenderObject()
	{
		this.RTT_size.x = (float)(Screen.width / this.Downsampling);
		this.RTT_size.y = (float)(Screen.height / this.Downsampling);
		if (!this._SceneColor || this.PreviousTextureSize != this.RTT_size)
		{
			if (this._SceneColor)
			{
				Object.DestroyImmediate(this._SceneColor);
			}
			this._SceneColor = new RenderTexture((int)this.RTT_size.x, (int)this.RTT_size.y, 0, this.TextureFormat);
			this._SceneColor.name = "SceneColor";
			this._SceneColor.isPowerOfTwo = true;
			this._SceneColor.hideFlags = 61;
			this.PreviousTextureSize = this.RTT_size;
		}
		if (this.MainCamera)
		{
			this.secondaryCamera.GetComponent<Camera>().clearFlags = this.MainCamera.clearFlags;
			this.secondaryCamera.GetComponent<Camera>().transform.rotation = this.MainCamera.transform.rotation;
			this.secondaryCamera.GetComponent<Camera>().transform.position = this.MainCamera.transform.position;
			this.secondaryCamera.GetComponent<Camera>().fieldOfView = this.MainCamera.fieldOfView;
			this.secondaryCamera.GetComponent<Camera>().cullingMask = this.Layer;
			this.secondaryCamera.GetComponent<Camera>().targetTexture = this._SceneColor;
			this.secondaryCamera.GetComponent<Camera>().Render();
			base.GetComponent<Renderer>().sharedMaterial.SetTexture("_R", this._SceneColor);
			base.GetComponent<Renderer>().sharedMaterial.SetTexture("_G", this._SceneColor);
			base.GetComponent<Renderer>().sharedMaterial.SetTexture("_B", this._SceneColor);
		}
	}

	private void Update()
	{
	}

	private void OnDisable()
	{
		Object.DestroyImmediate(this.secondaryCamera);
		if (this._SceneColor)
		{
			Object.DestroyImmediate(this._SceneColor);
			this._SceneColor = null;
		}
	}

	private void OnGUI()
	{
		int num = 10;
		int num2 = 100;
		if (this._SceneColor)
		{
			this.AspectRatio = this.RTT_size.x / this.RTT_size.y;
			this.Scale.x = (float)Screen.width * this.RTTSliderValue;
			this.Scale.y = (float)Screen.height * this.RTTSliderValue;
			if (this.ShowRTT)
			{
				GUI.DrawTexture(new Rect((float)num, (float)num2, this.Scale.x, this.Scale.y), this._SceneColor, 0, false, this.AspectRatio);
				this.RTTSliderValue = GUI.HorizontalSlider(new Rect(10f, 80f, 100f, 30f), this.RTTSliderValue, 0f, 0.5f);
			}
		}
	}

	private float RTTSliderValue = 0.3f;

	public bool ShowRTT;

	private float AspectRatio;

	private Vector2 Scale;

	public Camera MainCamera;

	public LayerMask Layer = -1;

	public RenderTextureFormat TextureFormat = 11;

	[SerializeField]
	[Range(1f, 8f)]
	private int Downsampling = 1;

	private Vector2 PreviousTextureSize = new Vector2(1f, 1f);

	private Vector2 RTT_size = new Vector2(128f, 128f);

	private GameObject secondaryCamera;

	public RenderTexture _SceneColor;
}
