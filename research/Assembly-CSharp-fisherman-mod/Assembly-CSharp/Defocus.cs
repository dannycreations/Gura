using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Defocus")]
public class Defocus : MonoBehaviour
{
	private void OnEnable()
	{
		this.Shader_Defocus = Shader.Find("Hidden/Defocus");
		if (this.Shader_Defocus == null)
		{
			Debug.Log("#ERROR# Hidden/Defocus Shader not found");
		}
		this.Material_Defocus = new Material(this.Shader_Defocus);
		this.Material_Defocus.hideFlags = 61;
		this.InitRT();
	}

	private void Update()
	{
	}

	private void Start()
	{
		this.Set();
	}

	private void InitRT()
	{
		int num = Mathf.NextPowerOfTwo(Camera.main.pixelWidth / 4);
		int num2 = Mathf.NextPowerOfTwo(Camera.main.pixelHeight / 4);
		this.RT_wetTexture = new RenderTexture(num, num2, 0, 0);
		this.RT_wetTexture.wrapMode = 1;
		this.RT_wetTexture.filterMode = 1;
		this.RT_wetTexture.name = "SceneColor";
		this.RT_wetTexture.isPowerOfTwo = true;
		this.RT_wetTexture.hideFlags = 61;
		Graphics.SetRenderTarget(this.RT_wetTexture);
		GL.Clear(false, true, Color.clear);
	}

	private void Set()
	{
		if (base.GetComponent<Camera>().depthTextureMode == null)
		{
			base.GetComponent<Camera>().depthTextureMode = 1;
		}
	}

	private void OnDisable()
	{
		GlobalConsts.isScreenRendered = false;
		if (this.RT_wetTexture)
		{
			Object.Destroy(this.RT_wetTexture);
			this.RT_wetTexture = null;
		}
		if (this.isRainActive)
		{
			this.isRainActive = false;
			this.RainDrops.SetActive(false);
		}
		if (this.isDropsActive)
		{
			this.isDropsActive = false;
			this.FishDrops.SetActive(false);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, this.RT_wetTexture);
		this.m_Wetness.SetTexture("_R", this.RT_wetTexture);
		this.m_Wetness.SetTexture("_G", this.RT_wetTexture);
		this.m_Wetness.SetTexture("_B", this.RT_wetTexture);
		GlobalConsts.isScreenRendered = true;
		this.Material_Defocus.SetTexture("_MainTex", source);
		if (GlobalConsts.LensWetnessRainActive && !this.isRainActive)
		{
			this.isRainActive = true;
			this.RainDrops.SetActive(true);
		}
		if (GlobalConsts.LensWetnessFishActive && !this.isDropsActive)
		{
			this.isDropsActive = true;
			this.FishDrops.SetActive(true);
		}
		if (!GlobalConsts.LensWetnessRainActive)
		{
			this.isRainActive = false;
		}
		if (!GlobalConsts.LensWetnessFishActive)
		{
			this.isDropsActive = false;
		}
		this.ScreenX = source.width;
		this.ScreenY = source.height;
		int num = this.ScreenX;
		int num2 = this.ScreenY;
		this.Material_Defocus.SetFloat("_EdgeSpread", this._EdgeSpread);
		this.Material_Defocus.SetFloat("_Blur", this._Blur);
		this.Material_Defocus.SetVector("_Offset", new Vector4(1f / (float)this.ScreenX, 1f / (float)this.ScreenY, 0f, 0f));
		num /= this.Downsampling;
		num2 /= this.Downsampling;
		RenderTexture renderTexture = RenderTexture.GetTemporary(num, num2, 0, source.format);
		Graphics.Blit(source, renderTexture, this.Material_Defocus, 0);
		for (int i = 1; i <= this.Iterations; i++)
		{
			float num3 = (this._Blur + (float)(i / this.Iterations)) / (float)this.ScreenX;
			float num4 = (this._Blur + (float)(i / this.Iterations)) / (float)this.ScreenY;
			RenderTexture renderTexture2 = RenderTexture.GetTemporary(num, num2, 0, source.format);
			this.Material_Defocus.SetVector("_Offset", new Vector4(0f, num4 * this._Blur, 0f, 0f));
			Graphics.Blit(renderTexture, renderTexture2, this.Material_Defocus, 0);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = renderTexture2;
			renderTexture2 = RenderTexture.GetTemporary(num, num2, 0, source.format);
			this.Material_Defocus.SetVector("_Offset", new Vector4(num3 * this._Blur, 0f, 0f, 0f));
			Graphics.Blit(renderTexture, renderTexture2, this.Material_Defocus, 0);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = renderTexture2;
		}
		RenderTexture.ReleaseTemporary(renderTexture);
		this.Material_Defocus.SetTexture("_BlurTex", renderTexture);
		RenderTexture renderTexture3 = RenderTexture.GetTemporary(num, num2, 0, 14);
		Graphics.Blit(null, renderTexture3, this.Material_Defocus, 2);
		for (int j = 1; j <= this.Iterations; j++)
		{
			float num5 = (this._EdgeSpread + (float)(j / this.Iterations)) / (float)this.ScreenX;
			float num6 = (this._EdgeSpread + (float)(j / this.Iterations)) / (float)this.ScreenY;
			RenderTexture renderTexture4 = RenderTexture.GetTemporary(num, num2, 0, 14);
			this.Material_Defocus.SetVector("_Offset", new Vector4(0f, num6, 0f, 0f));
			Graphics.Blit(renderTexture3, renderTexture4, this.Material_Defocus, 0);
			RenderTexture.ReleaseTemporary(renderTexture3);
			renderTexture3 = renderTexture4;
			renderTexture4 = RenderTexture.GetTemporary(num, num2, 0, 14);
			this.Material_Defocus.SetVector("_Offset", new Vector4(num5, 0f, 0f, 0f));
			Graphics.Blit(renderTexture3, renderTexture4, this.Material_Defocus, 0);
			RenderTexture.ReleaseTemporary(renderTexture3);
			renderTexture3 = renderTexture4;
		}
		RenderTexture.ReleaseTemporary(renderTexture3);
		this.Material_Defocus.SetTexture("_zBlurred", renderTexture3);
		Graphics.Blit(source, destination, this.Material_Defocus, 1);
	}

	public RenderTexture RT_wetTexture;

	public Material m_Wetness;

	public GameObject FishDrops;

	public GameObject RainDrops;

	private Shader Shader_Defocus;

	private Material Material_Defocus;

	private int ScreenX = 1280;

	private int ScreenY = 720;

	private bool isRainActive;

	private bool isDropsActive;

	[SerializeField]
	[Range(1f, 6f)]
	private int Iterations = 4;

	[SerializeField]
	[Range(1f, 6f)]
	private int Downsampling = 4;

	[SerializeField]
	[Range(0f, 5f)]
	private float _EdgeSpread = 1f;

	[SerializeField]
	[Range(0f, 2f)]
	private float _Blur = 1f;

	private enum Pass
	{
		Gaussian,
		Compose,
		GetZ
	}
}
