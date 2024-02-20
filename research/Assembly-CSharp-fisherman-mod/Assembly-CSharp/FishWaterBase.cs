using System;
using UnityEngine;

[ExecuteInEditMode]
public class FishWaterBase : MonoBehaviour
{
	public Color Base
	{
		get
		{
			return this._base0;
		}
	}

	public Color Abyss
	{
		get
		{
			return this._abyss0;
		}
	}

	private Color BaseStart { get; set; }

	private Color AbyssStart { get; set; }

	private Color BaseEnd { get; set; }

	private Color AbyssEnd { get; set; }

	private void Awake()
	{
		this.InitSharedMaterialColors();
	}

	public void SetColor(Color baseCol, Color abyssCol, float time)
	{
		this.BaseStart = this.sharedMaterial.GetColor("_BaseColor");
		this.AbyssStart = this.sharedMaterial.GetColor("_AbyssColor");
		this.BaseEnd = baseCol;
		this.AbyssEnd = abyssCol;
		this._duration = time;
		this._startTime = Time.deltaTime;
	}

	public void UpdateShader()
	{
		if (this.waterQuality > FishWaterQuality.Medium)
		{
			this.sharedMaterial.shader.maximumLOD = 501;
		}
		else if (this.waterQuality > FishWaterQuality.Low)
		{
			this.sharedMaterial.shader.maximumLOD = 301;
		}
		else
		{
			this.sharedMaterial.shader.maximumLOD = 201;
		}
		if (!SystemInfo.SupportsRenderTextureFormat(1))
		{
			this.edgeBlend = false;
		}
		FishDynWaterQuality fishDynWaterQuality = this.dynWaterQuality;
		if (fishDynWaterQuality != FishDynWaterQuality.High)
		{
			if (fishDynWaterQuality != FishDynWaterQuality.Low)
			{
				if (fishDynWaterQuality == FishDynWaterQuality.None)
				{
					Shader.EnableKeyword("DYN_WATER_NONE");
					Shader.DisableKeyword("DYN_WATER_LOW");
					Shader.DisableKeyword("DYN_WATER_HIGH");
				}
			}
			else
			{
				Shader.EnableKeyword("DYN_WATER_LOW");
				Shader.DisableKeyword("DYN_WATER_HIGH");
				Shader.DisableKeyword("DYN_WATER_NONE");
			}
		}
		else
		{
			Shader.EnableKeyword("DYN_WATER_HIGH");
			Shader.DisableKeyword("DYN_WATER_LOW");
			Shader.DisableKeyword("DYN_WATER_NONE");
		}
		if (this.edgeBlend)
		{
			Shader.EnableKeyword("WATER_EDGEBLEND_ON");
			Shader.DisableKeyword("WATER_EDGEBLEND_OFF");
			if (Camera.main)
			{
				Camera.main.depthTextureMode |= 1;
			}
		}
		else
		{
			Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
			Shader.DisableKeyword("WATER_EDGEBLEND_ON");
		}
	}

	public void WaterTileBeingRendered(Transform tr, Camera currentCam)
	{
		if (currentCam && this.edgeBlend)
		{
			currentCam.depthTextureMode |= 1;
		}
	}

	public void Update()
	{
		this.shaderSkipCounter++;
		if (this.shaderSkipCounter > 20)
		{
			this.shaderSkipCounter = 0;
			if (this.sharedMaterial)
			{
				this.UpdateShader();
			}
		}
		this.UpdateShaderColors();
	}

	private void UpdateShaderColors()
	{
		if (this._startTime >= 0f)
		{
			float num = Mathf.Clamp(Mathf.Abs(Time.deltaTime - this._startTime) / this._duration, 0f, 1f);
			this._startTime += Time.deltaTime;
			this.sharedMaterial.SetColor("_BaseColor", Color.Lerp(this.BaseStart, this.BaseEnd, num));
			this.sharedMaterial.SetColor("_AbyssColor", Color.Lerp(this.AbyssStart, this.AbyssEnd, num));
			if (num >= 1f)
			{
				this._startTime = -1f;
			}
		}
	}

	private void InitSharedMaterialColors()
	{
		this._base0 = this.sharedMaterial.GetColor("_BaseColor");
		this._abyss0 = this.sharedMaterial.GetColor("_AbyssColor");
		if (StaticUserData.CurrentPond != null)
		{
			int pondId = StaticUserData.CurrentPond.PondId;
			this.GetSharedMaterialColorFromPrefs("_BaseColor", pondId, ref this._base0);
			this.GetSharedMaterialColorFromPrefs("_AbyssColor", pondId, ref this._abyss0);
		}
		this.sharedMaterial.SetColor("_BaseColor", this._base0);
		this.sharedMaterial.SetColor("_AbyssColor", this._abyss0);
	}

	private void GetSharedMaterialColorFromPrefs(string colorId, int pondId, ref Color c)
	{
		string text = string.Format("{0}_{1}", colorId, pondId);
		string text2 = PlayerPrefs.GetString(text);
		Color color;
		if (!string.IsNullOrEmpty(text2) && ColorUtility.TryParseHtmlString(string.Format("#{0}", text2), ref color))
		{
			c = color;
		}
		if (string.IsNullOrEmpty(text2))
		{
			text2 = ColorUtility.ToHtmlStringRGBA(c);
			PlayerPrefs.SetString(text, text2);
		}
	}

	public Material sharedMaterial;

	public FishWaterQuality waterQuality = FishWaterQuality.High;

	public bool edgeBlend = true;

	public FishDynWaterQuality dynWaterQuality = FishDynWaterQuality.High;

	public int shaderSkipCounter;

	private float _duration;

	private float _startTime = -1f;

	private const string BaseColorId = "_BaseColor";

	private const string AbyssColorId = "_AbyssColor";

	private Color _base0;

	private Color _abyss0;
}
