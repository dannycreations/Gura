using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FriendsListItemInit : FriendListItemBase
{
	private void Update()
	{
		if (this.optionsPanelOpened)
		{
			if (this._myButton != null)
			{
				this._myButton.Highlight();
			}
		}
		else if (this._myButton != null)
		{
			this._myButton.Deselect();
		}
		if (this._friendToggle != null && !this._friendToggle.isOn)
		{
			if (this.optionsPanelOpened)
			{
				this.HideOptionsPanel();
			}
			if (this._myButton != null)
			{
				this._myButton.Deselect();
			}
		}
	}

	private void OnEnable()
	{
		this._friendToggle = this._helpers.MenuPrefabsList.topMenuForm.transform.Find("Image/TopMenuRight/Friends").GetComponent<Toggle>();
		PhotonConnectionFactory.Instance.OnReceiveBuoyShareRequest += this.OnBuoyReceived;
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted += this.UpdateCounter;
		PhotonConnectionFactory.Instance.OnBuoyShareDeclined += this.UpdateCounter;
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		Button button = ((!this.OpenDeliveryButton.activeInHierarchy) ? null : this.OpenDeliveryButton.GetComponent<Button>());
		if (button != null)
		{
			button.interactable = !base.transform.IsChildOf(InfoMessageController.Instance.transform) || type == InputModuleManager.InputType.Mouse;
		}
	}

	private void OnDisable()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		PhotonConnectionFactory.Instance.OnReceiveBuoyShareRequest -= this.OnBuoyReceived;
		PhotonConnectionFactory.Instance.OnBuoyShareAccepted -= this.UpdateCounter;
		PhotonConnectionFactory.Instance.OnBuoyShareDeclined -= this.UpdateCounter;
	}

	private void OnBuoyReceived(BuoySetting buoy)
	{
		this.UpdateCounter();
	}

	private void UpdateCounter()
	{
		if (this.OpenDeliveryButton != null && PhotonConnectionFactory.Instance.Profile.BuoyShareRequests != null)
		{
			this.OpenDeliveryButton.SetActive(PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.Any((BuoySetting x) => x.Sender == this._player.UserName));
		}
	}

	public void Init(Player player, GameObject friendsContentPanel, UINavigation rootNavigation)
	{
		this._player = player;
		this.FriendsContentPanel = friendsContentPanel;
		base.Init(this._player, this.FriendsContentPanel);
		this.UpdatePlayerInfo();
		this._rootNavigation = rootNavigation;
		this._myButton = base.GetComponent<BorderedButton>();
		this._myToggle = base.GetComponent<Toggle>();
		this.UpdateCounter();
		this.OnInputTypeChanged(SettingsManager.InputType);
	}

	public void OpenDeliveryWindow()
	{
		MenuHelpers.Instance.ShowBuoysDelivered(null, PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.FirstOrDefault((BuoySetting x) => x.Sender == this._player.UserName));
	}

	public void UpdatePlayerInfo()
	{
		if (this._player.IsOnline)
		{
			this.Status.GetComponent<Image>().color = new Color(0.27058825f, 0.6627451f, 0.29803923f);
		}
		else
		{
			this.Status.GetComponent<Image>().color = new Color(0.24313726f, 0.24313726f, 0.24313726f);
		}
		Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.PondId == this._player.PondId);
		if (this._player.IsOnline && pond != null)
		{
			this.Location.text = pond.Name + ", " + pond.State.Name;
		}
		else
		{
			this.Location.text = string.Empty;
		}
	}

	public void ShowOptionsPanel()
	{
		if (this.FriendOptionsPanel != null)
		{
			return;
		}
		if (this.FriendsContentPanel == null || this.FriendOptionsPanelPrefab == null)
		{
			Debug.LogErrorFormat("FriendsContentPanel: {0};  FriendOptionsPanelPrefab:{1}", new object[] { this.FriendsContentPanel, this.FriendOptionsPanelPrefab });
			return;
		}
		this.optionsPanelOpened = true;
		this.FriendOptionsPanel = GUITools.AddChild(this.FriendsContentPanel.transform.parent.gameObject, this.FriendOptionsPanelPrefab);
		FriendOptionsPanelInit component = this.FriendOptionsPanel.GetComponent<FriendOptionsPanelInit>();
		component.Init(this._player);
		component.OnActive += delegate(bool b)
		{
			if (b != this.optionsPanelOpened)
			{
				this.optionsPanelOpened = b;
				if (!b)
				{
					this.HideOptionsPanel();
				}
			}
		};
		LayoutRebuilder.ForceRebuildLayoutImmediate(component.transform as RectTransform);
		RectTransform rectTransform = component.transform as RectTransform;
		Vector3 vector = this.OptionsButton.GetComponent<RectTransform>().position;
		vector += new Vector3(-12f, -20f, 0f);
		Vector3 vector2 = this.FriendsContentPanel.transform.InverseTransformPoint(vector);
		Vector3 vector3 = vector2 - Vector3.up * rectTransform.rect.height;
		if (!(this.FriendsContentPanel.transform as RectTransform).rect.Contains(vector3))
		{
			vector2 += new Vector3(0f, rectTransform.rect.height, 0f);
		}
		this.FriendOptionsPanel.GetComponent<RectTransform>().position = this.FriendsContentPanel.transform.TransformPoint(vector2);
		this.FriendOptionsPanel.GetComponent<UINavigation>().SetFirstUpperActive();
		this._rootNavigation.enabled = false;
	}

	public void HideOptionsPanel()
	{
		this.optionsPanelOpened = false;
		InfoMessageController.Instance.StartCoroutine(this.ClosePanelDelayed());
		this._rootNavigation.enabled = true;
		this._rootNavigation.SelectLast();
	}

	private IEnumerator ClosePanelDelayed()
	{
		yield return new WaitForSeconds(0.1f);
		if (this.FriendOptionsPanel != null && !this.optionsPanelOpened)
		{
			Object.Destroy(this.FriendOptionsPanel.gameObject);
			this.FriendOptionsPanel = null;
		}
		yield break;
	}

	private void AddToChat()
	{
		if (!StaticUserData.ChatController.OpenPrivates.Any((Player x) => x.UserId == this._player.UserId))
		{
			StaticUserData.ChatController.OpenPrivates.Add(this._player);
		}
		if (StaticUserData.CurrentLocation != null)
		{
			KeysHandlerAction.EscapeHandler(false);
		}
		else
		{
			MessageBox messageBox = this._helpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ChatOnLocationText"), false, false, false, null);
			messageBox.GetComponent<EventAction>().ActionCalled += delegate(object sender, EventArgs args)
			{
				messageBox.Close();
			};
		}
	}

	public Image Status;

	public Text Location;

	private GameObject FriendOptionsPanel;

	public GameObject FriendOptionsPanelPrefab;

	public GameObject OptionsButton;

	public bool optionsPanelOpened;

	private UINavigation _rootNavigation;

	private MenuHelpers _helpers = new MenuHelpers();

	private Toggle _friendToggle;

	private BorderedButton _myButton;

	private Toggle _myToggle;

	public GameObject OpenDeliveryButton;
}
