using System;
using DG.Tweening;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubMenuController : MonoBehaviour
{
	private void Awake()
	{
		this._toggleGroup = base.GetComponent<ToggleGroup>();
		for (int i = 0; i < this.submenuToggles.Length; i++)
		{
			this.submenuToggles[i].onValueChanged.AddListener(new UnityAction<bool>(this.onChanged));
		}
	}

	private void OnDisable()
	{
		this.CloseSubMenuFast();
	}

	private void OnDestroy()
	{
		for (int i = 0; i < this.submenuToggles.Length; i++)
		{
			this.submenuToggles[i].onValueChanged.RemoveListener(new UnityAction<bool>(this.onChanged));
		}
	}

	private void ChangeSwitchOnServer(GameSwitchType type, bool value)
	{
		if (StaticUserData.IS_IN_TUTORIAL)
		{
			return;
		}
		PhotonConnectionFactory.Instance.ChangeSwitch(type, value, null);
	}

	public void OpenSubMenu(RectTransform next)
	{
		if (next == null)
		{
			return;
		}
		if (this._opennedSubMenu != null)
		{
			if (!this._inProgress)
			{
				this._inProgress = true;
				this._openNext = ((!(next == this._opennedSubMenu)) ? next : null);
				TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._opennedSubMenu, new Vector2(-this._formWidth, 0f), this._duration, true), 8), delegate
				{
					this._inProgress = false;
					this._opennedSubMenu.gameObject.SetActive(false);
					this._opennedSubMenu = null;
					this.OpenSubMenu(this._openNext);
				});
				if (this._opennedSubMenu == this.FishContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondFishSpecies, false);
				}
				else if (this._opennedSubMenu == this.PondContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondInfo, false);
				}
				else if (this._opennedSubMenu == this.WeatherContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondWeather, false);
				}
				else if (this._opennedSubMenu == this.LicenseContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenLicenseInfo, false);
				}
			}
			else
			{
				this._openNext = next;
			}
		}
		else if (!this._inProgress)
		{
			this._inProgress = true;
			this._opennedSubMenu = next;
			this._openNext = null;
			this._opennedSubMenu.gameObject.SetActive(true);
			TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._opennedSubMenu, new Vector2(0f, 0f), this._duration, true), 8), delegate
			{
				this._inProgress = false;
				this.OpenSubMenu(this._openNext);
			});
			if (PhotonConnectionFactory.Instance.IsConnectedToGameServer)
			{
				if (this._opennedSubMenu == this.FishContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondFishSpecies, true);
				}
				else if (this._opennedSubMenu == this.PondContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondInfo, true);
				}
				else if (this._opennedSubMenu == this.WeatherContent)
				{
					this.ChangeSwitchOnServer(GameSwitchType.OpenPondWeather, true);
				}
				else if (this._opennedSubMenu == this.LicenseContent)
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

	public void CloseSubMenu()
	{
		if (!this._inProgress)
		{
			this._inProgress = true;
			TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOAnchorPos(this._opennedSubMenu, new Vector2(-this._formWidth, 0f), this._duration, true), 8), delegate
			{
				this._inProgress = false;
				this._opennedSubMenu.gameObject.SetActive(false);
				this._opennedSubMenu = null;
				this.OpenSubMenu(this._openNext);
			});
		}
	}

	public void CloseSubMenuFast()
	{
		if (this.closePanelsButton != null)
		{
			this.closePanelsButton.onClick.Invoke();
		}
	}

	private void onChanged(bool ison)
	{
		if (EventSystem.current != null)
		{
			this.Update();
		}
	}

	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < this.submenuObjects.Length; i++)
		{
			if (EventSystem.current.currentSelectedGameObject == this.submenuObjects[i])
			{
				flag = true;
				break;
			}
		}
		bool flag2 = this._toggleGroup.AnyTogglesOn() || flag;
		if (!this._toggleGroup.AnyTogglesOn() && this._opennedSubMenu != null)
		{
			this.CloseSubMenu();
		}
		if (!this._wasAnyToggleOn && flag2)
		{
			this._wasAnyToggleOn = flag2;
			if (this.toggleOnButton != null)
			{
				this.toggleOnButton.OnSubmit(new BaseEventData(EventSystem.current));
			}
		}
		if (this._wasAnyToggleOn && !flag2)
		{
			this._wasAnyToggleOn = flag2;
			if (this.toggleOffButton != null)
			{
				this.toggleOffButton.OnSubmit(new BaseEventData(EventSystem.current));
			}
		}
	}

	private RectTransform _opennedSubMenu;

	private RectTransform _openNext;

	private float _formWidth = 1360f;

	private float _duration = 0.25f;

	private bool _inProgress;

	private ToggleGroup _toggleGroup;

	private bool _wasAnyToggleOn;

	[SerializeField]
	private Button toggleOnButton;

	[SerializeField]
	private Button toggleOffButton;

	[SerializeField]
	private Button closePanelsButton;

	[SerializeField]
	private GameObject[] submenuObjects;

	[SerializeField]
	private Toggle[] submenuToggles;

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
}
