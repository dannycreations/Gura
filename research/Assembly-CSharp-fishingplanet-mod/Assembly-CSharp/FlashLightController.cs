using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlashLightController : MonoBehaviour
{
	public bool HandLightEnabled
	{
		get
		{
			return this._handLightEnabled;
		}
	}

	protected virtual void Awake()
	{
		this._light = base.GetComponent<Light>();
		this._flare = base.GetComponent<LensFlare>();
	}

	protected virtual void Start()
	{
		IPhotonServerConnection instance = PhotonConnectionFactory.Instance;
		if (instance != null && instance.Profile != null)
		{
			PhotonConnectionFactory.Instance.OnItemGained += this.OnItemGained;
			this.TryToInitLight();
		}
		this.EnableLight(false);
	}

	private void OnItemGained(IEnumerable<InventoryItem> items, bool announce)
	{
		if (!this._isLightInitialized)
		{
			if (items.Any((InventoryItem item) => item.ItemSubType == ItemSubTypes.Hat))
			{
				this.TryToInitLight();
			}
		}
	}

	private void TryToInitLight()
	{
		IPhotonServerConnection instance = PhotonConnectionFactory.Instance;
		Hat hat = instance.Profile.Inventory.Items.FirstOrDefault((InventoryItem item) => item.Storage == StoragePlaces.Doll && item.ItemSubType == ItemSubTypes.Hat) as Hat;
		bool flag = hat != null && hat.Flashlight != null;
		if (flag || this._isForceLight)
		{
			instance.OnGotTime += this.OnUpdateTime;
			if (flag)
			{
				this._isLightInitialized = true;
				Flashlight flashlight = hat.Flashlight;
				this._light.type = 0;
				this._light.range = 50f;
				this._light.spotAngle = 60f;
				this._light.color = Color.white;
				this._light.intensity = 4f;
				this._light.shadows = 0;
				this._light.renderMode = 2;
				this._light.cookie = Resources.Load<Texture2D>("Textures/LightCookies/FlashlightCookie");
			}
		}
	}

	public static bool IsDarkTime
	{
		get
		{
			DateTime dateTime = TimeAndWeatherManager.CurrentInGameTime();
			return (dateTime.Hour >= 21 && dateTime.Hour <= 24) || (dateTime.Hour >= 0 && dateTime.Hour <= 4);
		}
	}

	protected virtual void OnUpdateTime(TimeSpan time)
	{
		if (this._handActivated || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		bool isDarkTime = FlashLightController.IsDarkTime;
		if (isDarkTime && !this._light.enabled)
		{
			this.EnableLight(true);
		}
		else if (!isDarkTime && this._light.enabled)
		{
			this.EnableLight(false);
		}
	}

	private void Update()
	{
		if (this._isLightInitialized && ControlsController.ControlsActions.FlashlightVisibility.WasPressed)
		{
			this._handActivated = true;
			this._handLightEnabled = !this._handLightEnabled;
			this.EnableLight(this._handLightEnabled);
		}
	}

	protected virtual void OnDestroy()
	{
		if (PhotonConnectionFactory.Instance != null)
		{
			PhotonConnectionFactory.Instance.OnGotTime -= this.OnUpdateTime;
			PhotonConnectionFactory.Instance.OnItemGained -= this.OnItemGained;
		}
	}

	public void PushLightDisabling()
	{
		base.gameObject.SetActive(false);
		if (this._light.enabled)
		{
			this._wasDisabled = true;
			this.EnableLight(false);
		}
	}

	public void PopLightDisabling()
	{
		base.gameObject.SetActive(true);
		if (this._wasDisabled)
		{
			this._wasDisabled = false;
			this.EnableLight(true);
		}
	}

	protected void EnableLight(bool enable)
	{
		this._light.enabled = enable;
		if (base.transform.childCount > 0)
		{
			base.transform.GetChild(0).gameObject.SetActive(enable);
		}
		if (this._flare != null)
		{
			this._flare.enabled = enable;
		}
	}

	[SerializeField]
	protected bool _isForceLight;

	private bool _handActivated;

	private bool _handLightEnabled;

	private bool _isLightInitialized;

	protected Light _light;

	protected LensFlare _flare;

	private bool _wasDisabled;
}
