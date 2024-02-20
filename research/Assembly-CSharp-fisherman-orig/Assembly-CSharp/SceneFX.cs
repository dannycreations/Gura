using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Wind/Add Soft Occlusion Wind Effect")]
public class SceneFX : MonoBehaviour
{
	public Vector3 WindVelocity
	{
		get
		{
			return this.windVelocity;
		}
	}

	private void Awake()
	{
		this.taUpdateWind();
		Shader.SetGlobalFloat("_LeafCounter", this.leafCounter);
		Shader.SetGlobalTexture("_CausticTex", this.CausticTexture);
	}

	private void Start()
	{
		Shader.SetGlobalTexture("_CausticTex", this.CausticTexture);
		this.mat1 = default(Matrix4x4);
		this.mat1.SetColumn(0, new Vector4(100f, 0f, 0f, 0f));
		this.mat1.SetColumn(1, new Vector4(0f, 1f, 0f, 0f));
		this.mat1.SetColumn(2, new Vector4(0f, 0f, 0f, 0f));
		this.mat1.SetColumn(3, new Vector4(0f, 0f, 1f, 1f));
		this.causticFrames = this.LoadFrames("Caustic/" + this.causticName);
		this.NextFrame();
		base.InvokeRepeating("NextFrame", 1f / this.fps, 1f / this.fps);
		this.SetupBillboardSettings();
	}

	private void OnDestroy()
	{
		this.BillboardTerrains = null;
	}

	private void SetupBillboardSettings()
	{
		if (this.BillboardTerrains == null)
		{
			return;
		}
		int num = this.BillboardTerrains.Length;
		if (num == 0)
		{
			return;
		}
		if (Application.isPlaying)
		{
			this._currentQuality = SettingsManager.RenderQuality;
		}
		float num2 = this.BillboardDistanceMin;
		switch (this._currentQuality)
		{
		case RenderQualities.Fastest:
			num2 = this.BillboardDistanceMin;
			break;
		case RenderQualities.Fast:
			num2 = (this.BillboardDistanceMax - this.BillboardDistanceMin) / 5f + this.BillboardDistanceMin;
			break;
		case RenderQualities.Simple:
			num2 = (this.BillboardDistanceMax - this.BillboardDistanceMin) / 5f * 2f + this.BillboardDistanceMin;
			break;
		case RenderQualities.Good:
			num2 = (this.BillboardDistanceMax - this.BillboardDistanceMin) / 5f * 3f + this.BillboardDistanceMin;
			break;
		case RenderQualities.Beautiful:
			num2 = (this.BillboardDistanceMax - this.BillboardDistanceMin) / 5f * 4f + this.BillboardDistanceMin;
			break;
		case RenderQualities.Fantastic:
			num2 = this.BillboardDistanceMax;
			break;
		case RenderQualities.Ultra:
			num2 = this.BillboardDistanceMax;
			break;
		}
		for (int i = 0; i < num; i++)
		{
			if (this.BillboardTerrains[i] == null)
			{
				LogHelper.Error("[GD] SceneFX:SetupBillboardSettings - empty Terrain object in [BillboardTerrains] index:{0} scene:{1}", new object[]
				{
					i,
					base.gameObject.scene.name
				});
			}
			else
			{
				this.BillboardTerrains[i].treeBillboardDistance = num2;
			}
		}
	}

	public void Update()
	{
		this.taUpdateWind();
		if (Application.isPlaying && this._currentQuality != SettingsManager.RenderQuality)
		{
			this.SetupBillboardSettings();
		}
	}

	private void taUpdateWind()
	{
		float num = (1.25f + Mathf.Sin(Time.time * this.WindFrequency) * Mathf.Sin(Time.time * 0.375f)) * 0.5f;
		if (TimeAndWeatherManager.CurrentWeather != null)
		{
			this.windVelocity = Quaternion.AngleAxis(this.WindAngle + 180f, Vector3.up) * new Vector3(0f, 0f, TimeAndWeatherManager.CurrentWeather.WindSpeed * num * this.WindPhysicsFactor);
		}
		this.TempWind = this.Wind;
		this.TempWindForce = this.Wind.w;
		this.TempWind.x = this.TempWind.x * num;
		this.TempWind.z = this.TempWind.z * ((1.25f + Mathf.Sin(Time.time * this.WindFrequency) * Mathf.Sin(Time.time * 0.193f)) * 0.5f);
		this.TempWind.w = this.TempWindForce;
		Shader.SetGlobalVector("_Wind", this.TempWind);
		Shader.SetGlobalFloat("_CustomMult", (this.TempWind.x + this.TempWind.z) * 10f);
		this.lightProjMatrix = Matrix4x4.identity;
		float num2 = 0.7853982f;
		num2 = Mathf.Cos(num2) / Mathf.Sin(num2);
		this.lightProjMatrix[3, 2] = 2f / num2;
		float num3 = 0.01f;
		Vector3 vector;
		vector..ctor(num3, num3, num3);
		this.lightRangeMatrix = Matrix4x4.Scale(vector);
		Shader.SetGlobalMatrix("_CustomMat", this.lightProjMatrix * this.lightRangeMatrix * base.transform.worldToLocalMatrix);
		this.windMatrix.x = Mathf.Sin(this.WindAngle / 57.29578f);
		this.windMatrix.y = Mathf.Cos(this.WindAngle / 57.29578f);
		Shader.SetGlobalVector("_WindAngleMatrix", this.windMatrix);
		Shader.SetGlobalVector("_SquashPlaneNormal", this.TempWind);
	}

	private Texture2D[] LoadFrames(string texName)
	{
		return Resources.LoadAll<Texture2D>(texName);
	}

	private void NextFrame()
	{
		foreach (Light light in this.causticLights)
		{
			light.cookie = this.causticFrames[this.frameIndex];
		}
		this.frameIndex = (this.frameIndex + 1) % this.causticFrames.Length;
	}

	public Vector4 Wind = new Vector4(0.85f, 0.075f, 0.4f, 0.5f);

	public float WindFrequency = 0.75f;

	public float NormalTilt = 5f;

	public float WaveSizeFoliageShader = 10f;

	public float WindMultiplierForGrassshader = 4f;

	public float WaveSizeForGrassshader = 10f;

	public Texture2D CausticTexture;

	public Texture2D[] causticFrames;

	public Light[] causticLights;

	public string causticName = "1-5m";

	public float WindAngle;

	[Space(15f)]
	public float WindPhysicsFactor = 0.5f;

	public Terrain[] BillboardTerrains;

	public float BillboardDistanceMax = 90f;

	public float BillboardDistanceMin = 45f;

	private Vector4 TempWind;

	private float TempWindForce;

	private float GrassWind;

	private Vector3 lightDir;

	private Vector3 templightDir;

	private float CameraAngle;

	private Terrain[] allTerrains;

	private float grey;

	private float leafCounter;

	private Matrix4x4 mat1;

	private Matrix4x4 lightProjMatrix;

	private Matrix4x4 lightRangeMatrix;

	[Space(15f)]
	public float fps = 30f;

	private int frameIndex;

	private Vector2 windMatrix;

	private Vector2 windMatrixOffesets;

	private RenderQualities _currentQuality;

	private float windGustPhase;

	private Vector3 windVelocity;
}
