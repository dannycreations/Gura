using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Assets.Scripts.Common.Managers.Helpers;
using DG.Tweening;
using I2.Loc;
using InControl;
using mset;
using ObjectModel;
using TPM;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InGameMap : MonoBehaviour
{
	private bool gamepad
	{
		get
		{
			return SettingsManager.InputType == InputModuleManager.InputType.GamePad;
		}
	}

	public void ChangeRotationMethod()
	{
		this.North = !this.North;
		this.rotationText[0].SetActive(this.North);
		this.rotationText[1].SetActive(!this.North);
	}

	public void AllignWithPlayer()
	{
		this.UncheckMagnetedToggle();
		this.mapRenderer.AlignWithPlayer();
	}

	public static InGameMap Instance { get; private set; }

	public static void PauseForLayersLess(int layer)
	{
		if (InGameMap.Instance != null && InGameMap.Instance.gameObject.activeInHierarchy && InGameMap.Instance._visibleLayer < layer)
		{
			InGameMap.Instance.ignoreControls = true;
		}
	}

	public static void UnpauseForLayersGreater(int layer)
	{
		if (InGameMap.Instance != null && InGameMap.Instance.gameObject.activeInHierarchy && InGameMap.Instance._visibleLayer >= layer)
		{
			InGameMap.Instance.ignoreControls = false;
		}
	}

	private void OnEnable()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		for (int i = 0; i < this.objectsToHideOnConsoles.Length; i++)
		{
			this.objectsToHideOnConsoles[i].SetActive(!this.gamepad);
		}
		this.ignoreControls = BlockableRegion.CurrentLayer != 0;
		if (GameFactory.Player.State == typeof(ShowMap) && !this.ignoreControls)
		{
			CursorManager.ShowCursor();
			InGameMap.BlockInputForMaps();
		}
		PhotonConnectionFactory.Instance.GetAvailableLocations(new int?(StaticUserData.CurrentPond.PondId));
		PhotonConnectionFactory.Instance.OnReceiveBuoyShareRequest += this.OnShareRequest;
		PhotonConnectionFactory.Instance.OnBuoyShareDeclined += this.RefreshBuoysCapacity;
		PhotonConnectionFactory.Instance.OnProductBought += this.OnProductBought;
		this.SetTexts();
		SkyManager skyManager = Object.FindObjectOfType<SkyManager>();
		if (skyManager != null)
		{
			this.north = skyManager.GlobalSky.transform.transform.eulerAngles.y - 180f;
		}
		this.AllignWithPlayer();
		this.RefreshBuoysCapacity();
		this.Crosshair.SetActive(this.gamepad);
		if (StaticUserData.IS_TPM_VISIBLE && !StaticUserData.IS_IN_TUTORIAL)
		{
			GameFactory.Player.CharCtr.EPlayerAdd += this.CharCtr_OnPlayerAdd;
			GameFactory.Player.CharCtr.EPlayerRemove += this.CharCtr_OnPlayerRemove;
			this._players = GameFactory.Player.CharCtr.GetActivePlayers();
		}
		else
		{
			this._players = new List<PlayerRecord>();
		}
		this.RebuildUsersList();
	}

	internal void OnDisable()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		if (StaticUserData.IS_TPM_VISIBLE && !StaticUserData.IS_IN_TUTORIAL)
		{
			GameFactory.Player.CharCtr.EPlayerAdd -= this.CharCtr_OnPlayerAdd;
			GameFactory.Player.CharCtr.EPlayerRemove -= this.CharCtr_OnPlayerRemove;
		}
		PhotonConnectionFactory.Instance.OnReceiveBuoyShareRequest -= this.OnShareRequest;
		PhotonConnectionFactory.Instance.OnBuoyShareDeclined -= this.RefreshBuoysCapacity;
		PhotonConnectionFactory.Instance.OnProductBought -= this.OnProductBought;
		if (GameFactory.Player.State == typeof(ShowMap) && BlockableRegion.CurrentLayer == 0)
		{
			ControlsController.ControlsActions.UnBlockInput();
		}
		if (this.mapInfoPanel.Openned)
		{
			this.mapInfoPanel.CloseInfoPanel();
		}
		if (GameFactory.Player.State == typeof(ShowMap) && BlockableRegion.CurrentLayer == 0)
		{
			CursorManager.HideCursor();
		}
	}

	private void CharCtr_OnPlayerRemove(PlayerRecord obj)
	{
		int num = this._players.FindIndex((PlayerRecord e) => e.PlayerName == obj.PlayerName);
		if (num != -1)
		{
			this._players.RemoveAt(num);
			this.RebuildUsersList();
		}
	}

	private void CharCtr_OnPlayerAdd(PlayerRecord obj)
	{
		if (this._players.FindIndex((PlayerRecord e) => e.PlayerName == obj.PlayerName) == -1)
		{
			this._players.Add(obj);
			this.RebuildUsersList();
		}
	}

	private void OnShareRequest(BuoySetting buoy)
	{
		this.RefreshBuoysCapacity();
	}

	public void RefreshBuoysCapacity()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null || profile.Inventory == null || StaticUserData.CurrentPond == null)
		{
			return;
		}
		int num = ((profile.Buoys == null) ? 0 : profile.Buoys.Count);
		this.BuoysCount.SetCapacity(num, profile.Inventory.CurrentBuoyCapacity);
		bool flag = num >= profile.Inventory.CurrentBuoyCapacity;
		this.BuyMoreBuoysButton.SetActive(flag);
		GameObject openShareRequests = this.OpenShareRequests;
		bool flag2;
		if (profile.BuoyShareRequests != null)
		{
			flag2 = profile.BuoyShareRequests.Any((BuoySetting x) => x.PondId == StaticUserData.CurrentPond.PondId);
		}
		else
		{
			flag2 = false;
		}
		openShareRequests.SetActive(flag2);
		this.AddButton.SetActive(!flag && num < profile.Inventory.CurrentBuoyCapacity);
		this.AddButtonHelp.SetActive(this.gamepad && !flag && num < profile.Inventory.CurrentBuoyCapacity);
		if (this.AddButtonHelp.activeInHierarchy)
		{
			DeviceBindingSource deviceBindingSource = ((this.mapState != InGameMap.MapState.Exploring) ? ((DeviceBindingSource)ControlsController.ControlsActions.SubmitMark.Bindings[0]) : ((DeviceBindingSource)ControlsController.ControlsActions.AddMark.Bindings[0]));
			this.AddButtonHelpLoc.SetTerm(deviceBindingSource.Control);
		}
		if (profile.Buoys != null)
		{
			this._buoys = profile.Buoys.Where((BuoySetting x) => x.PondId == StaticUserData.CurrentPond.PondId).ToList<BuoySetting>();
		}
		if (flag)
		{
			this.SetExplorationState();
		}
		else if (this.mapState == InGameMap.MapState.Exploring)
		{
			this.SetTexts();
		}
	}

	private void SetTexts()
	{
		this.helpPanel.SetActive(SettingsManager.InputType == InputModuleManager.InputType.GamePad);
		this.helpPanelMouse.SetActive(!this.helpPanel.activeSelf);
		this.SetFiltersNavigationEnabled(true);
		if (this.gamepad)
		{
			this.helpPanelTexts[0].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.LeftStickLeft], ScriptLocalization.Get("MoveMapCaption"));
			this.helpPanelTexts[1].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.LeftStickButton], ScriptLocalization.Get("CenterMap"));
			if (!this.BuyMoreBuoysButton.activeInHierarchy)
			{
				this.helpPanelTexts[2].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action3], ScriptLocalization.Get("StartSetMark"));
				if (this._currentMagnetedToggle != null)
				{
					this.helpPanelTexts[3].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action1], ScriptLocalization.Get("SelectMark"));
				}
				else if (this.OpenShareRequests.activeInHierarchy)
				{
					this.helpPanelTexts[3].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action4], ScriptLocalization.Get("SharedBuoysTitle"));
				}
				else
				{
					this.helpPanelTexts[3].text = string.Empty;
				}
			}
			else
			{
				this.helpPanelTexts[2].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action3], ScriptLocalization.Get("BuyBuoysTitle"));
				if (this._currentMagnetedToggle != null)
				{
					this.helpPanelTexts[3].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action1], ScriptLocalization.Get("SelectMark"));
				}
				else if (this.OpenShareRequests.activeInHierarchy)
				{
					this.helpPanelTexts[3].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action4], ScriptLocalization.Get("SharedBuoysTitle"));
				}
				else
				{
					this.helpPanelTexts[3].text = string.Empty;
				}
			}
			this.helpPanelTexts[4].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.RightStickUp], ScriptLocalization.Get("ZoomCaption"));
			this.helpPanelTexts[5].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.RightStickButton], ScriptLocalization.Get("OrientationMap"));
			if (this._currentMagnetedToggle != null)
			{
				this.helpPanelTexts[6].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action2], ScriptLocalization.Get("CloseTabletMode"));
				this.helpPanelTexts[7].text = string.Empty;
			}
			else
			{
				this.helpPanelTexts[6].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.DPadDown], ScriptLocalization.Get("NavigateFilters"));
				this.helpPanelTexts[7].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action2], ScriptLocalization.Get("CloseTabletMode"));
			}
		}
		else
		{
			this.helpPanelMouseTexts[0].text = "\ue768";
			this.helpPanelMouseTexts[1].text = ScriptLocalization.Get("MoveMapCaption");
			this.helpPanelMouseTexts[2].text = ((!this.BuyMoreBuoysButton.activeInHierarchy) ? "\ue742" : "\ue62c");
			this.helpPanelMouseTexts[3].text = ScriptLocalization.Get((!this.BuyMoreBuoysButton.activeInHierarchy) ? "StartSetMark" : "BuyBuoysTitle");
			this.helpPanelMouseTexts[4].text = "\ue765";
			this.helpPanelMouseTexts[5].text = ScriptLocalization.Get("LocalMove");
			this.helpPanelMouseTexts[6].text = "\ue772";
			this.helpPanelMouseTexts[7].text = ScriptLocalization.Get("ZoomCaption");
			this.helpPanelMouseTexts[8].text = "\ue769";
			this.helpPanelMouseTexts[9].text = ScriptLocalization.Get("CloseTabletMode");
		}
	}

	private void SetMapInfoPanelTexts()
	{
		this.helpPanel.SetActive(true);
		this.helpPanelMouse.SetActive(false);
		this.SetFiltersNavigationEnabled(false);
		this.helpPanel.SetActive(this.gamepad);
		this.helpPanelMouse.SetActive(!this.helpPanel.activeSelf);
		if (this.gamepad)
		{
			foreach (Text text in this.helpPanelTexts)
			{
				text.font = this.GamePadFont;
			}
			this.helpPanelTexts[0].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action4], ScriptLocalization.Get("ShareCaption"));
			this.helpPanelTexts[1].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action3], ScriptLocalization.Get("RemoveBuoyCaption"));
			this.helpPanelTexts[2].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action1], ScriptLocalization.Get("RenameBuoyCaption"));
			this.helpPanelTexts[3].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action2], ScriptLocalization.Get("Buttons/[Back]"));
			this.helpPanelTexts[4].text = string.Empty;
			this.helpPanelTexts[5].text = string.Empty;
			this.helpPanelTexts[6].text = string.Empty;
			this.helpPanelTexts[7].text = string.Empty;
		}
		else
		{
			this.helpPanelMouseTexts[0].text = "\ue735";
			this.helpPanelMouseTexts[1].text = ScriptLocalization.Get("ShareCaption");
			this.helpPanelMouseTexts[2].text = "\ue613";
			this.helpPanelMouseTexts[3].text = ScriptLocalization.Get("RemoveBuoyCaption");
			this.helpPanelMouseTexts[4].text = "\ue769";
			this.helpPanelMouseTexts[5].text = ScriptLocalization.Get("Buttons/[Back]");
			this.helpPanelMouseTexts[6].text = string.Empty;
			this.helpPanelMouseTexts[7].text = string.Empty;
			this.helpPanelMouseTexts[8].text = string.Empty;
			this.helpPanelMouseTexts[9].text = string.Empty;
		}
	}

	private void SetEditingModeTexts()
	{
		this.SetFiltersNavigationEnabled(false);
		this.helpPanel.SetActive(this.gamepad);
		this.helpPanelMouse.SetActive(!this.helpPanel.activeSelf);
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			foreach (Text text in this.helpPanelTexts)
			{
				text.font = this.GamePadFont;
			}
			this.helpPanelTexts[0].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action1], ScriptLocalization.Get("SaveButton"));
			this.helpPanelTexts[1].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action2], ScriptLocalization.Get("CancelButton"));
			this.helpPanelTexts[2].text = string.Empty;
			this.helpPanelTexts[3].text = string.Empty;
			this.helpPanelTexts[4].text = string.Empty;
			this.helpPanelTexts[5].text = string.Empty;
			this.helpPanelTexts[6].text = string.Empty;
			this.helpPanelTexts[7].text = string.Empty;
		}
		else
		{
			this.helpPanelMouseTexts[0].text = "\ue64e";
			this.helpPanelMouseTexts[1].text = ScriptLocalization.Get("SaveButton");
			this.helpPanelMouseTexts[2].text = "\ue61e";
			this.helpPanelMouseTexts[3].text = ScriptLocalization.Get("CancelButton");
			this.helpPanelMouseTexts[4].text = "\ue769";
			this.helpPanelMouseTexts[5].text = ScriptLocalization.Get("Buttons/[Back]");
			this.helpPanelMouseTexts[6].text = string.Empty;
			this.helpPanelMouseTexts[7].text = string.Empty;
			this.helpPanelMouseTexts[8].text = string.Empty;
			this.helpPanelMouseTexts[9].text = string.Empty;
		}
	}

	private void SetFilterModeTexts()
	{
		if (this.gamepad)
		{
			this.helpPanel.SetActive(true);
			this.helpPanelTexts[0].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action1], ScriptLocalization.Get("Select"));
			this.helpPanelTexts[1].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action2], ScriptLocalization.Get("Buttons/[Back]"));
			this.helpPanelTexts[2].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.DPadDown], ScriptLocalization.Get("NavigateFilters"));
			this.helpPanelTexts[3].text = string.Empty;
			this.helpPanelTexts[4].text = string.Empty;
			this.helpPanelTexts[5].text = string.Empty;
			this.helpPanelTexts[6].text = string.Empty;
			this.helpPanelTexts[7].text = string.Empty;
		}
	}

	private void SetMarkingModeTexts()
	{
		this.SetFiltersNavigationEnabled(false);
		this.helpPanel.SetActive(this.gamepad);
		this.helpPanelMouse.SetActive(!this.gamepad);
		if (this.gamepad)
		{
			if (this.AddButtonHelp.activeInHierarchy)
			{
				DeviceBindingSource deviceBindingSource = (DeviceBindingSource)ControlsController.ControlsActions.SubmitMark.Bindings[0];
				this.AddButtonHelpLoc.SetTerm(deviceBindingSource.Control);
			}
			this.helpPanelTexts[0].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.LeftStickLeft], ScriptLocalization.Get("MoveMapCaption"));
			this.helpPanelTexts[1].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.LeftStickButton], ScriptLocalization.Get("CenterMap"));
			this.helpPanelTexts[2].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action1], ScriptLocalization.Get("SubmitMark"));
			this.helpPanelTexts[3].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.RightStickUp], ScriptLocalization.Get("ZoomCaption"));
			this.helpPanelTexts[4].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.RightStickButton], ScriptLocalization.Get("OrientationMap"));
			this.helpPanelTexts[5].text = string.Format("{0} {1}", HotkeyIcons.KeyMappings[InputControlType.Action2], ScriptLocalization.Get("Buttons/[Back]"));
			this.helpPanelTexts[6].text = string.Empty;
			this.helpPanelTexts[7].text = string.Empty;
		}
		else
		{
			this.helpPanelMouseTexts[0].text = "\ue768";
			this.helpPanelMouseTexts[1].text = ScriptLocalization.Get("MoveMapCaption");
			this.helpPanelMouseTexts[2].text = "\ue772";
			this.helpPanelMouseTexts[3].text = ScriptLocalization.Get("ZoomCaption");
			this.helpPanelMouseTexts[4].text = "\ue766";
			this.helpPanelMouseTexts[5].text = ScriptLocalization.Get("SubmitMark");
			this.helpPanelMouseTexts[6].text = "\ue769";
			this.helpPanelMouseTexts[7].text = ScriptLocalization.Get("Buttons/[Back]");
			this.helpPanelMouseTexts[8].text = string.Empty;
			this.helpPanelMouseTexts[9].text = string.Empty;
		}
	}

	private void Awake()
	{
		InGameMap.Instance = this;
		this.terrainMask = 1 << LayerMask.NameToLayer("Terrain");
		this.weatherWidget = new WeatherWidget();
		this.weatherWidget.Init(this.weatherControl, this.pressureControl, this.windDirectionControl, this.windPowerControl, this.windPowerSuffix.gameObject, this.temperatureControl, this.windCompass, this.temperatureWaterControl, false);
		this.weatherWidget.VisualizeWeather();
		FlowRenderHeightmap flowRenderHeightmap = Object.FindObjectsOfType<FlowRenderHeightmap>().FirstOrDefault((FlowRenderHeightmap x) => x.isActiveAndEnabled);
		this.mapRenderer = new MapRenderer(flowRenderHeightmap, Object.FindObjectsOfType<Terrain>(), SurfaceSettings.LowestBottomY, this.mapImage);
		this.mapRenderer.Render(false);
		this.buoyparent = new GameObject("buoys");
		this.mapInfoPanel = new MapInfoPanel(this.infoPanel.GetComponent<RectTransform>());
		this.markPlacer = new MarkPlacer(this.markPrefab, this.mapParent, this.mapImage.GetComponent<RectTransform>().sizeDelta, this.mapRenderer);
		this.missionObjectsPlacer = new MarkPlacer(this.missionObjectPrefab, this.missionObjectsParent, this.mapImage.GetComponent<RectTransform>().sizeDelta, this.mapRenderer);
		this.rodPodsPlacer = new MarkPlacer(this.rodPodIconPrefab, this.mapParent, this.mapImage.GetComponent<RectTransform>().sizeDelta, this.mapRenderer);
		this.backGroundPlacer = new MarkPlacer(this.backGroundPrefab, this.bgParent, this.mapImage.GetComponent<RectTransform>().sizeDelta, this.mapRenderer);
		Vector3 inGameCoord = this.mapRenderer.GetInGameCoord(0f, 0f);
		this.backGroundPlacer.AddMark(inGameCoord);
		GameObject gameObject = this.backGroundPlacer.InstantiatedObjects[this.backGroundPlacer.InstantiatedObjects.Count - 1];
		this.mapBgTiledImage = gameObject.GetComponentInChildren<Image>();
		this.mapBgInitialScale = this.mapBgTiledImage.transform.localScale;
		this.SpawnRest();
		this.isobarScaleUI = new IsobarScale(this.isobarScale, this.low, this.ruller, SurfaceSettings.LowestBottomY);
		this.isobarScaleUI.GenerateScale(this.mapRenderer.IsobarDepthStep);
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted += this.SpawnRest;
		PhotonConnectionFactory.Instance.OnGotAvailableLocations += this.OnGotAvailableLocations;
		InGameMap.FishInfo = null;
		HintSystem.Instance.SpawnActiveMissionMarks();
	}

	private void Start()
	{
		this.UpdateTime();
		this.weatherWidget.VisualizeWeather();
		for (int i = 0; i < this.toogles.Length; i++)
		{
			int k = i;
			this.toogles[i].onValueChanged.AddListener(delegate(bool isOn)
			{
				Toggle toggle = this.toogles[k];
				MapToggleHandler component = toggle.GetComponent<MapToggleHandler>();
				if (component != null)
				{
					component.Change(isOn);
				}
				this._togglesFiltersObjectsVisibility[k].SetActive(isOn);
			});
		}
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this.SetTexts();
		this.Crosshair.SetActive(this.gamepad);
		for (int i = 0; i < this.objectsToHideOnConsoles.Length; i++)
		{
			this.objectsToHideOnConsoles[i].SetActive(type != InputModuleManager.InputType.GamePad);
		}
		this.RefreshBuoysCapacity();
	}

	private void SetFiltersNavigationEnabled(bool enabled)
	{
		bool flag = enabled && this._currentMagnetedToggle == null;
		this.filtersNavigation.enabled = flag;
		if (!flag && !EventSystem.current.alreadySelecting)
		{
			UINavigation.SetSelectedGameObject(null);
		}
	}

	public void SwitchToggle(Toggle a)
	{
		a.isOn = !a.isOn;
		UINavigation.SetSelectedGameObject(null);
	}

	private void OnDestroy()
	{
		this.mapRenderer.Clean();
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted -= this.SpawnRest;
		PhotonConnectionFactory.Instance.OnGotAvailableLocations -= this.OnGotAvailableLocations;
	}

	private void RebuildUsersList()
	{
		if (this.usersPlacer != null)
		{
			this.usersPlacer.Clear();
		}
		List<Transform> list = new List<Transform>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < this._players.Count; i++)
		{
			PlayerRecord playerRecord = this._players[i];
			list2.Add(playerRecord.PlayerName);
			list.Add(playerRecord.Controller.SceletonPartTransform);
		}
		list.Add(GameFactory.PlayerTransform);
		List<Color> list3 = new List<Color>();
		list3.AddRange(list2.Select(delegate(string id)
		{
			if (PhotonConnectionFactory.Instance.Profile.Friends != null && PhotonConnectionFactory.Instance.Profile.Friends.Exists((Player pl) => pl.UserId == id))
			{
				return this.friend;
			}
			return this.notFriend;
		}));
		list3.Add(this.self);
		this.usersPlacer = new MarkPlacer(this.userPrefab, this.userParent, this.mapImage.GetComponent<RectTransform>().sizeDelta, list, this.mapRenderer, list3);
		this.usersPlacer.Visualize();
	}

	private void OnGotAvailableLocations(IEnumerable<LocationBrief> locations)
	{
		if (this.locationPlacer != null)
		{
			return;
		}
		IEnumerable<string> enumerable = locations.Select((LocationBrief x) => x.Asset);
		this.locationPlacer = new MarkPlacer(this.locationPrefab, this.locationParent, this.mapImage.GetComponent<RectTransform>().sizeDelta, enumerable, this.mapRenderer);
		this.locationPlacer.Visualize();
	}

	private void SpawnRest()
	{
		if (PhotonConnectionFactory.Instance.Profile.Buoys != null)
		{
			this._buoys = PhotonConnectionFactory.Instance.Profile.Buoys.Where((BuoySetting x) => x.PondId == StaticUserData.CurrentPond.PondId).ToList<BuoySetting>();
			foreach (BuoySetting buoySetting in this._buoys)
			{
				Vector3 inGameCoord = this.mapRenderer.GetInGameCoord(buoySetting.Position.X, buoySetting.Position.Y);
				if (!this.buoys.ContainsKey(buoySetting.BuoyId))
				{
					this.buoys.Add(buoySetting.BuoyId, Object.Instantiate<GameObject>(this.buoyPrefab, inGameCoord, Quaternion.identity, this.buoyparent.transform));
					this.markPlacer.AddMark(inGameCoord);
					Toggle component = this.markPlacer.InstantiatedObjects[this.markPlacer.InstantiatedObjects.Count - 1].GetComponent<Toggle>();
					this.SetBuoyLogicOnToggle(component, buoySetting);
				}
			}
		}
		this.RefreshBuoysCapacity();
	}

	public void AddRodPod(string id, Vector3 pos)
	{
		RectTransform rectTransform = this.rodPodsPlacer.AddMark(pos, Vector3.zero, id);
		this.rodPodsPlacer.UpdateVizualization();
	}

	public void RemoveRodPod(string id)
	{
		if (this.rodPodsPlacer.RemoveByName(id))
		{
			this.rodPodsPlacer.UpdateVizualization();
		}
	}

	public void AddMissionObject(int id, Vector3 pos, Vector3 rot, GameMarkerState state, string taskName)
	{
		RectTransform rectTransform = this.missionObjectsPlacer.AddMark(pos, Vector3.zero, string.Format("mo_{0}", id));
		rectTransform.GetComponent<MissionObject>().UpdateState(state, taskName);
		this.missionObjectsPlacer.UpdateVizualization();
	}

	public void RemoveMissionObject(int id)
	{
		if (this.missionObjectsPlacer.RemoveByName(string.Format("mo_{0}", id)))
		{
			this.missionObjectsPlacer.UpdateVizualization();
		}
	}

	private void SetBuoyLogicOnToggle(Toggle toggle, BuoySetting buoy)
	{
		toggle.group = this.mapParent.GetComponent<ToggleGroup>();
		if (this._toggles == null)
		{
			this._toggles = new Dictionary<int, Toggle>();
		}
		if (!this._toggles.ContainsKey(buoy.BuoyId))
		{
			this._toggles.Add(buoy.BuoyId, toggle);
		}
		Transform transform = toggle.transform.Find("other/NameText");
		if (transform != null)
		{
			transform.GetComponent<Text>().text = buoy.Name;
		}
		toggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			BuoySetting buoy2 = buoy;
			this.currentBuoyId = buoy2.BuoyId;
			this._currentMark = toggle;
			if (isOn && this.mapState == InGameMap.MapState.Exploring)
			{
				Vector3 inGameCoord = this.mapRenderer.GetInGameCoord(buoy2.Position.X, buoy2.Position.Y);
				if (this._currentMagnetedToggle != null && this._currentMagnetedToggle != toggle)
				{
					this.UncheckMagnetedToggle();
				}
				this._currentMagnetedToggle = toggle;
				this._magnetPosition = new Vector2(buoy2.Position.X, buoy2.Position.Y);
				this.mapRenderer.SetTargetPosition(inGameCoord);
				bool flag = (Quaternion.Euler(0f, 0f, this.mapImage.transform.localEulerAngles.z) * (toggle.GetComponent<RectTransform>().anchoredPosition - this.mapImage.GetComponent<RectTransform>().anchoredPosition) + this.mapImage.GetComponent<RectTransform>().anchoredPosition).x > 0f;
				Vector2 vector = ((!flag) ? this.sRight : this.sLeft);
				Vector2 vector2 = ((!flag) ? this.lRight : this.lLeft);
				this.SetMapInfoPanelTexts();
				this.mapInfoPanel.OpenInfoPanel(vector, vector2);
				this._currentBuoyName = buoy2.Name;
				this.buoyName.text = buoy2.Name;
				if (buoy2.CreatedTime != null)
				{
					this.CreatedTime.text = buoy2.CreatedTime.Value.ToShortDateString();
				}
				this.TogglePosition.text = string.Format("[ {0:0.00} , {1:0.00} ]", inGameCoord.x, inGameCoord.z);
				if (buoy2 != null && buoy2.Fish != null)
				{
					this.concreteTrophyInit.gameObject.SetActive(true);
					ConcreteTrophyInit concreteTrophyInit = this.concreteTrophyInit;
					CaughtFish fish = buoy2.Fish;
					DateTime? createdTime = buoy2.CreatedTime;
					concreteTrophyInit.Refresh(fish, (createdTime == null) ? DateTime.UtcNow : createdTime.Value, buoy2.PondId);
				}
				else
				{
					this.concreteTrophyInit.SetPanelEmpty();
				}
			}
			else if (this.mapState != InGameMap.MapState.Exploring)
			{
				isOn = !isOn;
			}
		});
	}

	private int BuoyCount()
	{
		return PhotonConnectionFactory.Instance.Profile.Inventory.AvailableBuoyCapacity;
	}

	private void SetMarkState()
	{
		this.mapState = InGameMap.MapState.BuoyMark;
		CursorManager.Instance.SetCursor(CursorType.Selecting);
		this.mapParent.GetComponent<CanvasGroup>().blocksRaycasts = false;
		this.SetMarkingModeTexts();
		if (this.gamepad)
		{
			TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(this.Crosshair.transform, Vector3.one * 0.6f, 0.1f), 4), delegate
			{
				this.CrosshairImage.gameObject.SetActive(true);
			});
		}
	}

	private void SetExplorationState()
	{
		this.mapState = InGameMap.MapState.Exploring;
		CursorManager.Instance.SetCursor(CursorType.Standart);
		this.mapParent.GetComponent<CanvasGroup>().blocksRaycasts = true;
		this.SetTexts();
		if (this.gamepad)
		{
			this.CrosshairImage.gameObject.SetActive(false);
			TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOScale(this.Crosshair.transform, Vector3.one * 1f, 0.1f), 4);
		}
	}

	public void AddMark()
	{
		if (this.BuoyCount() > 0)
		{
			this.StartMarking();
		}
		else
		{
			this.BuyBuoysWindowOpen();
		}
	}

	private void StartMarking()
	{
		this.SetMarkState();
		if (InGameMap.FishInfo != null)
		{
			this.MessageBox = this._helpers.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("MarkLastFishMessage"), true).gameObject;
			this.MessageBox.GetComponent<MessageBox>().confirmButton.AddComponent<HintElementId>().SetElementId("NavMap_ConfirmBuoyLastFish", null, null);
			this.MessageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object ea, EventArgs o)
			{
				if (this.MessageBox != null)
				{
					this.MessageBox.GetComponent<MessageBox>().Close();
				}
				this.AddBuoyLocationToCoords(GameFactory.Player.CatchedFishPos);
				InGameMap.BlockInputForMaps();
				this.SetExplorationState();
				InGameMap.FishInfo = null;
			};
			this.MessageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object ea, EventArgs o)
			{
				if (this.MessageBox != null)
				{
					this.MessageBox.GetComponent<MessageBox>().Close();
				}
				InGameMap.BlockInputForMaps();
				InGameMap.FishInfo = null;
			};
			if (HintSystem.Instance != null)
			{
				if (HintSystem.Instance.activeHints.Any((ManagedHint x) => x.Message.MissionId == 60))
				{
					this.MessageBox.GetComponent<MessageBox>().declineButton.GetComponent<Button>().interactable = false;
				}
			}
		}
	}

	public void DeleteMark()
	{
		this.MessageBox = this._helpers.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("RemoveBuoyConfirm"), true).gameObject;
		this.MessageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object ea, EventArgs o)
		{
			if (this.MessageBox != null)
			{
				this.MessageBox.GetComponent<MessageBox>().Close();
			}
			Object.Destroy(this.buoys[this.currentBuoyId]);
			this.buoys.Remove(this.currentBuoyId);
			int num = this.markPlacer.InstantiatedObjects.FindIndex((GameObject t) => t.gameObject == this._currentMark.gameObject);
			this.markPlacer.RemoveAt(num);
			this.RemoveBuoyById(this.currentBuoyId);
			this.CloseMapInfoPanel();
		};
		this.MessageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object ea, EventArgs o)
		{
			if (this.MessageBox != null)
			{
				this.MessageBox.GetComponent<MessageBox>().Close();
			}
		};
	}

	public void ShareMark()
	{
		this.CloseMapInfoPanel();
		this._currentMark.isOn = false;
		BuoySetting buoySetting = PhotonConnectionFactory.Instance.Profile.Buoys.FirstOrDefault((BuoySetting x) => x.BuoyId == this.currentBuoyId);
		if (buoySetting != null)
		{
			ShareBuoyFriendList friendsList = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.SelectFriendsBuoy).GetComponent<ShareBuoyFriendList>();
			friendsList.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			friendsList.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			friendsList.Init(buoySetting);
			friendsList.CancelButtonText = ScriptLocalization.Get("CancelButton");
			friendsList.AcceptButtonText = ScriptLocalization.Get("ShareCaption");
			friendsList.Caption = buoySetting.Name;
			friendsList.GetComponent<EventConfirmAction>().CancelActionCalled += delegate
			{
				friendsList.Close();
			};
			ShareBuoyFriendList friendsList3 = friendsList;
			friendsList3.ConfirmAction = (Action<string>)Delegate.Combine(friendsList3.ConfirmAction, new Action<string>(this.Share));
			ShareBuoyFriendList friendsList2 = friendsList;
			friendsList2.ConfirmAction = (Action<string>)Delegate.Combine(friendsList2.ConfirmAction, new Action<string>(delegate
			{
				friendsList.Close();
			}));
			if (MessageFactory.MessageBoxQueue.Contains(friendsList))
			{
				MessageFactory.MessageBoxQueue.Remove(friendsList);
			}
			friendsList.Open();
			this.MessageBox = friendsList.gameObject;
		}
	}

	private void Share(string friend)
	{
		PhotonConnectionFactory.Instance.ShareBuoy(this.currentBuoyId, friend);
		PhotonConnectionFactory.Instance.OnBuoyShared += this.OnBuoyShared;
	}

	public void OpenRequests()
	{
		this.MessageBox = MenuHelpers.Instance.ShowBuoysDelivered(InfoMessageController.Instance.gameObject, PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.FirstOrDefault((BuoySetting x) => x.PondId == StaticUserData.CurrentPond.PondId));
	}

	public void OnMapClick()
	{
		if (this.mapState == InGameMap.MapState.BuoyMark && (Input.GetMouseButtonUp(0) || this.gamepad))
		{
			Vector3 vector = Input.mousePosition;
			if (this.gamepad)
			{
				vector = Camera.main.WorldToScreenPoint(this.CrosshairImage.transform.position);
			}
			RaycastHit raycastHit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(vector), ref raycastHit))
			{
				this.currentCoords = this.markPlacer.TransformCoordinates(raycastHit.textureCoord);
				this.currentCoords = this.mapRenderer.GetInGameCoord(this.currentCoords.x, this.currentCoords.z);
				Vector2 textureCoord = raycastHit.textureCoord;
				if (Physics.Raycast(new Ray(this.currentCoords + Vector3.up, Vector3.down), ref raycastHit, 100f, this.terrainMask) && raycastHit.point.y < 0f)
				{
					this.AddBuoyLocationToCoords(textureCoord);
				}
			}
		}
		this.SetExplorationState();
	}

	private void UpdatePositionText()
	{
		this.sb.Length = 0;
		this.sb.AppendFormat("[ {0:0.00} , {1:0.00} ]", this.currentCoords.x, this.currentCoords.z);
		this.coordinatesText.text = this.sb.ToString();
	}

	private void AddBuoyRequest(Vector2 mapCoordinates, string name, CaughtFish fish)
	{
		if (PhotonConnectionFactory.Instance.Profile.Inventory.AvailableBuoyCapacity > 0)
		{
			PhotonConnectionFactory.Instance.OnBuoySet += this.OnBuoySet;
			PhotonConnectionFactory.Instance.OnBuoySettingFailed += this.OnBuoySettingFailed;
			PhotonConnectionFactory.Instance.SetBuoy(name, new Point3(mapCoordinates.x, 0f, mapCoordinates.y), fish);
		}
	}

	private void OnBuoySet(BuoySetting buoy)
	{
		PhotonConnectionFactory.Instance.OnBuoySet -= this.OnBuoySet;
		PhotonConnectionFactory.Instance.OnBuoySettingFailed -= this.OnBuoySettingFailed;
		Vector3 inGameCoord = this.mapRenderer.GetInGameCoord(buoy.Position.X, buoy.Position.Y);
		this.SetBuoyLogicOnToggle(this._pendingToggle, buoy);
		this.buoys.Add(buoy.BuoyId, Object.Instantiate<GameObject>(this.buoyPrefab, inGameCoord, Quaternion.identity, this.buoyparent.transform));
		this.RefreshBuoysCapacity();
		this._pendingToggle.isOn = true;
		this._pendingToggle = null;
		GameFactory.Message.ShowLowerMessage(ScriptLocalization.Get("BuoyPlaced"), null, 2f, false);
	}

	public void RemoveBuoyById(int id)
	{
		BuoySetting buoySetting = PhotonConnectionFactory.Instance.Profile.Buoys.FirstOrDefault((BuoySetting x) => x.BuoyId == id);
		if (this._toggles.ContainsKey(id))
		{
			this._toggles.Remove(id);
		}
		if (buoySetting != null)
		{
			PhotonConnectionFactory.Instance.OnBuoyTaken += this.OnBuoyTaken;
			PhotonConnectionFactory.Instance.OnBuoyTakingFailed += this.OnBuoyTakingFailed;
			PhotonConnectionFactory.Instance.TakeBuoy(buoySetting.BuoyId);
		}
	}

	private void OnBuoyTaken(int buoyId)
	{
		PhotonConnectionFactory.Instance.OnBuoyTaken -= this.OnBuoyTaken;
		PhotonConnectionFactory.Instance.OnBuoyTakingFailed -= this.OnBuoyTakingFailed;
		if (this._toggles.ContainsKey(buoyId))
		{
			this._toggles.Remove(buoyId);
		}
		GameFactory.Message.ShowLowerMessage(ScriptLocalization.Get("BuoyTaken"), null, 2f, false);
		this.RefreshBuoysCapacity();
	}

	private void OnBuoyShared()
	{
		PhotonConnectionFactory.Instance.OnBuoyShared -= this.OnBuoyShared;
		PhotonConnectionFactory.Instance.OnBuoySharingFailed -= this.OnBuoySharingFailed;
		GameFactory.Message.ShowLowerMessage(ScriptLocalization.Get("BuoyShareSuccess"), null, 2f, false);
	}

	private void OnBuoySharingFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnBuoyShared -= this.OnBuoyShared;
		PhotonConnectionFactory.Instance.OnBuoySharingFailed -= this.OnBuoySharingFailed;
		GameFactory.Message.ShowLowerMessage(failure.ErrorMessage, null, 2f, false);
	}

	private void OnBuoyTakingFailed(Failure failure)
	{
		PhotonConnectionFactory.Instance.OnBuoyTaken -= this.OnBuoyTaken;
		PhotonConnectionFactory.Instance.OnBuoyTakingFailed -= this.OnBuoyTakingFailed;
		GameFactory.Message.ShowLowerMessage(failure.ErrorMessage, null, 2f, false);
	}

	private void OnProductBought(ProfileProduct product, int count)
	{
		this.RefreshBuoysCapacity();
	}

	private void OnBuoySettingFailed(Failure failure)
	{
		if (this._pendingToggle != null)
		{
			int num = this.markPlacer.InstantiatedObjects.FindIndex((GameObject t) => t.gameObject == this._pendingToggle.gameObject);
			this.markPlacer.RemoveAt(num);
			this._pendingToggle = null;
		}
		GameFactory.Message.ShowLowerMessage(failure.ErrorMessage, null, 2f, false);
		PhotonConnectionFactory.Instance.OnBuoySet -= this.OnBuoySet;
		PhotonConnectionFactory.Instance.OnBuoySettingFailed -= this.OnBuoySettingFailed;
		this.RefreshBuoysCapacity();
	}

	private void AddBuoyLocationToCoords(Vector2 coords)
	{
		this.markPlacer.AddMark(coords);
		this._pendingToggle = this.markPlacer.InstantiatedObjects[this.markPlacer.InstantiatedObjects.Count - 1].GetComponent<Toggle>();
		Vector3 vector = this.markPlacer.TransformCoordinates(coords);
		Vector2 vector2;
		vector2..ctor(vector.x, vector.z);
		this.AddBuoyRequest(vector2, ScriptLocalization.Get("MarkDefaultName"), InGameMap.FishInfo);
	}

	private void AddBuoyLocationToCoords(Vector3 coords)
	{
		this.markPlacer.UpdateVizualization();
		Vector2 vector = this.markPlacer.AddMark(coords);
		this._pendingToggle = this.markPlacer.InstantiatedObjects[this.markPlacer.InstantiatedObjects.Count - 1].GetComponent<Toggle>();
		Vector3 vector2 = this.markPlacer.TransformCoordinates(vector);
		Vector2 vector3;
		vector3..ctor(vector2.x, vector2.z);
		this.AddBuoyRequest(vector3, ScriptLocalization.Get("MarkDefaultName"), InGameMap.FishInfo);
	}

	private void CheckPositions()
	{
		Vector3 mapOffset = this.mapRenderer.MapOffset;
		Vector3 mapCoord = this.markPlacer.GetMapCoord(mapOffset - this.mapRenderer.InitialMin(), this.mapRenderer.InitialMin(), this.mapRenderer.InitialMax());
		Vector2 textureCoord = new Vector2(mapCoord.x, mapCoord.z);
		if (this._buoys != null)
		{
			float num = this.minRadius;
			List<BuoySetting> list = this._buoys.Where((BuoySetting x) => Vector2.Distance(new Vector2(x.Position.X, x.Position.Y), textureCoord) < this.minRadius * Mathf.Clamp01(2f * this.mapRenderer.ScaleRatio)).ToList<BuoySetting>();
			if (list.Count > 0 && !this.mapRenderer.ShouldIgnoreMovement() && !this.isDragged)
			{
				if (list.Count > 1)
				{
					list.Sort((BuoySetting x, BuoySetting y) => Vector2.Distance(new Vector2(x.Position.X, x.Position.Y), textureCoord).CompareTo(Vector2.Distance(new Vector2(y.Position.X, y.Position.Y), textureCoord)));
				}
				if (ControlsController.ControlsActions.MapMove.Value.magnitude < 0.05f && this._currentMagnetedToggle == null)
				{
					this._magnetPosition = new Vector2(list[0].Position.X, list[0].Position.Y);
					if (this._toggles.ContainsKey(list[0].BuoyId))
					{
						this._currentMagnetedToggle = this._toggles[list[0].BuoyId];
						this.HighlightMagnetedToggle();
					}
				}
			}
		}
	}

	private void UncheckMagnetedToggle()
	{
		if (this._currentMagnetedToggle == null)
		{
			return;
		}
		IPointerExitHandler[] componentsInChildren = this._currentMagnetedToggle.GetComponentsInChildren<IPointerExitHandler>();
		foreach (IPointerExitHandler pointerExitHandler in componentsInChildren)
		{
			pointerExitHandler.OnPointerExit(null);
		}
		this._currentMagnetedToggle = null;
		if (this.mapState != InGameMap.MapState.BuoyMark)
		{
			ShortcutExtensions.DOScale(this.Crosshair.transform, Vector3.one, 0.1f);
		}
		if (this.mapState == InGameMap.MapState.Exploring)
		{
			this.SetTexts();
		}
	}

	private void HighlightMagnetedToggle()
	{
		ShortcutExtensions.DOScale(this.Crosshair.transform, Vector3.one * 0.8f, 0.1f);
		IPointerEnterHandler[] componentsInChildren = this._currentMagnetedToggle.GetComponentsInChildren<IPointerEnterHandler>();
		foreach (IPointerEnterHandler pointerEnterHandler in componentsInChildren)
		{
			pointerEnterHandler.OnPointerEnter(null);
		}
		this.SetTexts();
	}

	private void SelectMagnetedToggle()
	{
		if (this._currentMagnetedToggle != null)
		{
			this._currentMagnetedToggle.isOn = true;
		}
	}

	private void UpdateCurrentCoordsAndCursorTexture(bool setCursorTexture = true)
	{
		Vector3 vector = Input.mousePosition;
		if (this.gamepad)
		{
			vector = Camera.main.WorldToScreenPoint(this.CrosshairImage.transform.position);
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(vector), ref raycastHit))
		{
			this.currentCoords = this.markPlacer.TransformCoordinates(raycastHit.textureCoord);
			this.currentCoords = this.mapRenderer.GetInGameCoord(this.currentCoords.x, this.currentCoords.z);
			if (setCursorTexture)
			{
				Vector2 textureCoord = raycastHit.textureCoord;
				if (Physics.Raycast(new Ray(this.currentCoords + Vector3.up, Vector3.down), ref raycastHit, 100f, this.terrainMask))
				{
					if (raycastHit.point.y < 0f)
					{
						if (this.gamepad)
						{
							this.CrosshairImage.sprite = this.EnabledCrosshair;
						}
						else
						{
							CursorManager.Instance.SetCursor(CursorType.MapMark);
						}
					}
					else if (this.gamepad)
					{
						this.CrosshairImage.sprite = this.DisabledCrosshair;
					}
					else
					{
						CursorManager.Instance.SetCursor(CursorType.MapCantMark);
					}
				}
				else if (this.gamepad)
				{
					this.CrosshairImage.sprite = this.DisabledCrosshair;
				}
				else
				{
					CursorManager.Instance.SetCursor(CursorType.MapCantMark);
				}
			}
		}
		if (!setCursorTexture && (CursorManager.Instance.GetCursor == CursorType.MapCantMark || CursorManager.Instance.GetCursor == CursorType.MapMark))
		{
			CursorManager.Instance.SetCursor(CursorType.Standart);
		}
	}

	private void LateUpdate()
	{
		if (this.MessageBox != null && !ControlsController.ControlsActions.IsBlockedKeyboardInput)
		{
			this.BlockInputForUI();
		}
		if (this.gamepad && this.mapState == InGameMap.MapState.Exploring && this.mapParent.activeInHierarchy)
		{
			this.CheckPositions();
		}
		if (this._currentMagnetedToggle != null && !this.isDragged)
		{
			this.mapRenderer.SetTargetPosition(Vector3.Lerp(this.mapRenderer.MapOffset, this.mapRenderer.GetInGameCoord(this._magnetPosition.x, this._magnetPosition.y), Time.deltaTime * 20f));
		}
		if (this.mapRenderer.Update())
		{
			this.backGroundPlacer.UpdateVizualization();
			if (this.locationPlacer != null)
			{
				this.locationPlacer.UpdateVizualization();
			}
			this.isobarScaleUI.GenerateScale(this.mapRenderer.IsobarDepthStep);
			this.mapBgTiledImage.transform.localScale = this.mapBgInitialScale / this.mapRenderer.ScaleRatio;
		}
		this.missionObjectsPlacer.UpdateVizualization();
		this.rodPodsPlacer.UpdateVizualization();
		this.markPlacer.UpdateVizualization();
		this.usersPlacer.UpdateVizualization();
		this.UpdatePositionText();
	}

	private void Update()
	{
		if (!Cursor.visible && CursorManager.Instance.MouseCursor)
		{
			CursorManager.ShowCursor();
		}
		if (this.MessageBox == null && (ControlsController.ControlsActions.ExcludeInputList.Count == 0 || !ControlsController.ControlsActions.IsBlockedAxis))
		{
			this.RefreshBuoysCapacity();
			InGameMap.BlockInputForMaps();
		}
		if (this.North)
		{
			this._desiredAngle = this.north;
		}
		else
		{
			this._desiredAngle = GameFactory.PlayerTransform.rotation.eulerAngles.y;
		}
		this.angle = Mathf.LerpAngle(this.angle, this._desiredAngle, 10f * Time.deltaTime);
		this.mapImage.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, this.angle);
		this.bgParent.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, this.angle);
		Vector3 vector;
		vector..ctor(0f, 0f, -this.angle);
		if (this.locationPlacer != null)
		{
			this.locationPlacer.UpdateRotation(vector);
		}
		this.markPlacer.UpdateRotation(vector);
		this.backGroundPlacer.UpdateRotation(vector);
		this.missionObjectsPlacer.UpdateRotation(vector);
		this.rodPodsPlacer.UpdateRotation(vector);
		this.timer += Time.deltaTime;
		if (this.timer >= this.timerMax)
		{
			this.timer = 0f;
			this.UpdateTime();
			if (this.windCompass.activeInHierarchy)
			{
				this.windCompass.GetComponent<CompassController>().additional_shift = this.angle - GameFactory.PlayerTransform.rotation.eulerAngles.y;
			}
			this.weatherWidget.VisualizeWeather();
		}
		if (this.ignoreControls)
		{
			return;
		}
		if (this.filtersNavigation.Selectables.Any((Selectable x) => x.gameObject == EventSystem.current.currentSelectedGameObject))
		{
			this.SetFilterModeTexts();
			if (ControlsController.ControlsActions.CloseMap.WasClicked)
			{
				UINavigation.SetSelectedGameObject(null);
				this.SetTexts();
				return;
			}
		}
		if (this._editingMode)
		{
			if (ControlsController.ControlsActions.SubmitRename.WasClicked)
			{
				this.ConfirmPressed();
			}
		}
		else if (!this.mapInfoPanel.Openned)
		{
			if (ControlsController.ControlsActions.ZoomMapIn.IsPressed)
			{
				this.ZoomIn();
			}
			if (ControlsController.ControlsActions.ZoomMapOut.IsPressed)
			{
				this.ZoomOut();
			}
			if (ControlsController.ControlsActions.AddMark.WasClicked)
			{
				this.AddMark();
			}
			if (ControlsController.ControlsActions.SubmitMark.WasClicked)
			{
				if (this._currentMagnetedToggle != null)
				{
					this.SelectMagnetedToggle();
				}
				else
				{
					this.OnMapClick();
				}
			}
			if (ControlsController.ControlsActions.ChangeRotationType.WasClicked)
			{
				this.ChangeRotationMethod();
			}
			if (ControlsController.ControlsActions.MoveToPlayer.WasClicked)
			{
				this.AllignWithPlayer();
			}
			if (ControlsController.ControlsActions.ShareBuoy.WasClicked && this.OpenShareRequests.activeInHierarchy && this.gamepad && this.mapState == InGameMap.MapState.Exploring)
			{
				this.OpenRequests();
			}
		}
		else if (this.mapInfoPanel.Openned)
		{
			if (ControlsController.ControlsActions.RenameBuoy.WasClicked)
			{
				this.StartNameEdit();
				return;
			}
			if (ControlsController.ControlsActions.ShareBuoy.WasClicked)
			{
				this.ShareMark();
			}
			if (ControlsController.ControlsActions.AddMark.WasClicked)
			{
				this.DeleteMark();
			}
		}
		if (ControlsController.ControlsActions.CancelRename.WasClicked && this._editingMode)
		{
			this.CancelPressed();
		}
		else if (ControlsController.ControlsActions.CloseMap.WasClicked)
		{
			if (this.mapState == InGameMap.MapState.BuoyMark)
			{
				this.SetExplorationState();
			}
			else if (this.mapInfoPanel.Openned && !this._editingMode)
			{
				this.CloseMapInfoPanel();
			}
			else if (!this._editingMode)
			{
				GameFactory.Player.IsTransitionMapClosing = true;
				this.ignoreControls = true;
			}
		}
		if (this.MessageBox == null && !this._editingMode && !this.mapInfoPanel.Openned)
		{
			Vector2 value = ControlsController.ControlsActions.MapMove.Value;
			bool flag = value.magnitude > Mathf.Epsilon;
			if (this.gamepad)
			{
				this.recenter.SetHighlighted(ControlsController.ControlsActions.MoveToPlayer.IsPressed);
				this.orientation.SetHighlighted(ControlsController.ControlsActions.ChangeRotationType.IsPressed);
				if (flag)
				{
					this.up.SetHighlighted(value.y > 0.5f);
					this.down.SetHighlighted(value.y < -0.5f);
					this.left.SetHighlighted(value.x < -0.5f);
					this.right.SetHighlighted(value.x > 0.5f);
				}
				else
				{
					this.up.Deselect();
					this.down.Deselect();
					this.left.Deselect();
					this.right.Deselect();
				}
			}
			if (flag)
			{
				value.x = Mathf.Clamp(value.x / 0.8f, -1f, 1f);
				value.y = Mathf.Clamp(value.y / 0.8f, -1f, 1f);
				value.x *= Mathf.Abs(value.x);
				value.y *= Mathf.Abs(value.y);
				this.Shift(value * this.mapRenderer.SizeRatio * this.moveSpeed * Time.deltaTime);
			}
			this.UpdateCurrentCoordsAndCursorTexture(this.mapState == InGameMap.MapState.BuoyMark);
		}
	}

	public void OnBeginDrag()
	{
		this.prevCursor = CursorManager.Instance.GetCursor;
		CursorManager.Instance.SetCursor(CursorType.Selecting);
		this.isDragged = true;
	}

	public void OnEndDrag()
	{
		CursorManager.Instance.SetCursor(this.prevCursor);
		this.isDragged = false;
	}

	public void OnDrag(BaseEventData EventData)
	{
		this.Shift(-(EventData as PointerEventData).delta * this.mapRenderer.SizeRatio);
	}

	public void ShiftLeft()
	{
		this.Shift(Vector2.left * this.mapRenderer.SizeRatio * this.moveSpeed * Time.deltaTime);
	}

	public void ShiftRight()
	{
		this.Shift(Vector2.right * this.mapRenderer.SizeRatio * this.moveSpeed * Time.deltaTime);
	}

	public void ShiftUp()
	{
		this.Shift(Vector2.up * this.mapRenderer.SizeRatio * this.moveSpeed * Time.deltaTime);
	}

	public void ShiftDown()
	{
		this.Shift(Vector2.down * this.mapRenderer.SizeRatio * this.moveSpeed * Time.deltaTime);
	}

	public void OnScroll(BaseEventData data)
	{
		Vector2 scrollDelta = (data as PointerEventData).scrollDelta;
		this.mapRenderer.Zoom(0.5f * this.mapRenderer.ZoomScale * scrollDelta.y);
	}

	public void Shift(Vector2 direction)
	{
		if (this._currentMagnetedToggle != null)
		{
			this.UncheckMagnetedToggle();
		}
		direction = Quaternion.Euler(0f, 0f, -this.angle) * direction;
		this.mapRenderer.Move(direction);
	}

	public void ZoomIn()
	{
		this.mapRenderer.Zoom(5f * this.mapRenderer.ZoomScale * Time.deltaTime);
	}

	public void ZoomOut()
	{
		this.mapRenderer.Zoom(-5f * this.mapRenderer.ZoomScale * Time.deltaTime);
	}

	public void UpdateTime()
	{
		if (TimeAndWeatherManager.CurrentTime == null)
		{
			return;
		}
		TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
		DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
		dateTime = dateTime.Add(value);
		this.timeControl.GetComponent<Text>().text = MeasuringSystemManager.TimeString(dateTime);
		this.daysControl.GetComponent<Text>().text = string.Format("{2} {0}/{1}", (value.Days - ((value.Hours >= 5) ? 0 : 1) + 1).ToString(CultureInfo.InvariantCulture), PhotonConnectionFactory.Instance.Profile.PondStayTime, ScriptLocalization.Get("DayCaption"));
	}

	private void BlockInputForUI()
	{
		ControlsController.ControlsActions.BlockInput(null);
	}

	public static void BlockInputForMaps()
	{
		if (BlockableRegion.CurrentLayer != 0)
		{
			return;
		}
		ControlsController.ControlsActions.BlockInput(new List<string>
		{
			"ZoomMapIn", "MoveMapRight", "MoveMapLeft", "MoveMapUp", "AddMark", "SubmitMark", "ZoomMapOut", "MoveToPlayer", "ChangeRotationType", "CloseMap",
			"OpenMap", "RenameBuoy", "SubmitRename", "CancelRename", "ShareBuoy", "UISubmit"
		});
	}

	public void BuyBuoysWindowOpen()
	{
		this.MessageBox = MenuHelpers.Instance.ShowBuyProductsOfTypeWindow(null, ScriptLocalization.Get("BuyBuoysTitle"), 7);
	}

	public void CloseMapInfoPanel()
	{
		this.mapParent.GetComponent<ToggleGroup>().SetAllTogglesOff();
		this.mapInfoPanel.CloseInfoPanel();
		this._currentBuoyName = string.Empty;
		this._editingMode = false;
		this.SetExplorationState();
	}

	public void ValidateBuoyName()
	{
		if (this.buoyName.text != this._currentBuoyName && !string.IsNullOrEmpty(this._currentBuoyName) && !this._editingMode)
		{
			this.StartNameEdit();
		}
	}

	public void StartNameEdit()
	{
		this._editingMode = true;
		this.SetEditingModeTexts();
		this.buoyName.Select();
		this.EditingButtons.SetActive(true);
		this.ControlButtons.SetActive(false);
	}

	public void CancelPressed()
	{
		this.buoyName.text = this._currentBuoyName;
		this.EditingButtons.SetActive(false);
		this.ControlButtons.SetActive(true);
		this._editingMode = false;
		this.SetMapInfoPanelTexts();
		UINavigation.SetSelectedGameObject(null);
	}

	public void ConfirmPressed()
	{
		this._editingMode = false;
		this.EditingButtons.SetActive(false);
		this.ControlButtons.SetActive(true);
		this.SetMapInfoPanelTexts();
		string text = AbusiveWords.ReplaceAbusiveWords(this.buoyName.text);
		if (string.IsNullOrEmpty(text))
		{
			text = ScriptLocalization.Get("MarkDefaultName");
		}
		this.buoyName.text = text;
		PhotonConnectionFactory.Instance.RenameBuoy(this.currentBuoyId, this.buoyName.text);
		PhotonConnectionFactory.Instance.OnBuoyRenamed += this.OnBuoyRenamed;
		PhotonConnectionFactory.Instance.OnBuoyRenamingFailed += this.OnBuoyRenamingFailed;
		UINavigation.SetSelectedGameObject(null);
	}

	private void OnBuoyRenamed()
	{
		GameFactory.Message.ShowLowerMessage(ScriptLocalization.Get("BuoyRenameSuccess"), null, 2f, false);
		PhotonConnectionFactory.Instance.OnBuoyRenamed -= this.OnBuoyRenamed;
		PhotonConnectionFactory.Instance.OnBuoyRenamingFailed -= this.OnBuoyRenamingFailed;
		this._currentBuoyName = this.buoyName.text;
		Transform transform = this._currentMark.transform.Find("other/NameText");
		if (transform != null)
		{
			transform.GetComponent<Text>().text = this._currentBuoyName;
		}
	}

	private void OnBuoyRenamingFailed(Failure f)
	{
		GameFactory.Message.ShowLowerMessage(ScriptLocalization.Get("BuoyRenameFailed"), null, 2f, false);
		PhotonConnectionFactory.Instance.OnBuoyRenamed -= this.OnBuoyRenamed;
		PhotonConnectionFactory.Instance.OnBuoyRenamingFailed -= this.OnBuoyRenamingFailed;
		this.buoyName.text = this._currentBuoyName;
	}

	public static float GetMinimumHeightFromTerrains(Terrain[] terrains)
	{
		float num = terrains[0].terrainData.bounds.max.y + terrains[0].transform.position.y;
		for (int i = 0; i < terrains.Length; i++)
		{
			float num2 = terrains[i].terrainData.bounds.min.y + terrains[i].transform.position.y;
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	[SerializeField]
	private ColorCodedButton up;

	[SerializeField]
	private ColorCodedButton down;

	[SerializeField]
	private ColorCodedButton left;

	[SerializeField]
	private ColorCodedButton right;

	[SerializeField]
	private ColorCodedButton recenter;

	[SerializeField]
	private ColorCodedButton orientation;

	public GameObject Crosshair;

	public Image CrosshairImage;

	public Sprite EnabledCrosshair;

	public Sprite DisabledCrosshair;

	[SerializeField]
	private RawImage mapImage;

	private Image mapBgTiledImage;

	[SerializeField]
	private int mapWidth;

	[SerializeField]
	private int mapHeight;

	[SerializeField]
	private GameObject weatherControl;

	[SerializeField]
	private GameObject pressureControl;

	[SerializeField]
	private GameObject windDirectionControl;

	[SerializeField]
	private GameObject windPowerControl;

	[SerializeField]
	private GameObject temperatureControl;

	[SerializeField]
	private GameObject daysControl;

	[SerializeField]
	private GameObject timeControl;

	[SerializeField]
	private GameObject windCompass;

	[SerializeField]
	private GameObject temperatureWaterControl;

	[SerializeField]
	private Text windPowerSuffix;

	[SerializeField]
	private Text coordinatesText;

	[SerializeField]
	private GameObject locationPrefab;

	[SerializeField]
	private GameObject locationParent;

	[SerializeField]
	private GameObject userPrefab;

	[SerializeField]
	private GameObject userParent;

	[SerializeField]
	private GameObject missionObjectPrefab;

	[SerializeField]
	private GameObject missionObjectsParent;

	[SerializeField]
	private GameObject markPrefab;

	[SerializeField]
	private GameObject mapParent;

	[SerializeField]
	private GameObject infoPanel;

	[SerializeField]
	private GameObject bgParent;

	[SerializeField]
	private GameObject rodPodIconPrefab;

	[SerializeField]
	private GameObject backGroundPrefab;

	[SerializeField]
	private Toggle[] toogles;

	[SerializeField]
	private GameObject[] _togglesFiltersObjectsVisibility;

	[SerializeField]
	private GameObject[] rotationText;

	[SerializeField]
	private Text[] helpPanelTexts;

	[SerializeField]
	private GameObject helpPanel;

	[SerializeField]
	private Text[] helpPanelMouseTexts;

	[SerializeField]
	private GameObject helpPanelMouse;

	[SerializeField]
	private GameObject buoyPrefab;

	[SerializeField]
	private GameObject ruller;

	[SerializeField]
	private ConcreteTrophyInit concreteTrophyInit;

	[SerializeField]
	private Text low;

	[SerializeField]
	private InputField buoyName;

	[SerializeField]
	private Image isobarScale;

	[SerializeField]
	private CapacityIndicator BuoysCount;

	[SerializeField]
	private GameObject AddButton;

	[SerializeField]
	private GameObject AddButtonHelp;

	[SerializeField]
	private GamePadIconLocalize AddButtonHelpLoc;

	[SerializeField]
	private GameObject BuyMoreBuoysButton;

	[SerializeField]
	private GameObject OpenShareRequests;

	private InGameMap.MapState mapState;

	public static InventoryItem Lure;

	public static CaughtFish FishInfo;

	private float mapAspectRatio;

	private WeatherWidget weatherWidget;

	private MapRenderer mapRenderer;

	private MarkPlacer locationPlacer;

	private MarkPlacer usersPlacer;

	private MarkPlacer markPlacer;

	private MarkPlacer missionObjectsPlacer;

	private MarkPlacer rodPodsPlacer;

	private MarkPlacer backGroundPlacer;

	private MapInfoPanel mapInfoPanel;

	private float _desiredAngle;

	private float timer;

	private float angle;

	private float north;

	private float timerMax = 1f;

	private Vector2 sLeft = new Vector2(-1500f, 0f);

	private Vector2 lLeft = new Vector2(-1070f, 0f);

	private Vector2 sRight = new Vector2(210f, 0f);

	private Vector2 lRight = new Vector2(-210f, 0f);

	[Space(15f)]
	public Color friend = Color.green;

	[Space(15f)]
	public Color notFriend = Color.gray;

	[Space(15f)]
	public Color self = Color.blue;

	[Space(15f)]
	private bool North = true;

	private Dictionary<int, GameObject> buoys = new Dictionary<int, GameObject>();

	private GameObject buoyparent;

	private int buoyNumber = 1;

	private int currentBuoyId = -1;

	private Toggle _currentMark;

	private MenuHelpers _helpers = new MenuHelpers();

	private GameObject MessageBox;

	private IsobarScale isobarScaleUI;

	private bool ignoreControls;

	private Vector3 mapBgInitialScale;

	private List<PlayerRecord> _players = new List<PlayerRecord>();

	private LayerMask terrainMask;

	private const float deepestRaycast = 100f;

	[SerializeField]
	private UINavigation filtersNavigation;

	[SerializeField]
	private GameObject[] objectsToHideOnConsoles;

	private int _visibleLayer;

	public Font PcFont;

	public Font GamePadFont;

	private List<BuoySetting> _buoys;

	public Text CreatedTime;

	public Text TogglePosition;

	private Dictionary<int, Toggle> _toggles;

	private StringBuilder sb = new StringBuilder(100);

	private const string formt = "[ {0:0.00} , {1:0.00} ]";

	private Toggle _pendingToggle;

	private Vector3 currentCoords = default(Vector3);

	private Toggle _currentMagnetedToggle;

	private Vector2 _magnetPosition;

	private float minRadius = 0.025f;

	private float moveSpeed = 800f;

	private CursorType prevCursor;

	private bool isDragged;

	[SerializeField]
	private GameObject EditingButtons;

	[SerializeField]
	private GameObject ControlButtons;

	private string _currentBuoyName;

	private bool _editingMode;

	private enum MapState
	{
		Exploring,
		BuoyMark
	}
}
