using System;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using UnityEngine;

public class MenuPrefabsList : MonoBehaviour
{
	public ActivityState currentActiveFormAS
	{
		get
		{
			return (!(this.currentActiveForm != null)) ? null : this.currentActiveForm.GetComponent<ActivityState>();
		}
	}

	public UgcMenuStateManager UgcMenuManager { get; private set; }

	public void Awake()
	{
		this.loginFormAS = ((!(this.loginForm != null)) ? null : this.loginForm.GetComponent<ActivityState>());
		this.playLoggedFormAS = ((!(this.playLoggedForm != null)) ? null : this.playLoggedForm.GetComponent<ActivityState>());
		this.playNotLoggedFormAS = ((!(this.playNotLoggedForm != null)) ? null : this.playNotLoggedForm.GetComponent<ActivityState>());
		this.registrationFormAS = ((!(this.registrationForm != null)) ? null : this.registrationForm.GetComponent<ActivityState>());
		this.settingsOnStartFormAS = ((!(this.settingsOnStartForm != null)) ? null : this.settingsOnStartForm.GetComponent<ActivityState>());
		this.topMenuFormAS = ((!(this.topMenuForm != null)) ? null : this.topMenuForm.GetComponent<ActivityState>());
		this.exitMenuFormAS = ((!(this.exitMenuForm != null)) ? null : this.exitMenuForm.GetComponent<ActivityState>());
		this.optionsFormAS = ((!(this.optionsForm != null)) ? null : this.optionsForm.GetComponent<ActivityState>());
		this.inventoryFormAS = ((!(this.inventoryForm != null)) ? null : this.inventoryForm.GetComponent<ActivityState>());
		this.shopFormAS = ((!(this.shopForm != null)) ? null : this.shopForm.GetComponent<ActivityState>());
		this.shopPremiumFormAS = ((!(this.shopPremiumForm != null)) ? null : this.shopPremiumForm.GetComponent<ActivityState>());
		this.globalMapFormAS = ((!(this.globalMapForm != null)) ? null : this.globalMapForm.GetComponent<ActivityState>());
		this.statisticsFormAS = ((!(this.statisticsForm != null)) ? null : this.statisticsForm.GetComponent<ActivityState>());
		this.HudUIAS = ((!(this.HudUI != null)) ? null : this.HudUI.GetComponent<ActivityState>());
		this.creditsAS = ((!(this.credits != null)) ? null : this.credits.GetComponent<ActivityState>());
		this.profileFormAS = ((!(this.profileForm != null)) ? null : this.profileForm.GetComponent<ActivityState>());
		this.fishKeepnetFormAS = ((!(this.fishKeepnetForm != null)) ? null : this.fishKeepnetForm.GetComponent<ActivityState>());
		this.leaderboardsAS = ((!(this.leaderboards != null)) ? null : this.leaderboards.GetComponent<ActivityState>());
		this.missionsAS = ((!(this.missions != null)) ? null : this.missions.GetComponent<ActivityState>());
		this.friendsAS = ((!(this.friends != null)) ? null : this.friends.GetComponent<ActivityState>());
		this.tutorialsListAS = ((!(this.tutorialsList != null)) ? null : this.tutorialsList.GetComponent<ActivityState>());
		this.tournamentsAS = ((!(this.tournaments != null)) ? null : this.tournaments.GetComponent<ActivityState>());
		this.helpPanelAS = ((!(this.helpPanel != null)) ? null : this.helpPanel.GetComponent<ActivityState>());
		this.SportAS = ((!(this.SportForm != null)) ? null : this.SportForm.GetComponent<ActivityState>());
		this.CreateTournamentFormAS = ((!(this.CreateTournamentForm != null)) ? null : this.CreateTournamentForm.GetComponent<ActivityState>());
		this.UgcRoomFormAS = ((!(this.UgcRoomForm != null)) ? null : this.UgcRoomForm.GetComponent<ActivityState>());
		this.PremiumShopRetailFormAS = ((!(this.PremiumShopRetailForm != null)) ? null : this.PremiumShopRetailForm.GetComponent<ActivityState>());
		this.UgcMenuManager = new UgcMenuStateManager(this);
		if (this.MainMenuCanvasGroup == null)
		{
			PondControllers component = base.GetComponent<PondControllers>();
			Transform transform = ((!(component != null)) ? base.transform.parent : component.PondMainMenu.transform);
			this.MainMenuCanvasGroup = transform.GetComponent<CanvasGroup>();
			if (this.MainMenuCanvasGroup == null)
			{
				this.MainMenuCanvasGroup = transform.gameObject.AddComponent<CanvasGroup>();
			}
		}
		this.Inited = true;
	}

	private void Start()
	{
		this.firstForm = ManagerScenes.Instance.FirstScreenForm.gameObject;
		this.startFormAS = ManagerScenes.Instance.StartForm;
		this.startForm = this.startFormAS.gameObject;
		this.travelingFormAS = ManagerScenes.Instance.TravelingForm;
		this.travelingForm = this.travelingFormAS.gameObject;
		this.loadingFormAS = ManagerScenes.Instance.LoadingForm;
		this.loadingForm = this.loadingFormAS.gameObject;
	}

	public PremiumShopRetailHandler PremiumShopRetailHandler
	{
		get
		{
			return (!(this.PremiumShopRetailFormAS != null)) ? null : this.PremiumShopRetailFormAS.GetComponent<PremiumShopRetailHandler>();
		}
	}

	public bool IsLoadingOrTransfer()
	{
		bool flag = (this.loadingFormAS == null || !this.loadingFormAS.isActive) && (this.travelingFormAS == null || !this.travelingFormAS.isActive) && (this.startFormAS == null || !this.startFormAS.isActive);
		return !flag || (!(GameFactory.Player != null) && (StaticUserData.CurrentPond != null || !(this.globalMapForm != null)) && (!(GameFactory.Player == null) || StaticUserData.CurrentPond == null));
	}

	public void SetMenuParentVisibility(bool visible)
	{
		this.MainMenuCanvasGroup.alpha = ((!visible) ? 0f : 1f);
		this.MainMenuCanvasGroup.blocksRaycasts = visible;
		this.MainMenuCanvasGroup.interactable = visible;
	}

	public void HideMenu()
	{
		if (this.IsLoadingFormActive)
		{
			this.loadingFormAS.Hide(true);
		}
		if (this.currentActiveForm != null)
		{
			this.currentActiveFormAS.Hide(true);
		}
		this.topMenuFormAS.Hide(false);
		if (this.helpPanelAS != null)
		{
			this.helpPanelAS.Hide(false);
		}
		if (this.HideCalled != null)
		{
			this.HideCalled();
		}
	}

	public bool IsLoadingFormActive
	{
		get
		{
			return this.loadingFormAS != null && this.loadingFormAS.isActive;
		}
	}

	public bool IsStartFormActive
	{
		get
		{
			return this.startFormAS != null && this.startFormAS.isActive;
		}
	}

	public bool IsTravelingFormActive
	{
		get
		{
			return this.travelingFormAS != null && this.travelingFormAS.isActive;
		}
	}

	public GameObject firstForm;

	public GameObject startForm;

	public GameObject loginForm;

	public GameObject playLoggedForm;

	public GameObject playNotLoggedForm;

	public GameObject registrationForm;

	public GameObject settingsOnStartForm;

	public GameObject loadingForm;

	public GameObject travelingForm;

	public GameObject topMenuForm;

	public GameObject exitMenuForm;

	public GameObject optionsForm;

	public GameObject inventoryForm;

	public GameObject shopForm;

	public GameObject shopPremiumForm;

	public GameObject globalMapForm;

	public GameObject statisticsForm;

	public GameObject MessagePanel;

	public GameObject HudUI;

	public GameObject credits;

	public GameObject profileForm;

	public GameObject fishKeepnetForm;

	public GameObject leaderboards;

	public GameObject missions;

	public GameObject friends;

	public GameObject tutorialsList;

	public GameObject tournaments;

	public GameObject helpPanel;

	public GameObject currentActiveForm;

	public GameObject SportForm;

	public GameObject CreateTournamentForm;

	public GameObject UgcRoomForm;

	public GameObject PremiumShopRetailForm;

	[HideInInspector]
	public bool Inited;

	[HideInInspector]
	public ActivityState startFormAS;

	[HideInInspector]
	public ActivityState loginFormAS;

	[HideInInspector]
	public ActivityState playLoggedFormAS;

	[HideInInspector]
	public ActivityState playNotLoggedFormAS;

	[HideInInspector]
	public ActivityState registrationFormAS;

	[HideInInspector]
	public ActivityState settingsOnStartFormAS;

	[HideInInspector]
	public ActivityState loadingFormAS;

	[HideInInspector]
	public ActivityState travelingFormAS;

	[HideInInspector]
	public ActivityState topMenuFormAS;

	[HideInInspector]
	public ActivityState exitMenuFormAS;

	[HideInInspector]
	public ActivityState optionsFormAS;

	[HideInInspector]
	public ActivityState inventoryFormAS;

	[HideInInspector]
	public ActivityState shopFormAS;

	[HideInInspector]
	public ActivityState shopPremiumFormAS;

	[HideInInspector]
	public ActivityState globalMapFormAS;

	[HideInInspector]
	public ActivityState statisticsFormAS;

	[HideInInspector]
	public ActivityState HudUIAS;

	[HideInInspector]
	public ActivityState creditsAS;

	[HideInInspector]
	public ActivityState profileFormAS;

	[HideInInspector]
	public ActivityState fishKeepnetFormAS;

	[HideInInspector]
	public ActivityState leaderboardsAS;

	[HideInInspector]
	public ActivityState missionsAS;

	[HideInInspector]
	public ActivityState friendsAS;

	[HideInInspector]
	public ActivityState tutorialsListAS;

	[HideInInspector]
	public ActivityState tournamentsAS;

	[HideInInspector]
	public ActivityState helpPanelAS;

	[HideInInspector]
	public ActivityState SportAS;

	[HideInInspector]
	public ActivityState CreateTournamentFormAS;

	[HideInInspector]
	public ActivityState UgcRoomFormAS;

	[HideInInspector]
	public ActivityState PremiumShopRetailFormAS;

	public CanvasGroup MainMenuCanvasGroup;

	public Action HideCalled;
}
