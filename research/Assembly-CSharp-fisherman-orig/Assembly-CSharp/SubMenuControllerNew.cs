using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SubMenuControllerNew : ActivityStateControlled
{
	public FishSubMenu FishMenu
	{
		get
		{
			return this.FishContent.GetComponent<FishSubMenu>();
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnActive = delegate(bool b)
	{
	};

	public bool IsTravelSubMenuOpened
	{
		get
		{
			return this._opennedSubMenu != null && this._opennedSubMenu.Toggle.gameObject.name == "TravelDetails";
		}
	}

	protected override void Start()
	{
		base.Start();
		foreach (SubMenuControllerNew.SubMenuToggleRectPair subMenuToggleRectPair in this.bindings)
		{
			if (subMenuToggleRectPair.Foldout.Navigation != null)
			{
				UINavigation uinavigation = subMenuToggleRectPair.Foldout.Navigation;
				uinavigation.OnLeftEdgeReached = (Action<Selectable>)Delegate.Combine(uinavigation.OnLeftEdgeReached, new Action<Selectable>(this.LeftEdgeReached));
			}
		}
	}

	private void Awake()
	{
		this._toggleGroup = base.GetComponent<ToggleGroup>();
		this._toggleGroup.allowSwitchOff = true;
		if (this.navigation == null)
		{
			this.navigation = base.GetComponent<UINavigation>();
		}
		if (this.hotkeys == null)
		{
			this.hotkeys = base.GetComponent<HotkeyPressRedirect>();
		}
		if (this.navigation.Selectables.Length == 0)
		{
			this.navigation.PreheatSelectables();
		}
		this.bindingsList = new List<SubMenuControllerNew.SubMenuToggleRectPair>(this.bindings);
		this.navigationList = new List<Selectable>(this.navigation.Selectables);
		for (int i = 0; i < this.bindings.Length; i++)
		{
			SubMenuControllerNew.SubMenuToggleRectPair bind = this.bindings[i];
			bind.Foldout.SetFocused(false);
			Toggle toggle = bind.Toggle;
			PointerActionHandler pointerActionHandler = toggle.GetComponent<PointerActionHandler>() ?? toggle.gameObject.AddComponent<PointerActionHandler>();
			if (pointerActionHandler.PointerClick == null)
			{
				pointerActionHandler.PointerClick = new UnityEvent();
			}
			if (pointerActionHandler.OnSelected == null)
			{
				pointerActionHandler.OnSelected = new UnityEvent();
			}
			if (pointerActionHandler.OnDeselected == null)
			{
				pointerActionHandler.OnDeselected = new UnityEvent();
			}
			if (pointerActionHandler.PointerEnter == null)
			{
				pointerActionHandler.PointerEnter = new UnityEvent();
			}
			if (pointerActionHandler.PointerExit == null)
			{
				pointerActionHandler.PointerExit = new UnityEvent();
			}
			pointerActionHandler.OnSelected.RemoveAllListeners();
			pointerActionHandler.OnSelected.AddListener(delegate
			{
				this.TabsSounds.OnSelect(null);
				this.SetNext(bind);
				if (this.ShouldOpen && SettingsManager.InputType == InputModuleManager.InputType.GamePad)
				{
					toggle.isOn = true;
				}
				this.submitHotkey.enabled = true;
				this.hotkeys.enabled = true;
			});
			pointerActionHandler.OnDeselected.RemoveAllListeners();
			pointerActionHandler.OnDeselected.AddListener(delegate
			{
				this.submitHotkey.enabled = false;
			});
			toggle.group = this._toggleGroup;
			toggle.onValueChanged.AddListener(delegate(bool x)
			{
				MonoBehaviour.print(string.Format("{2}: toggle value changed: {0}, ignoreLogic: {1}", x, this.ignoreLogic, bind.Foldout.name));
				bind.Arrow.text = ((!x) ? "\ue786" : "\ue811");
				if (this.ignoreLogic)
				{
					this.ignoreLogic = false;
					if (this.setBack)
					{
						toggle.isOn = !x;
						this.setBack = false;
					}
					return;
				}
				this.TabsSounds.OnSetToogle(x);
				if (x)
				{
					this.SetNext(bind);
					this.OpenNext();
				}
				else if (this.nextToOpen == null || this.nextToOpen == this._opennedSubMenu)
				{
					this.CloseSubMenu();
					this.ShouldOpen = false;
					if (UINavigation.CurrentSelectedGo != null)
					{
						if (this.navigationList.Any((Selectable y) => y.gameObject == UINavigation.CurrentSelectedGo))
						{
							if (this.bindingsList.All((SubMenuControllerNew.SubMenuToggleRectPair z) => z.Toggle.gameObject != UINavigation.CurrentSelectedGo))
							{
								this.ShouldOpen = true;
							}
						}
					}
				}
			});
		}
		for (int j = 0; j < this.navigation.Selectables.Length; j++)
		{
			Selectable selectable = this.navigation.Selectables[j];
			if (!this.bindingsList.Any((SubMenuControllerNew.SubMenuToggleRectPair x) => x.Toggle == selectable))
			{
				PointerActionHandler pointerActionHandler2 = selectable.GetComponent<PointerActionHandler>() ?? selectable.gameObject.AddComponent<PointerActionHandler>();
				if (pointerActionHandler2.OnSelected == null)
				{
					pointerActionHandler2.OnSelected = new UnityEvent();
				}
				pointerActionHandler2.OnSelected.AddListener(delegate
				{
					this.SetNext(null);
					if (this._opennedSubMenu != null)
					{
						this._opennedSubMenu.Toggle.isOn = false;
					}
				});
			}
		}
	}

	public void SetNext(SubMenuControllerNew.SubMenuToggleRectPair next)
	{
		this.nextToOpen = next;
	}

	public void OpenNext()
	{
		if (this.nextToOpen == this._opennedSubMenu && this.nextToOpen != null)
		{
			if (this.nextToOpen.Foldout.HasContent())
			{
				this.GiveFocus();
			}
			else
			{
				UIAudioSourceListener.Instance.Fail();
			}
		}
		else
		{
			this.OpenSubMenu(this.nextToOpen);
		}
	}

	private void Update()
	{
		if (base.ShouldUpdate() && UINavigation.CurrentSelectedGo != null && this.navigation.enabled)
		{
			Selectable currsel = UINavigation.CurrentSelectedGo.GetComponent<Selectable>();
			if (this.navigation.Selectables.Contains(currsel) && this.bindingsList.All((SubMenuControllerNew.SubMenuToggleRectPair x) => x.Toggle != currsel))
			{
				this.CloseSubMenu();
			}
		}
	}

	private void ConfigureOpened()
	{
		this.ShouldOpen = true;
		this.TabsSounds.OnSetToogle(true);
		this.BackgroundNavigation.enabled = false;
		this.backClose.enabled = true;
		if (this.LeaveButton != null)
		{
			this.LeaveButton.enabled = false;
		}
	}

	private void ConfigureClosed()
	{
		this.BackgroundNavigation.enabled = true;
		this.backClose.enabled = false;
		this.hotkeys.enabled = true;
		if (this.LeaveButton != null)
		{
			this.LeaveButton.enabled = true;
		}
	}

	public void OpenSubMenu(SubMenuControllerNew.SubMenuToggleRectPair next)
	{
		if (next == null || next == this._opennedSubMenu)
		{
			return;
		}
		this.ConfigureOpened();
		if (this._opennedSubMenu != null)
		{
			if (!this._isBusy)
			{
				this._openNext = ((next != this._opennedSubMenu) ? next : null);
				this.CloseSubMenu();
			}
			else
			{
				this._openNext = next;
			}
		}
		else if (!this._isBusy)
		{
			this._isBusy = true;
			this._opennedSubMenu = next;
			this.OnActive(true);
			this._openNext = null;
			this._opennedSubMenu.Foldout.SetOpened(true, delegate
			{
				this._isBusy = false;
				if (this._openNext != null)
				{
					this.OpenSubMenu(this._openNext);
				}
			});
			if (PhotonConnectionFactory.Instance.IsConnectedToGameServer)
			{
				if (this._opennedSubMenu.Foldout.Content == this.FishContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondFishSpecies, true);
				}
				else if (this._opennedSubMenu.Foldout.Content == this.PondContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondInfo, true);
				}
				else if (this._opennedSubMenu.Foldout.Content == this.WeatherContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondWeather, true);
				}
				else if (this._opennedSubMenu.Foldout.Content == this.LicenseContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenLicenseInfo, true);
				}
			}
		}
		else
		{
			this._openNext = next;
		}
	}

	private void LeftEdgeReached(Selectable sel)
	{
		this.ReturnFocus();
	}

	public void GiveFocus()
	{
		if (this.nextToOpen.Foldout.HasContent())
		{
			this.nextToOpen.Foldout.SetFocused(true);
			this.navigation.enabled = false;
			this.hotkeys.enabled = false;
		}
	}

	public void ReturnFocus()
	{
		if (this._opennedSubMenu == null)
		{
			return;
		}
		this._opennedSubMenu.Foldout.SetFocused(false);
		this.navigation.enabled = true;
		this.hotkeys.enabled = true;
		if (this._openNext == null)
		{
			bool flag = true;
			if (UINavigation.CurrentSelectedGo != null)
			{
				List<Selectable> list = new List<Selectable>(this.navigation.Selectables);
				flag = !list.Contains(UINavigation.CurrentSelectedGo.GetComponent<Selectable>());
			}
			if (flag && SettingsManager.InputType == InputModuleManager.InputType.GamePad)
			{
				this._opennedSubMenu.Toggle.Select();
			}
			else if (this._opennedSubMenu.Toggle.isOn)
			{
				this.ignoreLogic = true;
				this._opennedSubMenu.Toggle.isOn = false;
			}
		}
	}

	public void CloseButton()
	{
		this.ShouldOpen = false;
		if (this._opennedSubMenu != null && this._opennedSubMenu.Toggle.isOn)
		{
			this._opennedSubMenu.Toggle.isOn = false;
		}
		else
		{
			this.CloseSubMenu();
		}
	}

	public void OpenButton()
	{
		if (this.nextToOpen == null)
		{
			this.CloseSubMenu();
			return;
		}
		this.ShouldOpen = true;
		if (this.nextToOpen != null && !this.nextToOpen.Toggle.isOn)
		{
			this.nextToOpen.Toggle.isOn = true;
		}
		else
		{
			this.OpenNext();
		}
	}

	public void OpenButtonSubmit()
	{
		this.ShouldOpen = true;
		if (this.nextToOpen == null || this.nextToOpen.Toggle.isOn)
		{
			this.ignoreLogic = true;
			this.setBack = true;
		}
	}

	public void CloseSubMenu()
	{
		if (this._opennedSubMenu == null)
		{
			return;
		}
		if (this._opennedSubMenu.Foldout.Content == this.FishContent)
		{
			this.ChangeSwitchOnServer(GameSwitchType.OpenPondFishSpecies, false);
		}
		else if (this._opennedSubMenu.Foldout.Content == this.PondContent)
		{
			this.ChangeSwitchOnServer(GameSwitchType.OpenPondInfo, false);
		}
		else if (this._opennedSubMenu.Foldout.Content == this.WeatherContent)
		{
			this.ChangeSwitchOnServer(GameSwitchType.OpenPondWeather, false);
		}
		else if (this._opennedSubMenu.Foldout.Content == this.LicenseContent)
		{
			this.ChangeSwitchOnServer(GameSwitchType.OpenLicenseInfo, false);
		}
		if (!this._isBusy)
		{
			this._isBusy = true;
			this.TabsSounds.OnSetToogle(false);
			this._opennedSubMenu.Foldout.SetOpened(false, delegate
			{
				this._isBusy = false;
				this.ReturnFocus();
				this._opennedSubMenu = null;
				this.OnActive(false);
				if (this._openNext == null)
				{
					this.ConfigureClosed();
				}
				else
				{
					this.OpenSubMenu(this._openNext);
				}
			});
		}
		else
		{
			this.TabsSounds.OnSetToogle(false);
			SubMenuControllerNew.SubMenuToggleRectPair opennedSubMenu = this._opennedSubMenu;
			this._opennedSubMenu.Foldout.ForceClose();
			this._opennedSubMenu = opennedSubMenu;
			this.ReturnFocus();
			this._opennedSubMenu = null;
			this.OnActive(false);
			if (this._openNext == null)
			{
				this.ConfigureClosed();
			}
			else
			{
				this.OpenSubMenu(this._openNext);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < this.bindings.Length; i++)
		{
			this.bindings[i].Toggle.onValueChanged.RemoveAllListeners();
		}
	}

	public void SetInteractableToggle<T>(bool isInteractable) where T : SubMenuFoldoutBase
	{
		SubMenuControllerNew.SubMenuToggleRectPair subMenuToggleRectPair = this.bindingsList.FirstOrDefault((SubMenuControllerNew.SubMenuToggleRectPair p) => p.Foldout is T);
		if (subMenuToggleRectPair != null)
		{
			subMenuToggleRectPair.Toggle.interactable = isInteractable;
			subMenuToggleRectPair.Arrow.color = ((!isInteractable) ? Color.grey : Color.white);
		}
	}

	protected override void HideHelp()
	{
		this.CloseSubMenu();
		this._toggleGroup.SetAllTogglesOff();
	}

	private void ChangeSwitchOnServer(GameSwitchType type, bool value)
	{
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		PhotonConnectionFactory.Instance.ChangeSwitch(type, value, null);
	}

	private const string CrossIcon = "\ue811";

	private const string ArrowIcon = "\ue786";

	[SerializeField]
	private PlayButtonEffect TabsSounds;

	private bool _isBusy;

	private ToggleGroup _toggleGroup;

	[SerializeField]
	private HotkeyPressRedirect hotkeys;

	[SerializeField]
	private HotkeyPressRedirect backClose;

	[SerializeField]
	private UINavigation navigation;

	[SerializeField]
	private SubMenuControllerNew.SubMenuToggleRectPair[] bindings;

	private SubMenuControllerNew.SubMenuToggleRectPair nextToOpen;

	private SubMenuControllerNew.SubMenuToggleRectPair _opennedSubMenu;

	private SubMenuControllerNew.SubMenuToggleRectPair _openNext;

	private List<SubMenuControllerNew.SubMenuToggleRectPair> bindingsList;

	private List<Selectable> navigationList;

	[SerializeField]
	private RectTransform PondContent;

	[SerializeField]
	private RectTransform WeatherContent;

	[SerializeField]
	private RectTransform FishContent;

	[SerializeField]
	private RectTransform LicenseContent;

	[SerializeField]
	private RectTransform RoomsContent;

	[SerializeField]
	private UINavigation BackgroundNavigation;

	[SerializeField]
	private HotkeyPressRedirect LeaveButton;

	[SerializeField]
	private HotkeyPressRedirect submitHotkey;

	[SerializeField]
	private bool ShouldOpen;

	private bool ignoreLogic;

	private bool setBack;

	[Serializable]
	public class SubMenuToggleRectPair
	{
		public Toggle Toggle;

		public SubMenuFoldoutBase Foldout;

		public TextMeshProUGUI Arrow;
	}
}
