using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetLocationsOnGlobalMap : ActivityStateControlled
{
	public bool Inited
	{
		get
		{
			return this._wasInit;
		}
	}

	public void SelectActiveLocation()
	{
		if (this.ToggleGroup.ActiveToggles().Count<Toggle>() != 0)
		{
			EventSystem.current.SetSelectedGameObject(this.ToggleGroup.ActiveToggles().First<Toggle>().gameObject);
		}
		this.ShowLastLocation();
	}

	private IEnumerator FillBoatLocations(IEnumerable<LocationBrief> locations)
	{
		foreach (LocationBrief availableLocation in locations)
		{
			this._locationPositions.Add(this._locations[availableLocation.LocationId], GameObject.Find(availableLocation.Asset).transform.position);
			yield return null;
		}
		this.ShowBoatLocations();
		this.ShowLastLocation();
		yield break;
	}

	public void SetupLocations(IEnumerable<LocationBrief> locations)
	{
		this._locations.Clear();
		this._locationPositions.Clear();
		LocationPin locationPin = null;
		int num = 0;
		foreach (LocationBrief locationBrief in locations)
		{
			LocationPin locationPin2 = this.CreateButton(locationBrief.LocationId, (float)locationBrief.MapX, (float)locationBrief.MapY, locationBrief, num++);
			this._locations.Add(locationBrief.LocationId, locationPin2);
			locationPin2.Toggle.isOn = false;
			Navigation navigation = locationPin2.Toggle.navigation;
			navigation.selectOnDown = null;
			navigation.selectOnRight = null;
			navigation.selectOnLeft = null;
			navigation.selectOnUp = null;
			navigation.mode = 0;
			locationPin2.Toggle.navigation = navigation;
			if (locationPin == null || (PhotonConnectionFactory.Instance.Profile.PersistentData != null && PhotonConnectionFactory.Instance.Profile.PersistentData.LastPinId == locationBrief.LocationId))
			{
				locationPin = locationPin2;
			}
		}
		locationPin.Toggle.isOn = true;
		if (this.locationsNavigation != null)
		{
			this.locationsNavigation.ForceUpdate();
		}
		base.StartCoroutine(this.FillBoatLocations(locations));
		this.SelectActiveLocation();
	}

	protected virtual LocationPin CreateButton(int nextId, float x, float y, LocationBrief locationBrief, int index)
	{
		LocationPin locationPin;
		if (index < this.LocationPinsPool.Length)
		{
			locationPin = this.LocationPinsPool[index];
			locationPin.transform.SetParent(this.MapTexture.transform);
			locationPin.gameObject.SetActive(true);
		}
		else
		{
			locationPin = Object.Instantiate<LocationPin>(this.ButtonLocationPrefab, this.MapTexture.transform, false);
		}
		locationPin.name = "btnLocation" + nextId;
		(locationPin.transform as RectTransform).anchoredPosition = this.GetAbsolutePosition(x, y);
		locationPin.DoubleClick.ActionCalled += this.DoubleClick;
		locationPin.Toggle.group = this.ToggleGroup;
		locationPin.ElementId.SetElementId(locationBrief.Asset, null, null);
		locationPin.RLDA.TransferToLocation = TransferToLocation.Instance;
		locationPin.RLDA.CurrentLocationBrief = locationBrief;
		return locationPin;
	}

	protected Vector3 GetAbsolutePosition(float x, float y)
	{
		float height = this.MapTexture.rectTransform.rect.height;
		float width = this.MapTexture.rectTransform.rect.width;
		return new Vector3(x * width - width / 2f, y * height - height / 2f, 0f);
	}

	internal void Awake()
	{
		if (this.MapTexture != null)
		{
			this.MapTextureLoadable.Image = this.MapTexture;
			this.MapTextureLoadable.Load(string.Format("Textures/Inventory/{0}", StaticUserData.CurrentPond.MapBID));
			this.MapTextureLoadable.OnLoaded.RemoveAllListeners();
			this.MapTextureLoadable.OnLoaded.AddListener(delegate
			{
				this.MapTexture.color = Color.white;
				this.MapTextureLoadable.OnLoaded.RemoveAllListeners();
			});
		}
	}

	private void OnGotAvailableLocations(IEnumerable<LocationBrief> locations)
	{
		this.subscribed = false;
		PhotonConnectionFactory.Instance.OnGotAvailableLocations -= this.OnGotAvailableLocations;
		if (!this._wasInit)
		{
			this.SetupLocations(locations);
		}
		this._wasInit = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this.subscribed)
		{
			this.subscribed = false;
			PhotonConnectionFactory.Instance.OnGotAvailableLocations -= this.OnGotAvailableLocations;
		}
	}

	protected override void SetHelp()
	{
		if (!this._wasInit)
		{
			if (!this.subscribed)
			{
				this.subscribed = true;
				if (GameFactory.PondLocationsInfo == null)
				{
					PhotonConnectionFactory.Instance.OnGotAvailableLocations += this.OnGotAvailableLocations;
				}
			}
			if (GameFactory.PondLocationsInfo == null)
			{
				PhotonConnectionFactory.Instance.GetAvailableLocations(new int?(StaticUserData.CurrentPond.PondId));
			}
			else
			{
				this.OnGotAvailableLocations(GameFactory.PondLocationsInfo);
			}
		}
		else if (this._locations.Count > 0)
		{
			foreach (KeyValuePair<int, LocationPin> keyValuePair in this._locations)
			{
				keyValuePair.Value.SetType(LocationPin.ViewType.Usual);
			}
			this.ShowBoatLocations();
			this.ShowLastLocation();
			LocationPin locationPin = null;
			if (StaticUserData.CurrentLocation != null && this._locations.TryGetValue(StaticUserData.CurrentLocation.LocationId, out locationPin))
			{
				LocationPin locationPin2 = this._locations.Values.FirstOrDefault((LocationPin t) => t.Toggle.isOn);
				if (locationPin2 != null)
				{
					locationPin2.Toggle.isOn = false;
				}
				locationPin.Toggle.isOn = true;
			}
			else if (!this._locations.Values.Any((LocationPin t) => t.Toggle.isOn))
			{
				this._locations.First<KeyValuePair<int, LocationPin>>().Value.Toggle.isOn = true;
			}
		}
	}

	private void ShowLastLocation()
	{
		if (PhotonConnectionFactory.Instance == null || PhotonConnectionFactory.Instance.Profile == null)
		{
			return;
		}
		PersistentData persistentData = PhotonConnectionFactory.Instance.Profile.PersistentData;
		int num = -1;
		if (persistentData != null)
		{
			num = persistentData.LastPinId;
		}
		else if (StaticUserData.CurrentLocation != null)
		{
			num = StaticUserData.CurrentLocation.LocationId;
		}
		if (num > 0)
		{
			LocationPin locationPin = this._locations[num];
			locationPin.SetType((locationPin.CurrentType != LocationPin.ViewType.Boat) ? LocationPin.ViewType.Restored : LocationPin.ViewType.RestoredBoat);
		}
	}

	private void ShowBoatLocations()
	{
		if (this._locationPositions.Count == 0)
		{
			return;
		}
		if (GameFactory.BoatDock == null)
		{
			return;
		}
		this.ShowDockForType(GameFactory.BoatDock.GetAnyAvailableBoatType());
	}

	private void ShowDockForType(ItemSubTypes boatType)
	{
		if (boatType == ItemSubTypes.All)
		{
			return;
		}
		Vector3 position = GameFactory.BoatDock.GetSpawnSettingsForType(boatType).spawnPos.position;
		float num = float.MaxValue;
		LocationPin locationPin = null;
		foreach (KeyValuePair<LocationPin, Vector3> keyValuePair in this._locationPositions)
		{
			float num2 = Vector3.SqrMagnitude(keyValuePair.Value - position);
			if (num2 < num)
			{
				num = num2;
				locationPin = keyValuePair.Key;
			}
		}
		locationPin.SetType(LocationPin.ViewType.Boat);
	}

	private void DoubleClick(object sender, EventArgs e)
	{
		TransferToLocation.Instance.DoubleClickTransfer();
	}

	public Image MapTexture;

	private ResourcesHelpers.AsyncLoadableImage MapTextureLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public LocationPin ButtonLocationPrefab;

	public ToggleGroup ToggleGroup;

	[SerializeField]
	private UINavigation locationsNavigation;

	[SerializeField]
	private LocationPin[] LocationPinsPool;

	private bool _wasInit;

	private Dictionary<int, LocationPin> _locations = new Dictionary<int, LocationPin>();

	private Dictionary<LocationPin, Vector3> _locationPositions = new Dictionary<LocationPin, Vector3>();

	private bool subscribed;
}
