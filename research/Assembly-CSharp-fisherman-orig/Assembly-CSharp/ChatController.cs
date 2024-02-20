using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ObjectModel;

public class ChatController
{
	private bool IsMuted
	{
		get
		{
			return SettingsManager.VoiceChatMuted;
		}
	}

	public void JoinChatChannel()
	{
		string currentChannelName = TournamentHelper.CurrentChannelName;
		if (!string.IsNullOrEmpty(currentChannelName) && currentChannelName != this._currentChannelName)
		{
			this._currentChannelName = currentChannelName;
			LogHelper.Log("___kocha JoinChatChannel :{0}", new object[] { currentChannelName });
			PhotonConnectionFactory.Instance.JoinChatChannel(currentChannelName);
		}
	}

	public void LeaveChatChannel()
	{
		if (!string.IsNullOrEmpty(this._currentChannelName))
		{
			LogHelper.Log("___kocha LeaveChatChannel :{0}", new object[] { this._currentChannelName });
			PhotonConnectionFactory.Instance.LeaveChatChannel(this._currentChannelName);
			this._currentChannelName = null;
			StaticUserData.ChatController.ClearMessages(MessageChatType.Tournament);
		}
	}

	public bool IsChatChannel(string channelName)
	{
		return !string.IsNullOrEmpty(this._currentChannelName) && this._currentChannelName == channelName;
	}

	public void AddMessage(ChatMessage message, MessageChatType messageType)
	{
		if (this._allChatMessages.Count > 50)
		{
			this._allChatMessages.Dequeue();
		}
		switch (messageType)
		{
		case MessageChatType.Location:
			if (!this.IsMuted)
			{
				if (this._locationChatMessages.Count > 50)
				{
					this._locationChatMessages.Dequeue();
				}
				this._locationChatMessages.Enqueue(message);
				this._allChatMessages.Enqueue(message);
			}
			break;
		case MessageChatType.Global:
			if (this._globalChatMessages.Count > 50)
			{
				this._globalChatMessages.Dequeue();
			}
			this._globalChatMessages.Enqueue(message);
			this._allChatMessages.Enqueue(message);
			break;
		case MessageChatType.Club:
			if (!this.IsMuted)
			{
				if (this._teamChatMessages.Count > 50)
				{
					this._teamChatMessages.Dequeue();
				}
				this._teamChatMessages.Enqueue(message);
				this._allChatMessages.Enqueue(message);
			}
			break;
		case MessageChatType.Friends:
			if (!this.IsMuted)
			{
				if (this._friendsChatMessages.Count > 50)
				{
					this._friendsChatMessages.Dequeue();
				}
				this._friendsChatMessages.Enqueue(message);
				this._allChatMessages.Enqueue(message);
			}
			break;
		case MessageChatType.Private:
			if (!this.IsMuted)
			{
				if (this._privateChatMessages.Count > 50)
				{
					this._privateChatMessages.Dequeue();
				}
				this._privateChatMessages.Enqueue(message);
				this._allChatMessages.Enqueue(message);
				this.AddUnreadPrivateChatTab(message);
			}
			break;
		case MessageChatType.System:
			if (this._systemChatMessages.Count > 50)
			{
				this._systemChatMessages.Dequeue();
			}
			this._systemChatMessages.Enqueue(message);
			this._allChatMessages.Enqueue(message);
			break;
		case MessageChatType.Tournament:
			if (!this.IsMuted)
			{
				if (this._tournamentChatMessages.Count > 50)
				{
					this._tournamentChatMessages.Dequeue();
				}
				this._tournamentChatMessages.Enqueue(message);
			}
			break;
		}
	}

	private void AddUnreadPrivateChatTab(ChatMessage message)
	{
		if (!this.UnreadChatTabs.Any((Player x) => x.UserId == message.Sender.UserId))
		{
			this.UnreadChatTabs.Add(message.Sender);
		}
	}

	public void AddMessage(string messageText, MessageChatType messageType, bool setTimestamp = true)
	{
		ChatMessage chatMessage = new ChatMessage
		{
			Message = messageText,
			Sender = new Player
			{
				UserId = "-1"
			},
			Timestamp = ((!setTimestamp) ? null : new DateTime?(TimeHelper.UtcTime()))
		};
		this.AddMessage(chatMessage, messageType);
	}

	public void SendMessage(ChatMessage message, MessageChatType messageType)
	{
		message.Message = Regex.Replace(message.Message, "<.*?>", string.Empty);
		if (this._allChatMessages.Count > 50)
		{
			this._allChatMessages.Dequeue();
		}
		switch (messageType)
		{
		case MessageChatType.Location:
			PhotonConnectionFactory.Instance.SendChatMessage(null, message.Message, message.Channel, null, null, false, null);
			if (this._locationChatMessages.Count > 50)
			{
				this._locationChatMessages.Dequeue();
			}
			this._locationChatMessages.Enqueue(message);
			this._allChatMessages.Enqueue(message);
			break;
		case MessageChatType.Global:
			if (this._globalChatMessages.Count > 50)
			{
				this._globalChatMessages.Dequeue();
			}
			this._globalChatMessages.Enqueue(message);
			this._allChatMessages.Enqueue(message);
			break;
		case MessageChatType.Club:
			PhotonConnectionFactory.Instance.SendChatMessage(null, message.Message, message.Channel, null, null, false, null);
			if (this._teamChatMessages.Count > 50)
			{
				this._teamChatMessages.Dequeue();
			}
			this._teamChatMessages.Enqueue(message);
			this._allChatMessages.Enqueue(message);
			break;
		case MessageChatType.Friends:
			PhotonConnectionFactory.Instance.SendChatMessage(null, message.Message, message.Channel, null, null, false, null);
			if (this._friendsChatMessages.Count > 50)
			{
				this._friendsChatMessages.Dequeue();
			}
			this._friendsChatMessages.Enqueue(message);
			this._allChatMessages.Enqueue(message);
			break;
		case MessageChatType.Private:
			PhotonConnectionFactory.Instance.SendChatMessage(message.Recepient.UserName, message.Message, null, null, null, false, null);
			if (this._privateChatMessages.Count > 50)
			{
				this._privateChatMessages.Dequeue();
			}
			this._privateChatMessages.Enqueue(message);
			this._allChatMessages.Enqueue(message);
			break;
		case MessageChatType.System:
			if (this._systemChatMessages.Count > 50)
			{
				this._systemChatMessages.Dequeue();
			}
			this._systemChatMessages.Enqueue(message);
			this._allChatMessages.Enqueue(message);
			break;
		case MessageChatType.Tournament:
		{
			string currentChannelName = TournamentHelper.CurrentChannelName;
			if (!string.IsNullOrEmpty(currentChannelName))
			{
				PhotonConnectionFactory.Instance.SendChatMessageToChannel(currentChannelName, message.Message);
			}
			break;
		}
		}
	}

	public IEnumerable<ChatMessage> GetMessages(MessageChatType messageType)
	{
		switch (messageType)
		{
		case MessageChatType.Location:
			return this._locationChatMessages;
		case MessageChatType.Global:
			return this._globalChatMessages;
		case MessageChatType.Club:
			return this._teamChatMessages;
		case MessageChatType.Friends:
			return this._friendsChatMessages;
		case MessageChatType.Private:
			return this._privateChatMessages;
		case MessageChatType.System:
			return this._systemChatMessages;
		case MessageChatType.All:
			return this._allChatMessages;
		case MessageChatType.Tournament:
			return this._tournamentChatMessages;
		default:
			return null;
		}
	}

	public void ClearMessages(MessageChatType messageType)
	{
		switch (messageType)
		{
		case MessageChatType.Location:
			this._locationChatMessages.Clear();
			this._allChatMessages.Clear();
			break;
		case MessageChatType.Global:
			this._globalChatMessages.Clear();
			break;
		case MessageChatType.Club:
			this._teamChatMessages.Clear();
			break;
		case MessageChatType.Friends:
			this._friendsChatMessages.Clear();
			break;
		case MessageChatType.Private:
			this._privateChatMessages.Clear();
			break;
		case MessageChatType.System:
			this._systemChatMessages.Clear();
			break;
		case MessageChatType.Tournament:
			this._tournamentChatMessages.Clear();
			break;
		}
	}

	internal IEnumerable<ChatMessage> GetPrivateMessages(Player player)
	{
		return this._privateChatMessages.Where((ChatMessage x) => x.Sender.UserId == player.UserId || x.Recepient.UserId == player.UserId);
	}

	public void GetIncomingPrivateMessage(Player player)
	{
		if (!this.OpenPrivates.Any((Player x) => x.UserId == player.UserId))
		{
			this.OpenPrivates.Add(player);
		}
	}

	public void UpdateVoice(string uName, bool isMuted, bool isTalking)
	{
	}

	private const int CountOfMessages = 50;

	private readonly Queue<ChatMessage> _allChatMessages = new Queue<ChatMessage>();

	private readonly Queue<ChatMessage> _locationChatMessages = new Queue<ChatMessage>();

	private readonly Queue<ChatMessage> _globalChatMessages = new Queue<ChatMessage>();

	private readonly Queue<ChatMessage> _teamChatMessages = new Queue<ChatMessage>();

	private readonly Queue<ChatMessage> _friendsChatMessages = new Queue<ChatMessage>();

	private readonly Queue<ChatMessage> _privateChatMessages = new Queue<ChatMessage>();

	private readonly Queue<ChatMessage> _systemChatMessages = new Queue<ChatMessage>();

	private readonly Queue<ChatMessage> _tournamentChatMessages = new Queue<ChatMessage>();

	public int PlayerOnLocaionCount;

	public Dictionary<string, Player> PlayerOnLocation = new Dictionary<string, Player>();

	public List<Player> OpenPrivates = new List<Player>();

	public List<Player> UnreadChatTabs = new List<Player>();

	private string _currentChannelName;
}
