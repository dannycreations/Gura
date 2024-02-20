using System;
using System.Collections.Generic;
using System.Text;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatInGameController : MonoBehaviour
{
	internal void Start()
	{
		this.MessagesTextPanel.text = string.Empty;
		if (this.toggles != null)
		{
			this.toggles[this._currentToggle].isOn = true;
		}
		PhotonConnectionFactory.Instance.OnGotChatPlayersCount += this.OnGotChatPlayersCount;
		PhotonConnectionFactory.Instance.OnGettingChatPlayersCountFailed += this.OnGettingChatPlayersCountFailed;
		PhotonConnectionFactory.Instance.GetChatPlayersCount();
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotChatPlayersCount -= this.OnGotChatPlayersCount;
		PhotonConnectionFactory.Instance.OnGettingChatPlayersCountFailed -= this.OnGettingChatPlayersCountFailed;
	}

	internal void Update()
	{
		this.timerSyncUserCount += Time.deltaTime;
		if (this.timerSyncUserCount >= this.timerSyncUserCountMax)
		{
			this.timerSyncUserCount = 0f;
			PhotonConnectionFactory.Instance.GetChatPlayersCount();
		}
		this.TabAllCaption.text = string.Format("{0} ({1})", ScriptLocalization.Get("ChatAllCaption"), StaticUserData.ChatController.PlayerOnLocaionCount);
		string text = string.Empty;
		switch (this._currentToggle)
		{
		case 0:
			text = this.GetText(StaticUserData.ChatController.GetMessages(MessageChatType.All));
			break;
		case 1:
			text = this.GetText(StaticUserData.ChatController.GetMessages(MessageChatType.Private));
			break;
		case 2:
			text = this.GetText(StaticUserData.ChatController.GetMessages(MessageChatType.Friends));
			break;
		case 3:
			text = this.GetText(StaticUserData.ChatController.GetMessages(MessageChatType.Club));
			break;
		}
		if (this.MessagesTextPanel.text.GetHashCode() != text.GetHashCode())
		{
			this.MessagesTextPanel.text = text;
		}
		if (!this._chatIsActive && EventSystem.current.currentSelectedGameObject == this.InputPanel.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
		if (!this.LockChat && ControlsController.ControlsActions.Chat.WasPressedMandatory)
		{
			if (ControlsController.ControlsActions.IsBlockedKeyboardInput)
			{
				this._chatIsActive = false;
				this.ExitFromChatMode();
			}
			else
			{
				this.InputPanel.interactable = true;
				this._chatIsActive = true;
				ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Fire1", "Fire2" });
				this.InputPanel.ActivateInputField();
				EventSystem.current.SetSelectedGameObject(this.InputPanel.gameObject);
				foreach (Image image in this.BackgroundTabImage)
				{
					image.color = this.ActiveTabColor;
				}
				this.BackgroundImage.color = this.ActiveColor;
				this.ChangeMessagePrefix();
			}
		}
		if (this._chatIsActive)
		{
			EventSystem.current.SetSelectedGameObject(this.InputPanel.gameObject);
		}
		if (!this.LockChat && ControlsController.ControlsActions.ChatScrollUp.WasPressedMandatory && this.scrollBar != null && this.scrollBar.gameObject.activeSelf)
		{
			this.scrollBar.value -= this.scrollBar.size;
			this._inEndText = Math.Abs(this.scrollBar.value - 1f) < 0.001f;
		}
		if (!this.LockChat && ControlsController.ControlsActions.ChatScrollDown.WasPressedMandatory && this.scrollBar != null && this.scrollBar.gameObject.activeSelf)
		{
			this.scrollBar.value += this.scrollBar.size;
			this._inEndText = Math.Abs(this.scrollBar.value - 1f) < 0.001f;
		}
		if (!this.LockChat && ControlsController.ControlsActions.IsBlockedKeyboardInput && ControlsController.ControlsActions.ChatChangeTabToRight.WasPressedMandatory)
		{
			this._currentToggle++;
			if (this._currentToggle >= this.toggles.Count)
			{
				this._currentToggle = 0;
			}
			this.toggles[this._currentToggle].isOn = true;
			this.ChangeMessagePrefix();
		}
		if (!this.LockChat && ControlsController.ControlsActions.IsBlockedKeyboardInput && ControlsController.ControlsActions.ChatChangeTabToLeft.WasPressedMandatory)
		{
			this._currentToggle--;
			if (this._currentToggle < 0)
			{
				this._currentToggle = this.toggles.Count - 1;
			}
			this.toggles[this._currentToggle].isOn = true;
			this.ChangeMessagePrefix();
		}
		if (!this.LockChat && ControlsController.ControlsActions.IsBlockedKeyboardInput && ControlsController.ControlsActions.UICancel.WasPressedMandatory)
		{
			this._chatIsActive = false;
			this.ExitFromChatMode();
			this.InputPanel.text = string.Empty;
			foreach (Image image2 in this.BackgroundTabImage)
			{
				image2.color = this.InActiveTabColor;
			}
			this.BackgroundImage.color = this.InActiveColor;
			ControlsController.ControlsActions.UnBlockInput();
		}
		if (!this.LockChat && ControlsController.ControlsActions.IsBlockedKeyboardInput && ControlsController.ControlsActions.UISubmit.WasPressedMandatory)
		{
			this.SubmitMessage();
		}
	}

	public void SubmitMessage()
	{
		if (string.IsNullOrEmpty(this.InputPanel.text))
		{
			return;
		}
		if (this.InputPanel.text.StartsWith("/"))
		{
			this.HandleSpecialCommand();
		}
		else
		{
			this.HandleNormalMessage();
		}
	}

	private void HandleSpecialCommand()
	{
		string text = this.InputPanel.text;
		string[] array = this.InputPanel.text.Split(new char[] { ' ' });
		string text2 = array[0].Substring(1);
		string text3 = null;
		if (array.Length >= 2)
		{
			text3 = string.Join(" ", array.SubArray(1, array.Length - 1));
		}
		string text4 = text2.ToLower();
		if (text4 != null)
		{
			if (text4 == "ignore")
			{
				if (!string.IsNullOrEmpty(text3))
				{
					PhotonConnectionFactory.Instance.SendChatCommand(2, text3);
				}
				else
				{
					text = ScriptLocalization.Get("ChatCommandIncorrect");
				}
				goto IL_16D;
			}
			if (text4 == "unignore")
			{
				if (!string.IsNullOrEmpty(text3))
				{
					PhotonConnectionFactory.Instance.SendChatCommand(3, text3);
				}
				else
				{
					text = ScriptLocalization.Get("ChatCommandIncorrect");
				}
				goto IL_16D;
			}
			if (text4 == "report")
			{
				if (!string.IsNullOrEmpty(text3))
				{
					PhotonConnectionFactory.Instance.SendChatCommand(1, text3);
				}
				else
				{
					text = ScriptLocalization.Get("ChatCommandIncorrect");
				}
				goto IL_16D;
			}
			if (text4 == "incognito")
			{
				PhotonConnectionFactory.Instance.SendChatCommand(4, null);
				goto IL_16D;
			}
			if (text4 == "help")
			{
				text = "/ignore Username \n /unignore Username \n /report Username \n /incognito ";
				goto IL_16D;
			}
		}
		text = ScriptLocalization.Get("ChatCommandIncorrect");
		IL_16D:
		StaticUserData.ChatController.AddMessage(string.Format("<color=cyan>{0}</color>", text), MessageChatType.Global, true);
		this.InputPanel.text = string.Empty;
		this.InputPanel.ActivateInputField();
		EventSystem.current.SetSelectedGameObject(this.InputPanel.gameObject);
	}

	private void HandleNormalMessage()
	{
		ChatMessage chatMessage = new ChatMessage
		{
			Sender = new Player
			{
				UserId = PhotonConnectionFactory.Instance.Profile.UserId.ToString(),
				UserName = PhotonConnectionFactory.Instance.Profile.Name
			},
			Message = this.InputPanel.text
		};
		MessageChatType messageChatType = MessageChatType.Location;
		chatMessage.Channel = "Local";
		switch (this._currentToggle)
		{
		case 1:
		{
			messageChatType = MessageChatType.Private;
			chatMessage.Channel = string.Empty;
			chatMessage.Recepient = new Player();
			string[] array = chatMessage.Message.Split(new char[] { ':' }, 2);
			if (array.Length < 2)
			{
				this.InputPanel.text = string.Empty;
				this.InputPanel.ActivateInputField();
				EventSystem.current.SetSelectedGameObject(this.InputPanel.gameObject);
				return;
			}
			this.InputPanel.ActivateInputField();
			EventSystem.current.SetSelectedGameObject(this.InputPanel.gameObject);
			chatMessage.Recepient.UserName = array[0].Substring(1);
			chatMessage.Message = array[1];
			chatMessage.Channel = "Private";
			this.InputPanel.text = array[0] + ":";
			this.InputPanel.MoveTextEnd(true);
			break;
		}
		case 2:
			return;
		case 3:
			messageChatType = MessageChatType.Club;
			chatMessage.Channel = "Club";
			this.InputPanel.text = string.Empty;
			this.InputPanel.ActivateInputField();
			EventSystem.current.SetSelectedGameObject(this.InputPanel.gameObject);
			break;
		default:
			this.InputPanel.text = string.Empty;
			this.InputPanel.ActivateInputField();
			EventSystem.current.SetSelectedGameObject(this.InputPanel.gameObject);
			break;
		}
		StaticUserData.ChatController.SendMessage(chatMessage, messageChatType);
	}

	private void ExitFromChatMode()
	{
		foreach (Image image in this.BackgroundTabImage)
		{
			image.color = this.InActiveTabColor;
		}
		this.BackgroundImage.color = this.InActiveColor;
		ControlsController.ControlsActions.UnBlockKeyboard();
		this.InputPanel.text = string.Empty;
		this.InputPanel.interactable = false;
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}

	public void OnChangeText()
	{
		if (this.InputPanel.text.Length == 0)
		{
			this._currentToggle = 0;
			this.toggles[this._currentToggle].isOn = true;
			return;
		}
		char c = this.InputPanel.text[0];
		switch (c)
		{
		case '#':
			this._currentToggle = 1;
			this.toggles[this._currentToggle].isOn = true;
			break;
		default:
			if (c != '@')
			{
				this._currentToggle = 0;
				this.toggles[this._currentToggle].isOn = true;
			}
			break;
		case '%':
			break;
		}
	}

	private string GetText(IEnumerable<ChatMessage> messages)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ChatMessage chatMessage in messages)
		{
			if (chatMessage.Sender.UserId == PhotonConnectionFactory.Instance.Profile.UserId.ToString())
			{
				stringBuilder.AppendFormat("<color=green><b>{0}</b>:</color> {1}\n", this.GetSelfName(chatMessage), chatMessage.Message);
			}
			else if (chatMessage.Sender.UserId == "-1")
			{
				stringBuilder.AppendFormat("{0}\n", chatMessage.Message);
			}
			else
			{
				stringBuilder.AppendFormat("<color=cyan><b>{0}</b>:</color> {1}\n", this.GetName(chatMessage), chatMessage.Message);
			}
			if (this.scrollBar != null && this._inEndText)
			{
				this.scrollBar.value = 1f;
			}
		}
		return stringBuilder.ToString();
	}

	private string GetName(ChatMessage message)
	{
		string channel = message.Channel;
		if (channel != null)
		{
			if (channel == "Private")
			{
				return string.Format("{0} <color=white>[<i>{1}</i>]</color>", message.Sender.UserName, ScriptLocalization.Get("ChatPrivateCaption").ToLower());
			}
			if (channel == "Local")
			{
				return message.Sender.UserName;
			}
			if (channel == "Club")
			{
				return message.Sender.UserName;
			}
			if (channel == "Friends")
			{
				return message.Sender.UserName;
			}
			if (channel == "Global")
			{
				return message.Sender.UserName;
			}
			if (channel == "Tournament")
			{
				return message.Sender.UserName;
			}
		}
		return message.Sender.UserName;
	}

	private string GetSelfName(ChatMessage message)
	{
		string channel = message.Channel;
		if (channel != null)
		{
			if (channel == "Private")
			{
				return string.Format("{0} <color=white>[<i>{1}</i>]</color>", PhotonConnectionFactory.Instance.Profile.Name, ScriptLocalization.Get("ChatPrivateCaption").ToLower());
			}
			if (channel == "Local")
			{
				return PhotonConnectionFactory.Instance.Profile.Name;
			}
			if (channel == "Club")
			{
				return PhotonConnectionFactory.Instance.Profile.Name;
			}
			if (channel == "Friends")
			{
				return PhotonConnectionFactory.Instance.Profile.Name;
			}
			if (channel == "Global")
			{
				return PhotonConnectionFactory.Instance.Profile.Name;
			}
			if (channel == "Tournament")
			{
				return PhotonConnectionFactory.Instance.Profile.Name;
			}
		}
		return PhotonConnectionFactory.Instance.Profile.Name;
	}

	private void ChangeMessagePrefix()
	{
		int currentToggle = this._currentToggle;
		if (currentToggle != 0)
		{
			if (currentToggle == 1)
			{
				this.InputPanel.text = "#";
				this.InputPanel.MoveTextEnd(false);
			}
		}
		else
		{
			this.InputPanel.text = string.Empty;
		}
	}

	internal void OnEnable()
	{
		PhotonConnectionFactory.Instance.GetChatPlayersCount();
	}

	private void OnGotChatPlayersCount(int peerCount)
	{
		StaticUserData.ChatController.PlayerOnLocaionCount = peerCount;
	}

	private void OnGettingChatPlayersCountFailed(Failure failure)
	{
	}

	private int _currentToggle;

	private bool _inEndText = true;

	private bool _chatIsActive;

	public Text MessagesTextPanel;

	public InputField InputPanel;

	public Scrollbar scrollBar;

	public List<Toggle> toggles;

	public Image BackgroundImage;

	public List<Image> BackgroundTabImage;

	public Color ActiveColor;

	public Color InActiveColor;

	public Color ActiveTabColor;

	public Color InActiveTabColor;

	public Text TabAllCaption;

	[HideInInspector]
	public bool LockChat;

	private float timerSyncUserCount;

	private float timerSyncUserCountMax = 300f;
}
