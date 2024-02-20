using System;
using System.Collections;
using System.Diagnostics;
using mset;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class SkyBlender : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ChangedSky;

	private void Start()
	{
		this._manager = SkyManager.Get();
		this._lightPos = (this._oldLightPos = this.GameLight.transform.localPosition);
		this._lightRotation = (this._oldLightRotation = this.GameLight.transform.localRotation);
		this._oldLightIntensity = (this._curLightIntensity = this.GameLight.GetComponent<Light>().intensity);
		this._curLightColor = (this._oldLightColor = this.GameLight.GetComponent<Light>().color);
		Transform transform = base.transform.parent.Find("3DSky");
		if (transform != null)
		{
			this.GameLight.transform.parent.rotation = transform.transform.rotation;
		}
	}

	private void Update()
	{
		if (this._isLightTransition)
		{
			if ((Time.time - this._transStamp < this._blendingTime && Time.time - this._transStamp >= 0f) || Math.Abs(this._blendingTime) < 1E-05f)
			{
				Vector3 oldLightPos = this._oldLightPos;
				Vector3 lightPos = this._lightPos;
				Quaternion oldLightRotation = this._oldLightRotation;
				Quaternion lightRotation = this._lightRotation;
				float num = 1f;
				if (Math.Abs(this._blendingTime) > 0.001f)
				{
					num = (Time.time - this._transStamp) / this._blendingTime;
				}
				this.GameLight.transform.localPosition = Vector3.Lerp(oldLightPos, lightPos, num);
				this.GameLight.transform.localRotation = Quaternion.Lerp(oldLightRotation, lightRotation, num);
				this.GameLight.GetComponent<Light>().intensity = Mathf.Lerp(this._oldLightIntensity, this._curLightIntensity, num);
				this.GameLight.GetComponent<Light>().color = Vector4.Lerp(this._oldLightColor, this._curLightColor, num);
				if (this.waterBase && this.waterBase.sharedMaterial)
				{
					this.waterBase.sharedMaterial.SetColor("_SpecularColor", Vector4.Lerp(this._oldLightColor * this._oldLightMultiplier, this._curLightColor * this._curLightMultiplier, num));
				}
				if (Math.Abs(this._blendingTime) < 0.001f)
				{
					this._isLightTransition = false;
				}
				TimeAndWeatherManager.SetNightMode(this.GameLight.GetComponent<Light>().intensity);
			}
			else
			{
				this._isLightTransition = false;
			}
		}
	}

	public void ChangeSky(Sky newSky, Light light)
	{
		switch (SettingsManager.RenderQuality)
		{
		case RenderQualities.Fastest:
			this._blendingTime = 2f;
			break;
		case RenderQualities.Fast:
			this._blendingTime = 2.857143f;
			break;
		case RenderQualities.Simple:
			this._blendingTime = 4f;
			break;
		case RenderQualities.Good:
			this._blendingTime = 6.6666665f;
			break;
		case RenderQualities.Beautiful:
			this._blendingTime = 10f;
			break;
		case RenderQualities.Fantastic:
			this._blendingTime = 20f;
			break;
		case RenderQualities.Ultra:
			this._blendingTime = 20f;
			break;
		}
		this.SetParams(newSky, light);
	}

	public void ChangeSky(Sky newSky, Light light, float blendTime)
	{
		this._blendingTime = blendTime;
		this.SetParams(newSky, light);
	}

	private void SetParams(Sky newSky, Light light)
	{
		if (GameFactory.Player != null)
		{
			GameFactory.Player.CameraController.Camera.GetComponent<SunShafts>().sunTransform = this.GameLight.transform;
		}
		this._oldLightPos = this._lightPos;
		this._oldLightRotation = this._lightRotation;
		this._lightPos = light.transform.localPosition * 5f;
		this._lightRotation = light.transform.localRotation;
		this._oldLight = this._curLight;
		this._curLight = light;
		this._oldLightMultiplier = this._curLightMultiplier;
		this._oldLightIntensity = this._curLightIntensity;
		this._curLightIntensity = light.intensity;
		this._oldLightColor = this._curLightColor;
		this._curLightColor = light.color;
		this._isLightTransition = true;
		this._transStamp = Time.time;
		this.GameLight.GetComponent<Light>().shadows = this._curLight.shadows;
		this.GameLight.GetComponent<Light>().shadowStrength = this._curLight.shadowStrength;
		this._curLightMultiplier = Mathf.Clamp01((this._curLight.shadowStrength - 0.75f) / 0.25f);
		if (this.GameLight.GetComponent<Light>().shadowStrength > 0.75f)
		{
			Shader.SetGlobalFloat("UWIntensityRT", Mathf.Clamp01((this.GameLight.GetComponent<Light>().shadowStrength - 0.75f) / 0.25f));
		}
		else
		{
			Shader.SetGlobalFloat("UWIntensityRT", 0f);
		}
		this._manager.BlendToGlobalSky(newSky, this._blendingTime);
		base.StartCoroutine(this.DestroyOldSky(newSky));
	}

	private IEnumerator DestroyOldSky(Sky newSky)
	{
		yield return new WaitForSeconds(this._blendingTime + 1f);
		if (this._currentSky != null)
		{
			Object.DestroyImmediate(this._currentSky.gameObject);
		}
		if (this._oldLight != null)
		{
			Object.DestroyImmediate(this._oldLight.gameObject);
		}
		this._currentSky = newSky;
		if (this.ChangedSky != null)
		{
			this.ChangedSky(this, new EventArgs());
		}
		yield break;
	}

	private Sky _currentSky;

	private float _blendingTime;

	private const float BlendTime = 20f;

	private Light _curLight;

	private Light _oldLight;

	private float _curLightIntensity;

	private float _oldLightIntensity;

	private float _curLightMultiplier;

	private float _oldLightMultiplier;

	private Color _curLightColor;

	private Color _oldLightColor;

	private Vector3 _oldLightPos = Vector3.zero;

	private Vector3 _lightPos = Vector3.zero;

	private Quaternion _oldLightRotation;

	private Quaternion _lightRotation;

	private bool _isLightTransition;

	private float _transStamp;

	public GameObject GameLight;

	public FishWaterBase waterBase;

	private SkyManager _manager;
}
