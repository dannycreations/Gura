using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using Scripts.Common;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ActivityState))]
public class DashboardTabSetter : MonoBehaviour
{
	public static bool IsNewPremShopEnabled
	{
		get
		{
			return CacheLibrary.MapCache.IsAbTestActive(Constants.AB_TESTS.NEW_PREMIUM_SHOP_IMPLEMETATION);
		}
	}

	public static bool IsTutorialSlidesEnabled
	{
		get
		{
			return ChangeLanguage.GetCurrentLanguage.Lang != CustomLanguages.Italian;
		}
	}

	private void Awake()
	{
		DashboardTabSetter.Instance = this;
		PhotonConnectionFactory.Instance.OnLevelGained += this.OnLevelGained;
		this._activityState = base.GetComponent<ActivityState>();
		this._changer = base.GetComponent<ChangeFormByName>() ?? base.gameObject.AddComponent<ChangeFormByName>();
		if (this.ToggleGroup == null)
		{
			this.ToggleGroup = this._activityState.ActionObject.GetComponent<ToggleGroup>() ?? this._activityState.ActionObject.AddComponent<ToggleGroup>();
		}
		if (this.BumperControl == null)
		{
			this.BumperControl = this._activityState.ActionObject.GetComponent<BumperControl>() ?? this._activityState.ActionObject.AddComponent<BumperControl>();
		}
		this.ToggleGroup.allowSwitchOff = false;
		bool flag = StaticUserData.CurrentPond != null;
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < this.MenuMapping.Length; i++)
		{
			DashboardTabSetter.ToggleFormPair pair = this.MenuMapping[i];
			if (pair.form == FormsEnum.TutorialsList && !DashboardTabSetter.IsTutorialSlidesEnabled)
			{
				pair.toggle.gameObject.SetActive(false);
			}
			else if ((pair.form == FormsEnum.Quests && StaticUserData.DISABLE_MISSIONS) || (pair.form == FormsEnum.Sport && !DashboardTabSetter.IsSportEnabled) || (pair.form == FormsEnum.Tournaments && DashboardTabSetter.IsSportEnabled))
			{
				pair.toggle.gameObject.SetActive(false);
			}
			else if ((pair.form != FormsEnum.LocalMap || flag) && (pair.form != FormsEnum.GlobalMap || !flag))
			{
				if (pair.form == FormsEnum.PremiumShopRetail)
				{
					pair.toggle.gameObject.SetActive(false);
				}
				else
				{
					if (pair.form == FormsEnum.PremiumShop)
					{
					}
					if (pair.form == FormsEnum.Sport)
					{
						this.SetActiveSport(pair.toggle);
					}
					list.Add(pair.toggle);
					pair.toggle.group = this.ToggleGroup;
					pair.toggle.onValueChanged.AddListener(delegate(bool isOn)
					{
						if (!this._isActive)
						{
							return;
						}
						if (this.SoundEffects != null)
						{
							this.SoundEffects.OnSetToogle(isOn);
						}
						if (!isOn)
						{
							return;
						}
						if (pair.form == FormsEnum.Sport)
						{
							UgcMenuStateManager ugcMenuManager = MenuHelpers.Instance.MenuPrefabsList.UgcMenuManager;
							UgcMenuStateManager.UgcStates ugcState = ugcMenuManager.UgcState;
							if (ugcState == UgcMenuStateManager.UgcStates.Sport)
							{
								this._changer.Change(pair.form, this._immediate);
							}
							else
							{
								this._changer.Change(pair.form, true);
								ugcMenuManager.SetActiveFormSport(UgcMenuStateManager.UgcStates.Sport, false, true);
								ugcMenuManager.SetActiveFormSport(ugcState, true, false);
							}
						}
						else
						{
							this._changer.Change(pair.form, this._immediate);
						}
						if (pair.form == FormsEnum.PremiumShop)
						{
							if (this._isEnableCaptureActionInStats)
							{
								PhotonConnectionFactory.Instance.CaptureActionInStats("OpenGameScreen", pair.form.ToString(), (!DashboardTabSetter.IsNewPremShopEnabled) ? "old" : "new", "0");
							}
							this._isEnableCaptureActionInStats = true;
							if (!DashboardTabSetter.IsNewPremShopEnabled)
							{
								ShopMainPageHandler.OpenPremiumShop();
							}
						}
						else if (pair.form == FormsEnum.Shop)
						{
							ShopMainPageHandler.OpenMainShopPage();
						}
					});
				}
			}
		}
		this.BumperControl.SetSelectables(list.ToArray());
		this.mmList = new List<DashboardTabSetter.ToggleFormPair>(DashboardTabSetter.Instance.MenuMapping);
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnLevelGained -= this.OnLevelGained;
	}

	public static DashboardTabSetter.ToggleFormPair GetMenuTogglePairByForm(FormsEnum form)
	{
		return DashboardTabSetter.Instance.mmList.FirstOrDefault((DashboardTabSetter.ToggleFormPair pair) => pair.form == form);
	}

	public static void ForceSwitch(ActivityState activityState)
	{
		DashboardTabSetter.ToggleFormPair menuTogglePairByForm = DashboardTabSetter.GetMenuTogglePairByForm(activityState.FormType);
		if (menuTogglePairByForm.toggle != null)
		{
			menuTogglePairByForm.toggle.group.allowSwitchOff = true;
			menuTogglePairByForm.toggle.group.SetAllTogglesOff();
			menuTogglePairByForm.toggle.isOn = true;
			menuTogglePairByForm.toggle.group.allowSwitchOff = false;
		}
	}

	public static void SwitchForms(FormsEnum form, bool immediate = false)
	{
		if (DashboardTabSetter.Instance == null)
		{
			MenuHelpers.Instance.MenuPrefabsList.topMenuForm.GetComponent<DashboardTabSetter>().Awake();
		}
		DashboardTabSetter.Instance.ToggleGroup.allowSwitchOff = true;
		for (int i = 0; i < DashboardTabSetter.Instance.MenuMapping.Length; i++)
		{
			DashboardTabSetter.Instance.MenuMapping[i].toggle.isOn = false;
		}
		DashboardTabSetter.ToggleFormPair menuTogglePairByForm = DashboardTabSetter.GetMenuTogglePairByForm(form);
		if (menuTogglePairByForm.toggle == null)
		{
			Debug.LogError("Unregistered menu form transfer called: " + form);
			return;
		}
		DashboardTabSetter.Instance._immediate = immediate;
		menuTogglePairByForm.toggle.isOn = true;
		DashboardTabSetter.Instance._immediate = false;
		DashboardTabSetter.Instance.ToggleGroup.allowSwitchOff = false;
	}

	public void SetActive(bool flag)
	{
		this._isActive = flag;
	}

	public void SetEnableCaptureActionInStats(bool flag)
	{
		this._isEnableCaptureActionInStats = flag;
	}

	public void SetAllTogglesInteractable(bool flag)
	{
		for (int i = 0; i < this.MenuMapping.Length; i++)
		{
			Toggle toggle = this.MenuMapping[i].toggle;
			toggle.interactable = flag;
			Button component = toggle.GetComponent<Button>();
			if (component != null)
			{
				component.interactable = flag;
			}
		}
	}

	private void OnLevelGained(LevelInfo level)
	{
		for (int i = 0; i < this.MenuMapping.Length; i++)
		{
			DashboardTabSetter.ToggleFormPair toggleFormPair = this.MenuMapping[i];
			if (toggleFormPair.form == FormsEnum.Sport)
			{
				this.SetActiveSport(toggleFormPair.toggle);
				break;
			}
		}
	}

	private void SetActiveSport(Toggle t)
	{
		t.interactable = this.IsSportAvailableByLevel;
		MainDashboardUnlockInfo component = t.GetComponent<MainDashboardUnlockInfo>();
		component.SetActive(!t.interactable);
		if (!t.interactable)
		{
			component.SetLvlInfo(string.Format(ScriptLocalization.Get("UnlockedAtLevelCaption"), 3));
		}
	}

	private bool IsSportAvailableByLevel
	{
		get
		{
			return PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Level >= 3;
		}
	}

	[SerializeField]
	public DashboardTabSetter.ToggleFormPair[] MenuMapping;

	private List<DashboardTabSetter.ToggleFormPair> mmList;

	public BumperControl BumperControl;

	public ToggleGroup ToggleGroup;

	public PlayButtonEffect SoundEffects;

	public static DashboardTabSetter Instance;

	private ChangeFormByName _changer;

	private ActivityState _activityState;

	private bool _immediate;

	private bool _isActive = true;

	private bool _isEnableCaptureActionInStats = true;

	public static bool IsSportEnabled = true;

	public const int SportUnlockLevel = 3;

	[Serializable]
	public struct ToggleFormPair
	{
		public FormsEnum form;

		public Toggle toggle;
	}
}
