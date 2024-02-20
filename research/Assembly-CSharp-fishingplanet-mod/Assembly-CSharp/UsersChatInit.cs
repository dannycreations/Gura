using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UsersChatInit : MenuBase
{
	private void Update()
	{
		int num = StaticUserData.ChatController.PlayerOnLocation.Values.Aggregate(0, (int x, Player y) => x.GetHashCode() ^ y.GetHashCode());
		if (num != this._hashCodeOriginal)
		{
			this.Init();
		}
		else if (StaticUserData.ChatController.PlayerOnLocation.Values.Count == 0 && base.IsActive && !this._isEmpty)
		{
			this.AddDummy();
		}
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this.CheckPanel();
		}
	}

	private void Init()
	{
		this.Clear();
		Dictionary<string, Player>.ValueCollection values = StaticUserData.ChatController.PlayerOnLocation.Values;
		this._hashCodeOriginal = values.Aggregate(0, (int x, Player y) => x.GetHashCode() ^ y.GetHashCode());
		foreach (Player player in values)
		{
			GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.UserChatItemPrefab);
			gameObject.GetComponent<UserChatHandler>().Init(player, this.BackgroundPanel, false, false);
			this._buttons.Add(gameObject.GetComponent<Button>());
			this._chatHandlers.Add(gameObject.GetComponent<UserChatHandler>());
		}
		this._isEmpty = false;
		if (values.Count == 0)
		{
			this.AddDummy();
		}
	}

	public void UpdateVoice(string uName, bool isMuted, bool isTalking)
	{
		UserChatHandler userChatHandler = this._chatHandlers.FirstOrDefault((UserChatHandler p) => p.UserName == uName);
		if (userChatHandler != null)
		{
			userChatHandler.UpdateVoice(isMuted, isTalking);
		}
	}

	protected override void CheckPanel()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (EventSystem.current.currentSelectedGameObject == this.toogle.gameObject)
			{
				if (!this.toogle.isOn)
				{
					this.ActivePanel(true);
				}
			}
			else
			{
				UserChatHandler userChatHandler = this._chatHandlers.FirstOrDefault((UserChatHandler item) => item.MenuPanel != null);
				if (userChatHandler == null && this._menuWasOpenned)
				{
					if (!this.Navigation.enabled)
					{
						this.Navigation.enabled = true;
					}
					this.Navigation.SelectLast();
					this._menuWasOpenned = false;
				}
				if (userChatHandler != null && !this._menuWasOpenned)
				{
					this._menuWasOpenned = true;
					userChatHandler.MenuPanel.GetComponent<UINavigation>().SetFirstUpperActive();
					this.Navigation.enabled = false;
				}
				if (this.toogle.isOn)
				{
					if (!(EventSystem.current.currentSelectedGameObject == null))
					{
						if (this._buttons.Any((Button button) => button.gameObject == EventSystem.current.currentSelectedGameObject))
						{
							return;
						}
					}
					Button[] array = ((!(userChatHandler != null)) ? new Button[0] : userChatHandler.MenuPanel.GetComponentsInChildren<Button>());
					if (!(userChatHandler == null))
					{
						if (array.Any((Button button) => button.gameObject == EventSystem.current.currentSelectedGameObject))
						{
							return;
						}
					}
					this.ActivePanel(false);
					this._menuWasOpenned = false;
					for (int i = 0; i < this._chatHandlers.Count; i++)
					{
						this._chatHandlers[i].ResetCounters();
					}
				}
			}
		}
	}

	protected override void ActivePanel(bool flag)
	{
		if (!flag)
		{
			this.Clear();
		}
		base.ActivePanel(flag);
		this.toogle.isOn = flag;
	}

	private void Clear()
	{
		this._hashCodeOriginal = 0;
		this._isEmpty = false;
		this._buttons.Clear();
		this._chatHandlers.Clear();
		for (int i = 0; i < this.ContentPanel.transform.childCount; i++)
		{
			Object.Destroy(this.ContentPanel.transform.GetChild(i).gameObject);
		}
	}

	private void AddDummy()
	{
		this._isEmpty = true;
		GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.UserChatItemPrefab);
		gameObject.GetComponent<UserChatHandler>().Init(new Player
		{
			UserName = ScriptLocalization.Get("ListIsEmpty")
		}, this.BackgroundPanel, false, true);
		this._buttons.Add(gameObject.GetComponent<Button>());
	}

	private int _hashCodeOriginal;

	public GameObject UserChatItemPrefab;

	public GameObject ContentPanel;

	public Scrollbar Scrollbar;

	public GameObject BackgroundPanel;

	public Toggle toogle;

	public UINavigation Navigation;

	private List<Button> _buttons = new List<Button>();

	private List<UserChatHandler> _chatHandlers = new List<UserChatHandler>();

	private bool _menuWasOpenned;

	private bool _isEmpty;
}
