using System;
using System.Collections;
using System.Linq;
using mset;
using UnityEngine;

public class TestSkyController : MonoBehaviour
{
	public GameObject CurrentSkyInstance { get; set; }

	private IEnumerator LoadAssetBundle(string assetBundleName)
	{
		AssetBundle result = null;
		if (this._assets != null)
		{
			this._assets.Unload(false);
		}
		yield return base.StartCoroutine(AssetBundleManager.LoadAssetBundle(assetBundleName, delegate(AssetBundle value)
		{
			result = value;
		}));
		this._assets = result;
		this._assetBundleName = assetBundleName;
		this._requestedBundle = false;
		yield break;
	}

	private void Update()
	{
		if (ControlsController.ControlsActions.NextHours.WasPressedMandatory)
		{
			this._currentSkyIndex++;
			if (this._currentSkyIndex > 4)
			{
				this._currentSkyIndex = 0;
			}
			this._isForce = true;
			this.ChangeSky();
		}
		this._connectionTimer += Time.deltaTime;
		if (this._isForce && this._connectionTimer >= 1f && !this._isChanging)
		{
			this._connectionTimer = 0f;
			this.ChangeSky();
		}
	}

	private void ChangeCaustics()
	{
	}

	private void ChangeSky()
	{
		SkyInfo sky = this.GetSky();
		if (sky == null)
		{
			return;
		}
		if (sky.TypeName != this._currentSky.TypeName)
		{
			if ((this._assets == null || this._assetBundleName != sky.AssetBundleName) && sky.AssetBundleName != this._assetBundleName)
			{
				if (this._requestedBundle)
				{
					return;
				}
				this._requestedBundle = true;
				base.StartCoroutine(this.LoadAssetBundle(sky.AssetBundleName));
				return;
			}
			else
			{
				if (this._assets == null)
				{
					return;
				}
				this._isChanging = true;
				Object @object = this._assets.LoadAsset(sky.SkyPrefabName);
				Object object2 = this._assets.LoadAsset(sky.LightPrefabName);
				if (!this._isForce)
				{
					this.ChangeSkyLoad(@object, object2, null);
				}
				else
				{
					this._isForce = false;
					this.ChangeSkyLoad(@object, object2, new float?(0f));
				}
				Debug.Log("Sky changed to: " + sky.SkyPrefabName);
				this._currentSky = sky;
			}
		}
	}

	private SkyInfo GetSky()
	{
		SkyWeather skyWeather = SkyWeather.Clear;
		DateTime timeTemp = new DateTime(2000, 1, 1, 0, 0, 0);
		switch (this._currentSkyIndex)
		{
		case 0:
			timeTemp = timeTemp.AddHours(4.0);
			break;
		case 1:
			timeTemp = timeTemp.AddHours(8.0);
			break;
		case 2:
			timeTemp = timeTemp.AddHours(12.0);
			break;
		case 3:
			timeTemp = timeTemp.AddHours(16.0);
			break;
		case 4:
			timeTemp = timeTemp.AddHours(20.0);
			break;
		}
		return SkyList.Instance().FirstOrDefault((SkyInfo x) => x.WeatherType == skyWeather && x.StartTime <= timeTemp.Hour && x.EndTime >= timeTemp.Hour);
	}

	private void ChangeSkyLoad(Object sky, Object light, float? blendTime = null)
	{
		if (this._assets == null)
		{
			return;
		}
		GameObject gameObject = GameObject.Find("3DSky");
		GameObject gameObject2 = Object.Instantiate(sky) as GameObject;
		GameObject gameObject3 = Object.Instantiate(light) as GameObject;
		this._currentLightInstance = gameObject3;
		this.ChangeAdditionalParams();
		if (gameObject != null)
		{
			gameObject2.transform.rotation = gameObject.transform.rotation;
		}
		if (blendTime == null)
		{
			base.GetComponent<global::SkyBlender>().ChangeSky(gameObject2.GetComponent<Sky>(), gameObject3.GetComponent<Light>());
		}
		else
		{
			base.GetComponent<global::SkyBlender>().ChangeSky(gameObject2.GetComponent<Sky>(), gameObject3.GetComponent<Light>(), blendTime.Value);
		}
		this.CurrentSkyInstance = gameObject2;
		this._isChanging = false;
	}

	public void ChangeAdditionalParams()
	{
	}

	private void OnDestroy()
	{
		if (this._assets != null)
		{
			this._assets.Unload(false);
		}
	}

	public void ForceChangeSky()
	{
		this._isForce = true;
	}

	private float _connectionTimer = 1f;

	public const float ConnectionTimerMax = 1f;

	private SkyInfo _currentSky = new SkyInfo();

	private AssetBundle _assets;

	private string _assetBundleName;

	private bool _requestedBundle;

	private GameObject _currentLightInstance;

	private bool _isForce;

	private bool _isFirstSkyInited;

	private int _currentSkyIndex;

	private bool _isChanging;
}
