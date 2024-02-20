using System;
using UnityEngine;

[ExecuteInEditMode]
public class FogVolume : MonoBehaviour
{
	public float GetVisibility()
	{
		return this.Visibility;
	}

	private void CreateMaterial()
	{
		this.FogMaterial = new Material(Shader.Find("Hidden/FogVolume"));
		this.FogMaterial.name = "Fog Material";
		this.FogMaterial.hideFlags = 61;
	}

	private void SetCameraDepth()
	{
		if (this.ForwardRenderingCamera)
		{
			this.ForwardRenderingCamera.depthTextureMode |= 1;
		}
	}

	private void OnEnable()
	{
		this.CreateMaterial();
		this.SetCameraDepth();
		this.FogVolumeGameObject = base.gameObject;
		this.FogVolumeGameObject.GetComponent<Renderer>().sharedMaterial = this.FogMaterial;
		this.ToggleKeyword();
	}

	public static void Wireframe(GameObject obj, bool Enable)
	{
	}

	private void Update()
	{
	}

	private void OnWillRenderObject()
	{
		this.FogMaterial.SetColor("_Color", this.FogColor);
		this.FogMaterial.SetColor("_InscatteringColor", this.InscatteringColor);
		this.FogMaterial.SetVector("_ScaleFog", base.transform.localScale);
		if (this.Sun)
		{
			this.FogMaterial.SetFloat("_InscatteringIntensity", this.InscatteringIntensity);
			this.FogMaterial.SetVector("L", -this.Sun.transform.forward);
			this.FogMaterial.SetFloat("InscatteringShape", this.InscatteringShape);
			this.FogMaterial.SetFloat("InscatteringTransitionWideness", this.InscatteringTransitionWideness);
		}
		if (this.EnableNoise && this._NoiseVolume)
		{
			Shader.SetGlobalTexture("_NoiseVolume", this._NoiseVolume);
			this.FogMaterial.SetFloat("gain", this.NoiseIntensity);
			this.FogMaterial.SetFloat("threshold", this.NoiseContrast * 0.5f);
			this.FogMaterial.SetFloat("_3DNoiseScale", this._3DNoiseScale * 0.001f);
			this.FogMaterial.SetFloat("_3DNoiseStepSize", this._3DNoiseStepSize * 0.001f);
			this.FogMaterial.SetVector("Speed", this.Speed);
			this.FogMaterial.SetVector("Stretch", new Vector4(1f, 1f, 1f, 1f) + this.Stretch * 0.01f);
		}
		if (this.Gradient != null)
		{
			this.FogMaterial.SetTexture("_Gradient", this.Gradient);
		}
		this.FogMaterial.SetFloat("InscatteringStartDistance", this.InscatteringStartDistance);
		Vector3 localScale = this.FogVolumeGameObject.transform.localScale;
		base.transform.localScale = new Vector3((float)decimal.Round((decimal)localScale.x, 2), localScale.y, localScale.z);
		this.FogMaterial.SetVector("_BoxMin", localScale * -0.5f);
		this.FogMaterial.SetVector("_BoxMax", localScale * 0.5f);
		this.FogMaterial.SetFloat("_Visibility", this.Visibility);
		base.GetComponent<Renderer>().sortingOrder = this.DrawOrder;
	}

	private void ToggleKeyword()
	{
		if (this.FogMaterial)
		{
			if (this.EnableNoise && SystemInfo.supports3DTextures)
			{
				this.FogMaterial.EnableKeyword("_FOG_VOLUME_NOISE");
			}
			else
			{
				this.FogMaterial.DisableKeyword("_FOG_VOLUME_NOISE");
			}
			if (this.EnableInscattering && this.Sun)
			{
				this.FogMaterial.EnableKeyword("_FOG_VOLUME_INSCATTERING");
			}
			else
			{
				this.FogMaterial.DisableKeyword("_FOG_VOLUME_INSCATTERING");
			}
			if (this.EnableGradient && this.Gradient != null)
			{
				this.FogMaterial.EnableKeyword("_FOG_GRADIENT");
			}
			else
			{
				this.FogMaterial.DisableKeyword("_FOG_GRADIENT");
			}
			if (this.FastFog)
			{
				this.FogMaterial.EnableKeyword("_FOG_FAST");
			}
			else
			{
				this.FogMaterial.DisableKeyword("_FOG_FAST");
			}
		}
	}

	private GameObject FogVolumeGameObject;

	public Camera ForwardRenderingCamera;

	[HideInInspector]
	public Material FogMaterial;

	[SerializeField]
	private Color InscatteringColor = Color.white;

	[SerializeField]
	private Color FogColor = new Color(0.5f, 0.6f, 0.7f, 1f);

	public float Visibility = 5f;

	[Range(-1f, 1f)]
	public float InscatteringShape;

	public float InscatteringIntensity = 2f;

	public float InscatteringStartDistance = 400f;

	public float InscatteringTransitionWideness = 1f;

	public float _3DNoiseScale = 300f;

	public float _3DNoiseStepSize = 50f;

	public Texture3D _NoiseVolume;

	[Range(0f, 10f)]
	public float NoiseIntensity = 1f;

	[Range(0f, 1f)]
	public float NoiseContrast;

	[SerializeField]
	private Light Sun;

	[SerializeField]
	private int DrawOrder;

	public Texture2D Gradient;

	[SerializeField]
	private bool EnableGradient;

	[SerializeField]
	private bool EnableInscattering;

	[SerializeField]
	private bool EnableNoise;

	[SerializeField]
	private bool FastFog;

	public Vector4 Speed = new Vector4(0f, 0f, 0f, 0f);

	public Vector4 Stretch = new Vector4(0f, 0f, 0f, 0f);
}
