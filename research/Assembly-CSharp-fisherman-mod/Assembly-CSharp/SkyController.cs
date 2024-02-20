using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using mset;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class SkyController : MonoBehaviour, ISkyController
{
	public bool IsFirstSkyInited()
	{
		return this._isFirstSkyInited;
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ChangedSky;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> UnnecessaryChangeSky;

	private void Awake()
	{
		GameFactory.SkyControllerInstance = this;
		base.GetComponent<global::SkyBlender>().ChangedSky += this.SkyController_ChangedSky;
		this._sky3D = GameObject.Find("3DSky");
	}

	private GameObject Sky3D
	{
		get
		{
			if (this._sky3D == null)
			{
				this._sky3D = GameObject.Find("3DSky");
			}
			return this._sky3D;
		}
	}

	public SkyInfo CurrentSky
	{
		get
		{
			return this._currentSky;
		}
		set
		{
			this._currentSky = value;
		}
	}

	public GameObject CurrentSkyInstance { get; set; }

	public IEnumerator SetFirstSky()
	{
		Debug.Log("Try set first sky!");
		while (TimeAndWeatherManager.CurrentWeather == null || TimeAndWeatherManager.CurrentTime == null)
		{
			yield return new WaitForSeconds(1f);
		}
		GameFactory.Player.CameraController.Camera.GetComponent<SunShafts>().sunTransform = base.GetComponent<global::SkyBlender>().GameLight.transform;
		this._requestedBundle = true;
		if (TimeAndWeatherManager.CurrentWeather == null)
		{
			this._requestedBundle = false;
			yield break;
		}
		SkyInfo sky = this.GetSky();
		Object skyAsset = null;
		Object lightAsset = null;
		AssetBundle assetBundle = null;
		this._isChangingProcess = true;
		FishWaterTile.DoingRenderWater = false;
		yield return base.StartCoroutine(AssetBundleManager.LoadSkyFromBundle(sky.AssetBundleName, sky.SkyPrefabName, sky.LightPrefabName, delegate(Object assetSky, Object assetLight, AssetBundle bundle)
		{
			skyAsset = assetSky;
			lightAsset = assetLight;
			assetBundle = bundle;
		}));
		FishWaterTile.DoingRenderWater = true;
		this._assetBundleName = sky.AssetBundleName;
		this._assets = assetBundle;
		this.ChangeSkyLoad(skyAsset, lightAsset, new float?(0f));
		this._currentSky = sky;
		this._requestedBundle = false;
		base.Invoke("ChangedSkyEvent", 0.5f);
		this._isFirstSkyInited = true;
		yield break;
	}

	private void ChangedSkyEvent()
	{
		if (this.ChangedSky != null)
		{
			this.ChangedSky(this, new EventArgs());
		}
	}

	private void Update()
	{
		if (!this._isFirstSkyInited)
		{
			return;
		}
		this._connectionTimer += Time.deltaTime;
		if (!this._isChangingProcess && (this._connectionTimer >= 25f || this._isForce))
		{
			this._connectionTimer = 0f;
			if (TimeAndWeatherManager.CurrentWeather == null)
			{
				return;
			}
			try
			{
				if (GameFactory.Player != null)
				{
					SkyInfo sky = this.GetSky();
					if (sky != null)
					{
						if (this._currentSky == null || sky.TypeName != this._currentSky.TypeName)
						{
							base.StartCoroutine(this.ChangeSky(sky));
						}
						else if (this.UnnecessaryChangeSky != null)
						{
							this.UnnecessaryChangeSky(this, new EventArgs());
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	private IEnumerator ChangeSky(SkyInfo sky)
	{
		this._isChangingProcess = true;
		Object skyAsset = null;
		Object lightAsset = null;
		AssetBundle assetBundle = null;
		this._assetBundleName = sky.AssetBundleName;
		if (this._assets != null)
		{
			this._previouslyAssets = this._assets;
		}
		yield return base.StartCoroutine(AssetBundleManager.LoadSkyFromBundle(sky.AssetBundleName, sky.SkyPrefabName, sky.LightPrefabName, delegate(Object assetSky, Object assetLight, AssetBundle bundle)
		{
			skyAsset = assetSky;
			lightAsset = assetLight;
			assetBundle = bundle;
			this._assets = assetBundle;
			if (!this._isForce)
			{
				this.ChangeSkyLoad(skyAsset, lightAsset, null);
			}
			else
			{
				this._isForce = false;
				this.ChangeSkyLoad(skyAsset, lightAsset, new float?(0f));
			}
			this._currentSky = sky;
		}));
		yield break;
	}

	private SkyInfo GetSky()
	{
		if (TimeAndWeatherManager.CurrentTime == null)
		{
			return null;
		}
		TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
		DateTime timeTemp = new DateTime(2000, 1, 1, 0, 0, 0);
		timeTemp = timeTemp.Add(value);
		SkyWeather skyWeather = SkyWeather.Clear;
		string sky = TimeAndWeatherManager.CurrentWeather.Sky;
		switch (sky)
		{
		case "Clear":
			skyWeather = SkyWeather.Clear;
			break;
		case "Cloudy":
			skyWeather = SkyWeather.PartlyCloudy;
			break;
		case "HeavyCloudy":
			skyWeather = SkyWeather.Cloudy;
			break;
		case "Unclear":
			skyWeather = SkyWeather.Rainy;
			break;
		case "ClearMoonNight":
			skyWeather = SkyWeather.Clear;
			break;
		case "ClearHalfMoonNight":
			skyWeather = SkyWeather.ClearHalfMoon;
			break;
		case "ClearNight":
			skyWeather = SkyWeather.PartlyCloudy;
			break;
		case "CloudyNight":
			skyWeather = SkyWeather.PartlyCloudy;
			break;
		case "OvercastNight":
			skyWeather = SkyWeather.PartlyCloudy;
			break;
		case "Event":
			skyWeather = SkyWeather.Event;
			break;
		}
		return SkyList.Instance().FirstOrDefault((SkyInfo x) => x.WeatherType == skyWeather && x.StartTime <= timeTemp.Hour && x.EndTime >= timeTemp.Hour);
	}

	private void ChangeSkyLoad(Object sky, Object light, float? blendTime = null)
	{
		DebugUtility.Sky.Trace("ChangeSky", new object[0]);
		if (sky == null)
		{
			this._isChangingProcess = false;
			return;
		}
		GameObject sky3D = this.Sky3D;
		GameObject gameObject = Object.Instantiate(sky) as GameObject;
		GameObject gameObject2 = Object.Instantiate(light) as GameObject;
		this._currentLightInstance = gameObject2;
		this.ChangeAdditionalParams();
		if (sky3D != null)
		{
			gameObject.transform.rotation = sky3D.transform.rotation;
		}
		if (blendTime == null)
		{
			base.GetComponent<global::SkyBlender>().ChangeSky(gameObject.GetComponent<Sky>(), gameObject2.GetComponent<Light>());
		}
		else
		{
			base.GetComponent<global::SkyBlender>().ChangeSky(gameObject.GetComponent<Sky>(), gameObject2.GetComponent<Light>(), blendTime.Value);
		}
		this._isChangingProcess = false;
		this.CurrentSkyInstance = gameObject;
	}

	private void SkyController_ChangedSky(object sender, EventArgs e)
	{
		try
		{
			if (this._previouslyAssets != null)
			{
				this._previouslyAssets.Unload(true);
			}
		}
		finally
		{
		}
		this.ChangedSkyEvent();
	}

	public void ChangeAdditionalParams()
	{
		GameObject currentLightInstance = this._currentLightInstance;
		if (currentLightInstance.GetComponent<Light>().shadows != null && currentLightInstance.GetComponent<Light>().shadowStrength > 0.75f)
		{
			GameFactory.Player.CameraController.Camera.GetComponent<SunShafts>().enabled = true;
		}
		else
		{
			GameFactory.Player.CameraController.Camera.GetComponent<SunShafts>().enabled = false;
		}
	}

	private void OnDestroy()
	{
		try
		{
			if (this._assets != null)
			{
				this._assets.Unload(false);
			}
			if (this._previouslyAssets != null)
			{
				this._previouslyAssets.Unload(true);
			}
		}
		finally
		{
		}
	}

	public void ForceChangeSky()
	{
		this._isForce = true;
	}

	private float _connectionTimer;

	public const float ConnectionTimerMax = 25f;

	private SkyInfo _currentSky = new SkyInfo();

	private AssetBundle _assets;

	private AssetBundle _previouslyAssets;

	private string _assetBundleName;

	private bool _requestedBundle;

	private bool _isChangingProcess;

	private GameObject _currentLightInstance;

	private bool _isForce;

	private bool _isFirstSkyInited;

	private GameObject _sky3D;
}
