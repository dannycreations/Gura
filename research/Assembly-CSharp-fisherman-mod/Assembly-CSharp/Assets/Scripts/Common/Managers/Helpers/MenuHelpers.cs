using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Common.Managers.Helpers
{
	public class MenuHelpers
	{
		public MenuPrefabsList MenuPrefabsList
		{
			get
			{
				if (this._menuPrefabsList == null)
				{
					GameObject gameObject = GameObject.Find(StaticUserData.GameObjectCommonDataName);
					if (gameObject == null)
					{
						return null;
					}
					this._menuPrefabsList = gameObject.GetComponent<MenuPrefabsList>();
				}
				return this._menuPrefabsList;
			}
		}

		public MessageBoxList MessageBoxList
		{
			get
			{
				if (this._infoMessagesController == null)
				{
					GameObject gameObject = GameObject.Find("InfoMessagesController");
					if (gameObject == null)
					{
						return null;
					}
					this._infoMessagesController = gameObject.GetComponent<MessageBoxList>();
				}
				return this._infoMessagesController;
			}
		}

		public static MenuHelpers Instance
		{
			get
			{
				MenuHelpers menuHelpers;
				if ((menuHelpers = MenuHelpers._instance) == null)
				{
					menuHelpers = (MenuHelpers._instance = new MenuHelpers());
				}
				return menuHelpers;
			}
		}

		public bool IsInMenu
		{
			get
			{
				return PondControllers.Instance == null || PondControllers.Instance.IsInMenu;
			}
		}

		public void KillCamera()
		{
			if (this._guiCamera != null)
			{
				Object.Destroy(this._guiCamera.gameObject);
				this._guiCamera = null;
			}
		}

		public Camera GUICamera
		{
			get
			{
				if (this._guiCamera == null)
				{
					this._guiCamera = Camera.allCameras.FirstOrDefault((Camera x) => x.CompareTag("GUICamera"));
				}
				return this._guiCamera;
			}
		}

		public void SetEnabledGUICamera(bool flag)
		{
			if (this.GUICamera != null)
			{
				LogHelper.Log("___kocha SetEnabledGUICamera flag:{0}", new object[] { flag });
				this.GUICamera.enabled = flag;
			}
		}

		public void HideMenu(bool disableGuiCamera = true, bool hideGameBg = true, bool hideMenuList = true)
		{
			if (hideGameBg)
			{
				ManagerScenes.Instance.Background.SetVisibility(false);
			}
			if (hideMenuList)
			{
				this.MenuPrefabsList.HideMenu();
			}
			this.MenuPrefabsList.SetMenuParentVisibility(false);
			if (disableGuiCamera)
			{
				this.SetEnabledGUICamera(false);
			}
		}

		public void ShowMenu(bool enableGuiCamera = true, bool showGameBg = true)
		{
			if (showGameBg)
			{
				ManagerScenes.Instance.Background.SetVisibility(true);
			}
			if (enableGuiCamera)
			{
				this.SetEnabledGUICamera(true);
			}
			this.MenuPrefabsList.SetMenuParentVisibility(true);
		}

		public void SkipTutorial()
		{
			if (!this.skipSubscribed)
			{
				this.skipSubscribed = true;
				PhotonConnectionFactory.Instance.OnTutorialSkipped += this.Instance_OnTutorialSkipped;
				PhotonConnectionFactory.Instance.OnSkipTutorialFailed += this.Instance_OnSkipTutorialFailed;
			}
			PhotonConnectionFactory.Instance.SkipTutorial();
		}

		private void Instance_OnTutorialSkipped()
		{
			this.skipSubscribed = false;
			PhotonConnectionFactory.Instance.OnTutorialSkipped -= this.Instance_OnTutorialSkipped;
			PhotonConnectionFactory.Instance.OnSkipTutorialFailed -= this.Instance_OnSkipTutorialFailed;
			this.RestartGame();
		}

		private void Instance_OnSkipTutorialFailed(Failure fail)
		{
			this.skipSubscribed = false;
			PhotonConnectionFactory.Instance.OnTutorialSkipped -= this.Instance_OnTutorialSkipped;
			PhotonConnectionFactory.Instance.OnSkipTutorialFailed -= this.Instance_OnSkipTutorialFailed;
			Debug.LogError(fail.ErrorMessage);
		}

		public void RestartGame()
		{
			DisconnectServerAction.Instance.OnDisconnectContinueClick(false);
			FirstScreenInit.autoSkip = true;
		}

		public GameObject GetFormByName(FormsEnum name)
		{
			switch (name)
			{
			case FormsEnum.Start:
				return this.MenuPrefabsList.startForm;
			case FormsEnum.Login:
				return this.MenuPrefabsList.loginForm;
			case FormsEnum.PlayLogged:
				return this.MenuPrefabsList.playLoggedForm;
			case FormsEnum.PlayNotLogged:
				return this.MenuPrefabsList.playNotLoggedForm;
			case FormsEnum.Registration:
				return this.MenuPrefabsList.registrationForm;
			case FormsEnum.Loading:
				return this.MenuPrefabsList.loadingForm;
			case FormsEnum.TopDashboard:
				return this.MenuPrefabsList.topMenuForm;
			case FormsEnum.Options:
				return this.MenuPrefabsList.optionsForm;
			case FormsEnum.Inventory:
				return this.MenuPrefabsList.inventoryForm;
			case FormsEnum.Shop:
				return this.MenuPrefabsList.shopForm;
			case FormsEnum.GlobalMap:
				return this.MenuPrefabsList.globalMapForm;
			case FormsEnum.LocalMap:
				return this.MenuPrefabsList.globalMapForm;
			case FormsEnum.ExitForm:
				return this.MenuPrefabsList.exitMenuForm;
			case FormsEnum.Traveling:
				return this.MenuPrefabsList.travelingForm;
			case FormsEnum.Stats:
				return this.MenuPrefabsList.statisticsForm;
			case FormsEnum.Credits:
				return this.MenuPrefabsList.credits;
			case FormsEnum.Profile:
				return this.MenuPrefabsList.profileForm;
			case FormsEnum.FishKeepnet:
				return this.MenuPrefabsList.fishKeepnetForm;
			case FormsEnum.Leaderboards:
				return this.MenuPrefabsList.leaderboards;
			case FormsEnum.Friends:
				return this.MenuPrefabsList.friends;
			case FormsEnum.TutorialsList:
				return this.MenuPrefabsList.tutorialsList;
			case FormsEnum.Tournaments:
				return this.MenuPrefabsList.tournaments;
			case FormsEnum.Quests:
				return this.MenuPrefabsList.missions;
			case FormsEnum.PremiumShop:
				return (!DashboardTabSetter.IsNewPremShopEnabled) ? this.MenuPrefabsList.shopForm : this.MenuPrefabsList.shopPremiumForm;
			case FormsEnum.Sport:
				return this.MenuPrefabsList.SportForm;
			case FormsEnum.PremiumShopRetail:
				return this.MenuPrefabsList.PremiumShopRetailForm;
			}
			return null;
		}

		public ActivityState GetFormActivityStateByName(FormsEnum name)
		{
			switch (name)
			{
			case FormsEnum.Start:
				return this.MenuPrefabsList.startFormAS;
			case FormsEnum.Login:
				return this.MenuPrefabsList.loginFormAS;
			case FormsEnum.PlayLogged:
				return this.MenuPrefabsList.playLoggedFormAS;
			case FormsEnum.PlayNotLogged:
				return this.MenuPrefabsList.playNotLoggedFormAS;
			case FormsEnum.Registration:
				return this.MenuPrefabsList.registrationFormAS;
			case FormsEnum.Loading:
				return this.MenuPrefabsList.loadingFormAS;
			case FormsEnum.TopDashboard:
				return this.MenuPrefabsList.topMenuFormAS;
			case FormsEnum.Options:
				return this.MenuPrefabsList.optionsFormAS;
			case FormsEnum.Inventory:
				return this.MenuPrefabsList.inventoryFormAS;
			case FormsEnum.Shop:
				return this.MenuPrefabsList.shopFormAS;
			case FormsEnum.GlobalMap:
				return this.MenuPrefabsList.globalMapFormAS;
			case FormsEnum.LocalMap:
				return this.MenuPrefabsList.globalMapFormAS;
			case FormsEnum.ExitForm:
				return this.MenuPrefabsList.exitMenuFormAS;
			case FormsEnum.Traveling:
				return this.MenuPrefabsList.travelingFormAS;
			case FormsEnum.Stats:
				return this.MenuPrefabsList.statisticsFormAS;
			case FormsEnum.Credits:
				return this.MenuPrefabsList.creditsAS;
			case FormsEnum.Profile:
				return this.MenuPrefabsList.profileFormAS;
			case FormsEnum.FishKeepnet:
				return this.MenuPrefabsList.fishKeepnetFormAS;
			case FormsEnum.Leaderboards:
				return this.MenuPrefabsList.leaderboardsAS;
			case FormsEnum.Friends:
				return this.MenuPrefabsList.friendsAS;
			case FormsEnum.TutorialsList:
				return this.MenuPrefabsList.tutorialsListAS;
			case FormsEnum.Tournaments:
				return this.MenuPrefabsList.tournamentsAS;
			case FormsEnum.Quests:
				return this.MenuPrefabsList.missionsAS;
			case FormsEnum.PremiumShop:
				return (!DashboardTabSetter.IsNewPremShopEnabled) ? this.MenuPrefabsList.shopFormAS : this.MenuPrefabsList.shopPremiumFormAS;
			case FormsEnum.Sport:
				return this.MenuPrefabsList.SportAS;
			case FormsEnum.PremiumShopRetail:
				return this.MenuPrefabsList.PremiumShopRetailFormAS;
			}
			return null;
		}

		public void ShowForm(GameObject panel, bool immediate = false)
		{
			ActivityState component = panel.GetComponent<ActivityState>();
			if (component != null)
			{
				this.ShowForm(component, false);
			}
			else
			{
				CanvasAlphaTween component2 = panel.GetComponent<CanvasAlphaTween>();
				if (component2 != null)
				{
					panel.SetActive(true);
					component2.From = 0f;
					component2.To = 1f;
					component2.Duration = ((!immediate) ? 0.25f : 0f);
					component2.Play();
				}
			}
		}

		public void ShowForm(ActivityState activityState, bool immediate = false)
		{
			activityState.Show(immediate);
		}

		public void ChangeFormAction(GameObject currentPanel, GameObject nextPanel, bool showDashboard = true, bool immediate = false)
		{
			this.ChangeFormAction((!(currentPanel == null)) ? currentPanel.GetComponent<ActivityState>() : null, nextPanel.GetComponent<ActivityState>(), showDashboard, immediate);
		}

		public void ChangeFormAction(ActivityState currentPanel, ActivityState nextPanel, bool showDashboard = true, bool immediate = false)
		{
			if (currentPanel != null)
			{
				currentPanel.NextPanelName = nextPanel.name;
				this.HideForm(currentPanel, immediate);
				currentPanel.NextPanelName = null;
			}
			if (showDashboard != StaticUserData.IsShowDashboard && this.MenuPrefabsList.topMenuFormAS != null)
			{
				if (showDashboard)
				{
					this.ShowForm(this.MenuPrefabsList.topMenuFormAS, immediate);
				}
				else
				{
					this.HideForm(this.MenuPrefabsList.topMenuFormAS, immediate);
				}
				if (this.MenuPrefabsList.helpPanelAS != null)
				{
					if (showDashboard)
					{
						this.ShowForm(this.MenuPrefabsList.helpPanelAS, immediate);
					}
					else
					{
						this.HideForm(this.MenuPrefabsList.helpPanelAS, immediate);
					}
				}
			}
			nextPanel.PrevPanelName = ((!(currentPanel != null)) ? null : currentPanel.name);
			this.ShowForm(nextPanel, immediate);
			nextPanel.PrevPanelName = null;
		}

		public void HideForm(GameObject panel, bool immediate = false)
		{
			ActivityState component = panel.GetComponent<ActivityState>();
			if (component)
			{
				component.Hide(immediate);
			}
			else
			{
				CanvasAlphaTween component2 = panel.GetComponent<CanvasAlphaTween>();
				if (component2 != null)
				{
					panel.SetActive(true);
					component2.From = 1f;
					component2.To = 0f;
					component2.Duration = ((!immediate) ? 0.25f : 0f);
					component2.Play();
				}
			}
		}

		public void HideForm(ActivityState activityState, bool immediate = false)
		{
			if (activityState)
			{
				activityState.Hide(immediate);
			}
			else
			{
				CanvasAlphaTween component = activityState.GetComponent<CanvasAlphaTween>();
				if (component != null)
				{
					component.gameObject.SetActive(true);
					component.From = 1f;
					component.To = 0f;
					component.Duration = ((!immediate) ? 0.25f : 0f);
					component.Play();
				}
			}
		}

		public void StartGameAction(ActivityState currentActivePanel)
		{
			if (this.MenuPrefabsList.loadingForm.GetComponent<GotoGlobalMap>() == null)
			{
				ActivityState loadingFormAS = this.MenuPrefabsList.loadingFormAS;
				loadingFormAS.gameObject.AddComponent<GotoGlobalMap>();
				this.ChangeFormAction(currentActivePanel, loadingFormAS, true, false);
			}
		}

		public MessageBoxBase ShowSearchSelectorMessage(List<IScrollSelectorElement> data, int selectedIndex, RectTransform selectorTransform, string placeholderText, Action<int> onPick)
		{
			GameObject gameObject = GUITools.AddChild(InfoMessageController.Instance.gameObject, this.MessageBoxList.ScrollSearchSelector);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			ScrollSelectorMessageBox component = gameObject.GetComponent<ScrollSelectorMessageBox>();
			component.Init(data, selectedIndex, selectorTransform, placeholderText, onPick);
			return component;
		}

		public MessageBox ShowMessage(GameObject parent, string caption, string message, bool onPriority = false, bool autoClose = false, bool playFail = false, Action closeAction = null)
		{
			if (InfoMessageController.Instance != null)
			{
				parent = InfoMessageController.Instance.gameObject;
			}
			GameObject gameObject = GUITools.AddChild(parent, this.MessageBoxList.messageBoxPrefab);
			MessageBox mb = gameObject.GetComponent<MessageBox>();
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			mb.Caption = caption;
			mb.Message = message;
			mb.OnPriority = onPriority;
			mb.ConfirmButtonText = ScriptLocalization.Get("CloseButton");
			if (playFail)
			{
				UIAudioSourceListener.Instance.Fail();
			}
			if (autoClose)
			{
				gameObject.GetComponent<EventAction>().ActionCalled += delegate(object ea, EventArgs o)
				{
					if (closeAction != null)
					{
						closeAction();
					}
					mb.Close();
				};
			}
			return mb;
		}

		public MessageBox ShowMessage(GameObject parent, string caption, string message, string confirmText, bool onPriority)
		{
			if (InfoMessageController.Instance != null)
			{
				parent = InfoMessageController.Instance.gameObject;
			}
			MessageBox messageBox = this.ShowMessage(parent, caption, message, onPriority, false, false, null);
			messageBox.ConfirmButtonText = confirmText;
			return messageBox;
		}

		public MessageBox ShowMessageSelectable(GameObject parent, string caption, string message, bool onPriority = false)
		{
			if (InfoMessageController.Instance != null)
			{
				parent = InfoMessageController.Instance.gameObject;
			}
			GameObject gameObject = GUITools.AddChild(parent, this.MessageBoxList.messageBoxSelectablePrefab);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			MessageBox component = gameObject.GetComponent<MessageBox>();
			component.Caption = caption;
			component.Message = message;
			component.OnPriority = onPriority;
			return component;
		}

		public MessageBox ShowMessageSelectable(GameObject parent, string caption, string message, string confirmText, string cancelText, bool disableConfirmButton = false, bool onPriority = false)
		{
			if (InfoMessageController.Instance != null)
			{
				parent = InfoMessageController.Instance.gameObject;
			}
			GameObject gameObject = GUITools.AddChild(parent, this.MessageBoxList.messageBoxSelectablePrefab);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			MessageBox component = gameObject.GetComponent<MessageBox>();
			component.Caption = caption;
			component.Message = message;
			component.ConfirmButtonText = confirmText;
			component.CancelButtonText = cancelText;
			component.OnPriority = onPriority;
			if (disableConfirmButton)
			{
				component.DisableConfirmButton();
			}
			return component;
		}

		public MessageBox ShowMessageSelectableWithoutButtons(GameObject parent, string caption, string message, bool onPriority = false)
		{
			if (InfoMessageController.Instance != null)
			{
				parent = InfoMessageController.Instance.gameObject;
			}
			GameObject gameObject = GUITools.AddChild(parent, this.MessageBoxList.messageBoxSelectableWithoutButtonsPrefab);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			MessageBox component = gameObject.GetComponent<MessageBox>();
			component.Caption = caption;
			component.Message = message;
			component.OnPriority = onPriority;
			return component;
		}

		public MessageBox ShowYesNo(string capt, string message, Action actionCalled, string confirmButtonTextLocId = "YesCaption", Action cancelCalled = null, string cancelButtonLocId = "NoCaption", Action afterHideAcceptAction = null, Action afterHideCancelAction = null, GameObject prefab = null)
		{
			GameObject gameObject = InfoMessageController.Instance.gameObject;
			if (prefab == null)
			{
				prefab = this.MessageBoxList.messageBoxSelectablePrefab;
			}
			GameObject gameObject2 = GUITools.AddChild(gameObject, prefab);
			RectTransform component = gameObject2.GetComponent<RectTransform>();
			component.anchoredPosition = Vector3.zero;
			component.sizeDelta = Vector2.zero;
			MessageBox mb = gameObject2.GetComponent<MessageBox>();
			mb.Caption = ((!string.IsNullOrEmpty(capt)) ? capt : ScriptLocalization.Get("ConfirmSkipText"));
			mb.Message = message;
			mb.ConfirmButtonText = ScriptLocalization.Get(confirmButtonTextLocId);
			mb.CancelButtonText = ScriptLocalization.Get(cancelButtonLocId);
			mb.OnPriority = true;
			EventConfirmAction component2 = mb.GetComponent<EventConfirmAction>();
			component2.CancelActionCalled += delegate(object e, EventArgs obj)
			{
				if (afterHideCancelAction != null)
				{
					mb.AfterFullyHidden.AddListener(new UnityAction(afterHideCancelAction.Invoke));
				}
				mb.Close();
				if (cancelCalled != null)
				{
					cancelCalled();
				}
			};
			component2.ConfirmActionCalled += delegate(object e, EventArgs obj)
			{
				if (afterHideAcceptAction != null)
				{
					mb.AfterFullyHidden.AddListener(new UnityAction(afterHideAcceptAction.Invoke));
				}
				mb.Close();
				if (actionCalled != null)
				{
					actionCalled();
				}
			};
			return mb;
		}

		public MessageBox ShowFullDescriptionMessage(string message, bool onPriority = false)
		{
			GameObject gameObject = InfoMessageController.Instance.gameObject;
			GameObject gameObject2 = GUITools.AddChild(gameObject, this.MessageBoxList.ShowFullDescriptionPanel);
			MessageBox component = gameObject2.GetComponent<MessageBox>();
			gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject2.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			component.Caption = string.Empty;
			component.Message = message;
			component.OnPriority = onPriority;
			component.ConfirmButtonText = ScriptLocalization.Get("CloseButton");
			return component;
		}

		public GameObject ShowMessageThreeSelectable(GameObject parent, string caption, string message, string confirmText, string cancelText, string thirdActionText)
		{
			if (InfoMessageController.Instance != null)
			{
				parent = InfoMessageController.Instance.gameObject;
			}
			GameObject gameObject = GUITools.AddChild(parent, this.MessageBoxList.messageBoxThreeSelectablePrefab);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			MessageBox component = gameObject.GetComponent<MessageBox>();
			component.Message = message;
			component.Caption = caption;
			component.ConfirmButtonText = confirmText;
			component.CancelButtonText = cancelText;
			component.ThirdButtonText = thirdActionText;
			return gameObject;
		}

		public GameObject ShowBuyProductsOfTypeWindow(GameObject parent, string caption, ProductTypes type)
		{
			if (InfoMessageController.Instance != null)
			{
				parent = InfoMessageController.Instance.gameObject;
			}
			GameObject _messageBox = GUITools.AddChild(parent, MessageBoxList.Instance.ExpandStorageWindow);
			_messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			_messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			_messageBox.GetComponent<StorageBoxListInit>().Init(CacheLibrary.ProductCache.Products.Where((StoreProduct x) => x.TypeId == type).ToList<StoreProduct>());
			_messageBox.GetComponent<StorageBoxListInit>().TitleText.text = caption;
			_messageBox.GetComponent<StorageBoxListInit>().CloseButton.onClick.AddListener(delegate
			{
				_messageBox.GetComponent<AlphaFade>().HidePanel();
			});
			_messageBox.GetComponent<AlphaFade>().ShowPanel();
			return _messageBox;
		}

		public MessageBox ShowPlayerProfile(Profile profile, Action onClose)
		{
			GameObject gameObject = InfoMessageController.Instance.gameObject;
			MessageBox _messageBox = GUITools.AddChild(gameObject, MessageBoxList.Instance.messageBoxPlayerProfilePrefab).GetComponent<MessageBox>();
			_messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			_messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			_messageBox.GetComponent<InitPlayerProfile>().Init(profile, false);
			_messageBox.GetComponent<MessageBox>().CancelButtonText = ScriptLocalization.Get("CloseButton");
			_messageBox.GetComponent<EventAction>().ActionCalled += delegate(object e, EventArgs obj)
			{
				_messageBox.Close();
				if (onClose != null)
				{
					onClose();
				}
			};
			_messageBox.Open();
			return _messageBox;
		}

		public GameObject ShowBuoysDelivered(GameObject parent, BuoySetting buoy)
		{
			if (InfoMessageController.Instance != null)
			{
				parent = InfoMessageController.Instance.gameObject;
			}
			GameObject gameObject = GUITools.AddChild(parent, MessageBoxList.Instance.BuoyDelivered);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
			if (buoy == null)
			{
				buoy = PhotonConnectionFactory.Instance.Profile.BuoyShareRequests[0];
			}
			BuoyDeliveredInit component = gameObject.GetComponent<BuoyDeliveredInit>();
			component.Init(buoy, false);
			component.Open();
			return gameObject;
		}

		public void ShowMessage(string capt, string text, bool onPriority = true, Action actionCalled = null, bool forceShow = false)
		{
			GameObject gameObject = this.ShowMessage(null, capt, text, false, false, false, null).gameObject;
			MessageBox msgBox = gameObject.GetComponent<MessageBox>();
			msgBox.OnPriority = onPriority;
			gameObject.GetComponent<EventAction>().ActionCalled += delegate(object go, EventArgs args)
			{
				msgBox.Close();
				if (actionCalled != null)
				{
					actionCalled();
				}
			};
			if (forceShow)
			{
				msgBox.Open();
			}
			else
			{
				MessageBoxList.Instance.Show();
			}
		}

		public void ChumMixing(ChumIngredient ingredient, Func<InventoryItem, InventoryItem, bool> transferItemFunc)
		{
			GameObject gameObject = this.PreparePanel(MessageBoxList.Instance.ChumMixPrefab, InfoMessageController.Instance.gameObject);
			Mixing component = gameObject.GetComponent<Mixing>();
			if (component.Init(ingredient, transferItemFunc))
			{
				component.MessageType = InfoMessageTypes.ChumMixing;
				MessageFactory.InfoMessagesQueue.Enqueue(component);
			}
			else
			{
				Object.Destroy(gameObject);
			}
		}

		private GameObject PreparePanel(InfoMessage im, GameObject root)
		{
			GameObject gameObject = GUITools.AddChild(root, im.gameObject);
			gameObject.transform.localScale = Vector3.zero;
			gameObject.SetActive(false);
			gameObject.GetComponent<AlphaFade>().FastHidePanel();
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.anchoredPosition = Vector3.zero;
			component.sizeDelta = Vector2.zero;
			return gameObject;
		}

		private MenuPrefabsList _menuPrefabsList;

		private MessageBoxList _infoMessagesController;

		private static MenuHelpers _instance;

		private Camera _guiCamera;

		private bool skipSubscribed;
	}
}
