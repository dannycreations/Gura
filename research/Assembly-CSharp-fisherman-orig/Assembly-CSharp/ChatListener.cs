using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assets.Scripts.UI._2D.PlayerProfile;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using CodeStage.AntiCheat.ObscuredTypes;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces;
using UnityEngine;

public class ChatListener : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<ChatListener.EventChatMessageArgs> OnGotIncomeMessage;

	internal void Start()
	{
		this._joinMessage = ScriptLocalization.Get("JoinMessage");
		this._leaveMessage = ScriptLocalization.Get("LeaveMessage");
		this._rankMessage = ScriptLocalization.Get("RankMessage");
		this._levelMessage = ScriptLocalization.Get("LevelMessage");
		this._achivementMessage = ScriptLocalization.Get("AchivementMessage");
		this._questMessage = ScriptLocalization.Get("QuestMessage");
		this._caughtMessage = ScriptLocalization.Get("CaughtMessage");
		this._brokenMessage = ScriptLocalization.Get("BrokenMessage");
		this._fireworkLaunchedMessage = ScriptLocalization.Get("FireworkLaunched");
		this._buoySharedMessage = ScriptLocalization.Get("RecievedBuoyText");
		this._itemCaughtMessage = ScriptLocalization.Get("UnderwaterItemMessage");
		if (this._currentPond == null || this._currentPond != StaticUserData.CurrentPond)
		{
			StaticUserData.ChatController.ClearMessages(MessageChatType.Location);
		}
		GameFactory.ChatListener = this;
		PhotonConnectionFactory.Instance.IsChatMessagingOn = true;
		PhotonConnectionFactory.Instance.OnIncomingChannelMessage += this.Instance_OnIncomingChannelMessage;
		PhotonConnectionFactory.Instance.OnLocalEvent += this.OnLocalEvent;
		PhotonConnectionFactory.Instance.OnIncomingChatMessage += this.OnIncomingChatMessage;
		PhotonConnectionFactory.Instance.OnIncognitoChanged += this.OnIncognitoChanged;
		PhotonConnectionFactory.Instance.OnChatCommandSucceeded += this.OnChatCommandSucceeded;
		PhotonConnectionFactory.Instance.OnChatCommandFailed += this.OnChatCommandFailed;
		PhotonConnectionFactory.Instance.OnGotCurrentRoomPopulation += this.InstanceOnGotCurrentRoomPopulation;
		PhotonConnectionFactory.Instance.OnGettingCurrenRoomPopulationFailed += this.InstanceOnGettingCurrenRoomPopulationFailed;
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnUserCompetitionPromotion += this.Instance_OnUserCompetitionPromotion;
	}

	private void InstanceOnGettingCurrenRoomPopulationFailed(Failure failure)
	{
		Debug.LogError(failure.ErrorMessage);
	}

	internal void OnDestroy()
	{
		base.StopAllCoroutines();
		PhotonConnectionFactory.Instance.OnIncomingChannelMessage -= this.Instance_OnIncomingChannelMessage;
		PhotonConnectionFactory.Instance.OnLocalEvent -= this.OnLocalEvent;
		PhotonConnectionFactory.Instance.OnIncomingChatMessage -= this.OnIncomingChatMessage;
		PhotonConnectionFactory.Instance.OnIncognitoChanged -= this.OnIncognitoChanged;
		PhotonConnectionFactory.Instance.OnChatCommandSucceeded -= this.OnChatCommandSucceeded;
		PhotonConnectionFactory.Instance.OnChatCommandFailed -= this.OnChatCommandFailed;
		PhotonConnectionFactory.Instance.OnGotCurrentRoomPopulation -= this.InstanceOnGotCurrentRoomPopulation;
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.IsChatMessagingOn = false;
		PhotonConnectionFactory.Instance.OnUserCompetitionPromotion -= this.Instance_OnUserCompetitionPromotion;
		StaticUserData.ChatController.LeaveChatChannel();
		if (ToggleChatController.Instance != null)
		{
			ToggleChatController.Instance.RemoveChannelChat();
		}
	}

	internal void Update()
	{
		if (this._currentPond != StaticUserData.CurrentPond)
		{
			this._currentPond = StaticUserData.CurrentPond;
			StaticUserData.ChatController.ClearMessages(MessageChatType.Location);
			if (this._currentPond != null)
			{
				StaticUserData.ChatController.AddMessage(string.Format("{0} <b>{1}</b>", ScriptLocalization.Get("WelcomeMessage"), this._currentPond.Name), MessageChatType.Location, false);
			}
			StaticUserData.ChatController.PlayerOnLocation.Clear();
		}
		if (this._resetLocationRoom)
		{
			PhotonConnectionFactory.Instance.GetCurrentRoomPopulation();
			this._resetLocationRoom = false;
		}
		if (this._currentPond == null && StaticUserData.ChatController != null)
		{
			StaticUserData.ChatController.ClearMessages(MessageChatType.Location);
		}
	}

	private void OnMoved(TravelDestination destination)
	{
		if (destination == TravelDestination.Room)
		{
			this._resetLocationRoom = true;
		}
	}

	private void Instance_OnUserCompetitionPromotion(UserCompetitionPromotionMessage promotion)
	{
		if (!ObscuredPrefs.GetBool("ShowUserCompetitionPromotion", true))
		{
			return;
		}
		Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == promotion.PondId);
		StaticUserData.ChatController.AddMessage(new ChatMessage
		{
			Sender = new Player
			{
				UserId = "-1"
			},
			Message = (string.IsNullOrEmpty(promotion.PromotionMessage) ? string.Format(ScriptLocalization.Get("UGC_ChatPromotionMessage"), UgcConsts.GetYellowTan(promotion.HostName), UgcConsts.GetYellowTan(promotion.NameCustom), string.Format("<b>{0}</b>", pond.Name)) : promotion.PromotionMessage),
			MessageType = LocalEventType.Chat
		}, MessageChatType.System);
	}

	internal void OnJoin(Player player)
	{
		StaticUserData.ChatController.AddMessage(new ChatMessage
		{
			Sender = new Player
			{
				UserId = "-1"
			},
			Message = string.Format(this._joinMessage, PlayerProfileHelper.GetPlayerNameLevelRank(player)),
			MessageType = LocalEventType.Join
		}, MessageChatType.Location);
		StaticUserData.ChatController.PlayerOnLocation.Add(player.UserName, player);
		StaticUserData.ChatController.PlayerOnLocaionCount++;
	}

	internal void OnLeave(Player player)
	{
		StaticUserData.ChatController.AddMessage(new ChatMessage
		{
			Sender = new Player
			{
				UserId = "-1"
			},
			Message = string.Format(this._leaveMessage, PlayerProfileHelper.GetPlayerNameLevelRank(player)),
			MessageType = LocalEventType.Leave
		}, MessageChatType.Location);
		StaticUserData.ChatController.PlayerOnLocation.Remove(player.UserName);
		StaticUserData.ChatController.PlayerOnLocaionCount--;
	}

	private void InstanceOnGotCurrentRoomPopulation(IEnumerable<Player> players)
	{
		StaticUserData.ChatController.PlayerOnLocation = players.ToDictionary((Player x) => x.UserName, (Player x) => x);
	}

	private void Instance_OnIncomingChannelMessage(ChatMessage message)
	{
		this.AddChannelMessage(message);
	}

	internal void OnIncomingChatMessage(ChatMessage message)
	{
		message.Message = message.Message.TrimEnd(new char[] { '\r', '\n' });
		string channel = message.Channel;
		if (channel != null)
		{
			if (!(channel == "Private"))
			{
				if (!(channel == "Local"))
				{
					if (!(channel == "Club"))
					{
						if (!(channel == "Global"))
						{
							if (channel == "Tournament")
							{
								StaticUserData.ChatController.AddMessage(message, MessageChatType.Location);
							}
						}
						else
						{
							StaticUserData.ChatController.AddMessage(message, MessageChatType.Global);
						}
					}
					else
					{
						StaticUserData.ChatController.AddMessage(message, MessageChatType.Club);
					}
				}
				else
				{
					StaticUserData.ChatController.AddMessage(message, MessageChatType.Location);
				}
			}
			else
			{
				StaticUserData.ChatController.AddMessage(message, MessageChatType.Private);
				StaticUserData.ChatController.GetIncomingPrivateMessage(message.Sender);
			}
		}
		if (this.OnGotIncomeMessage != null)
		{
			this.OnGotIncomeMessage(this, new ChatListener.EventChatMessageArgs
			{
				Message = message
			});
		}
	}

	private void OnChatCommandFailed(Failure failure)
	{
		ChatMessage chatMessage = new ChatMessage
		{
			Sender = new Player
			{
				UserId = "-1"
			}
		};
		if (failure.ErrorCode == 32591)
		{
			chatMessage.Message = string.Format("<color=#00ffff##>{0}</color>", ScriptLocalization.Get("ChatCommandResponcePlayerNotFound"));
		}
		if (failure.ErrorCode == 32590)
		{
			chatMessage.Message = string.Format("<color=#00ffff##>{0}</color>", ScriptLocalization.Get("ChatCommandResponceIgnoreNotFound"));
		}
		this._systemMessagesQueue.Enqueue(chatMessage);
		PhotonConnectionFactory.Instance.GetServerTime(1);
	}

	private void OnChatCommandSucceeded(ChatCommandInfo info)
	{
		ChatMessage chatMessage = new ChatMessage
		{
			Sender = new Player
			{
				UserId = "-1"
			}
		};
		ChatCommands command = info.Command;
		if (command != 2)
		{
			if (command != 3)
			{
				if (command == 1)
				{
					string text = string.Format(ScriptLocalization.Get("ChatCommandResponceReport"), info.Username);
					chatMessage.Message = string.Format("<color=#00ffff##>{0}</color>", text);
				}
			}
			else
			{
				string text = string.Format(ScriptLocalization.Get("ChatCommandResponceUnIgnore"), info.Username);
				chatMessage.Message = string.Format("<color=#00ffff##>{0}</color>", text);
			}
		}
		else
		{
			string text = string.Format(ScriptLocalization.Get("ChatCommandResponceIgnore"), info.Username);
			chatMessage.Message = string.Format("<color=#00ffff##>{0}</color>", text);
		}
		this._systemMessagesQueue.Enqueue(chatMessage);
		PhotonConnectionFactory.Instance.GetServerTime(1);
	}

	private void OnIncognitoChanged(bool incognito)
	{
		ChatMessage chatMessage = new ChatMessage
		{
			Sender = new Player
			{
				UserId = "-1"
			}
		};
		if (incognito)
		{
			chatMessage.Message = string.Format("<color=#00ffff##>{0}</color>", ScriptLocalization.Get("ChatCommandResponceIncognitoOn"));
		}
		else
		{
			chatMessage.Message = string.Format("<color=#00ffff##>{0}</color>", ScriptLocalization.Get("ChatCommandResponceIncognitoOff"));
		}
		this._systemMessagesQueue.Enqueue(chatMessage);
		PhotonConnectionFactory.Instance.GetServerTime(1);
	}

	public void OnLocalEvent(LocalEvent localEvent)
	{
		LocalEventType eventType = localEvent.EventType;
		if (eventType != LocalEventType.Join)
		{
			if (eventType == LocalEventType.Leave)
			{
				StaticUserData.ChatController.PlayerOnLocaionCount--;
			}
		}
		else
		{
			StaticUserData.ChatController.PlayerOnLocaionCount++;
		}
		this.OnLocalEventApply(localEvent);
	}

	private void OnLocalEventApply(LocalEvent localEvent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		LocalEventType localEventType = LocalEventType.Chat;
		try
		{
			switch (localEvent.EventType)
			{
			case LocalEventType.Join:
				stringBuilder.AppendFormat(this._joinMessage, PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player));
				if (!StaticUserData.ChatController.PlayerOnLocation.ContainsKey(localEvent.Player.UserName))
				{
					StaticUserData.ChatController.PlayerOnLocation.Add(localEvent.Player.UserName, localEvent.Player);
				}
				localEventType = LocalEventType.Join;
				break;
			case LocalEventType.Leave:
				stringBuilder.AppendFormat(this._leaveMessage, PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player));
				if (StaticUserData.ChatController.PlayerOnLocation.ContainsKey(localEvent.Player.UserName))
				{
					StaticUserData.ChatController.PlayerOnLocation.Remove(localEvent.Player.UserName);
				}
				localEventType = LocalEventType.Leave;
				break;
			case LocalEventType.FishCaught:
			{
				string fishColor = UIHelper.GetFishColor(localEvent.Fish, null);
				stringBuilder.AppendFormat(this._caughtMessage, new object[]
				{
					PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player),
					MeasuringSystemManager.FishWeight(localEvent.Fish.Weight).ToString("N2"),
					localEvent.Fish.Name,
					fishColor
				});
				localEventType = LocalEventType.FishCaught;
				break;
			}
			case LocalEventType.TackleBroken:
			{
				BrokenTackleType tackleType = localEvent.TackleType;
				switch (tackleType)
				{
				case BrokenTackleType.Rod:
					stringBuilder.AppendFormat("<color=#808080##><b>{0}</b>: {1}</color>", PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), ScriptLocalization.Get("BrakeRodChatText"));
					break;
				case BrokenTackleType.Reel:
					stringBuilder.AppendFormat("<color=#808080##><b>{0}</b>: {1}</color>", PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), ScriptLocalization.Get("BrakeReelChatText"));
					break;
				case BrokenTackleType.Line:
					stringBuilder.AppendFormat("<color=#808080##><b>{0}</b>: {1}</color>", PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), ScriptLocalization.Get("BrakeLineChatText"));
					break;
				default:
					if (tackleType == BrokenTackleType.LineCut)
					{
						stringBuilder.AppendFormat("<color=#808080##><b>{0}</b>: {1}</color>", PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), ScriptLocalization.Get("BittenOffLineChatText"));
					}
					break;
				}
				localEventType = LocalEventType.TackleBroken;
				break;
			}
			case LocalEventType.Level:
				if (localEvent.Level != 0)
				{
					stringBuilder.AppendFormat(this._levelMessage, PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), localEvent.Level);
				}
				else if (localEvent.Rank != 0)
				{
					stringBuilder.AppendFormat(this._rankMessage, PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), localEvent.Rank);
				}
				localEventType = LocalEventType.Level;
				break;
			case LocalEventType.Achivement:
				stringBuilder.AppendFormat(this._achivementMessage, PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), localEvent.Achivement, string.Format(localEvent.AchivementStage, localEvent.AchivementStageCount));
				localEventType = LocalEventType.Achivement;
				break;
			case LocalEventType.QuestCompleted:
			case LocalEventType.MissionCompleted:
				stringBuilder.AppendFormat(this._questMessage, PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), string.Format("<b>{0}</b>", localEvent.Quest), localEvent.QuestStage);
				break;
			case LocalEventType.FireworkLaunched:
				stringBuilder.AppendFormat(this._fireworkLaunchedMessage, PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player));
				break;
			case LocalEventType.BuoyShared:
				if (localEvent.Receiver == PhotonConnectionFactory.Instance.Profile.Name)
				{
					stringBuilder.Append("<color=#808080##>" + string.Format(this._buoySharedMessage, string.Format("<b>{0}</b>:", PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player))) + "</color>");
				}
				break;
			case LocalEventType.ItemCaught:
				stringBuilder.AppendFormat(this._itemCaughtMessage, PlayerProfileHelper.GetPlayerNameLevelRank(localEvent.Player), localEvent.CaughtItem.Name, "808080");
				localEventType = LocalEventType.ItemCaught;
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "   " + ex.StackTrace);
		}
		if (stringBuilder.Length > 0)
		{
			this._systemMessagesQueue.Enqueue(new ChatMessage
			{
				Message = stringBuilder.ToString(),
				Sender = new Player
				{
					UserId = "-1"
				},
				MessageType = localEventType
			});
			ChatMessage chatMessage = this._systemMessagesQueue.Dequeue();
			chatMessage.Timestamp = new DateTime?(PhotonConnectionFactory.Instance.ServerUtcNow.ToLocalTime());
			StaticUserData.ChatController.AddMessage(chatMessage, MessageChatType.System);
		}
	}

	private void AddChannelMessage(ChatMessage message)
	{
		string currentChannelName = TournamentHelper.CurrentChannelName;
		if (currentChannelName == message.Channel)
		{
			UserCompetitionChatMessage userCompetitionChatMessage = UgcChatHelper.TryParseSpecialMsg(message);
			if (userCompetitionChatMessage != null)
			{
				if (!UgcChatHelper.HasLocalization(userCompetitionChatMessage.MessageType))
				{
					return;
				}
				message.Message = UgcChatHelper.GetSpecialMsg(userCompetitionChatMessage);
			}
			StaticUserData.ChatController.AddMessage(message, MessageChatType.Tournament);
		}
	}

	private string _joinMessage;

	private string _leaveMessage;

	private string _levelMessage;

	private string _rankMessage;

	private string _achivementMessage;

	private string _questMessage;

	private string _caughtMessage;

	private string _brokenMessage;

	private string _fireworkLaunchedMessage;

	private string _buoySharedMessage;

	private string _itemCaughtMessage;

	private Pond _currentPond;

	private bool _resetLocationRoom = true;

	private Queue<ChatMessage> _systemMessagesQueue = new Queue<ChatMessage>();

	public class EventChatMessageArgs : EventArgs
	{
		public ChatMessage Message { get; set; }
	}
}
