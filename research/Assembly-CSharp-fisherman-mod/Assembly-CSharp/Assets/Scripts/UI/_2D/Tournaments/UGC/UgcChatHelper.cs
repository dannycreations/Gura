using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Newtonsoft.Json;
using ObjectModel;

namespace Assets.Scripts.UI._2D.Tournaments.UGC
{
	public class UgcChatHelper
	{
		public static UserCompetitionChatMessage TryParseSpecialMsg(ChatMessage message)
		{
			UserCompetitionChatMessage userCompetitionChatMessage2;
			try
			{
				UserCompetitionChatMessage userCompetitionChatMessage = JsonConvert.DeserializeObject<UserCompetitionChatMessage>(message.Data);
				if (!string.IsNullOrEmpty(message.OneTimeData))
				{
					UserCompetitionChatMessageType messageType = userCompetitionChatMessage.MessageType;
					if (messageType != UserCompetitionChatMessageType.UgcCompetitionStartRequested && messageType != UserCompetitionChatMessageType.UgcCompetitionStartUnrequested)
					{
						userCompetitionChatMessage.Players = JsonConvert.DeserializeObject<List<UserCompetitionPlayer>>(message.OneTimeData);
					}
					else
					{
						userCompetitionChatMessage.UserCompetitionRuntime = JsonConvert.DeserializeObject<UserCompetitionRuntime>(message.OneTimeData);
					}
				}
				userCompetitionChatMessage2 = userCompetitionChatMessage;
			}
			catch
			{
				userCompetitionChatMessage2 = null;
			}
			return userCompetitionChatMessage2;
		}

		public static bool HasLocalization(UserCompetitionChatMessageType messageType)
		{
			return UgcChatHelper.ChatMessageTypeLoc.ContainsKey(messageType);
		}

		public static string GetSpecialMsg(UserCompetitionChatMessage msg)
		{
			string text = UgcChatHelper.ChatMessageTypeLoc[msg.MessageType];
			if (msg.MessageType == UserCompetitionChatMessageType.UgcPlayerCaughtFish)
			{
				FishBrief fishBrief = CacheLibrary.MapCache.FishesLight.FirstOrDefault((FishBrief p) => p.FishId == msg.FishId);
				if (fishBrief != null)
				{
					string text2 = ScriptLocalization.Get(text);
					object empty = string.Empty;
					string text3 = "<color=#{0}>{1} - {2:N2}{3}</color>";
					object[] array = new object[4];
					array[0] = UIHelper.GetFishColor(fishBrief);
					array[1] = fishBrief.Name;
					int num = 2;
					float? fishWeight = msg.FishWeight;
					array[num] = MeasuringSystemManager.FishWeight((fishWeight == null) ? 0f : fishWeight.Value);
					array[3] = MeasuringSystemManager.FishWeightSufix();
					return string.Format(text2, empty, string.Format(text3, array)).TrimStart(new char[0]);
				}
				return "<color=#FF0000FF>Error fish: " + msg.FishId + "</color>";
			}
			else
			{
				if (msg.MessageType == UserCompetitionChatMessageType.UgcPlayerRemovedByHost)
				{
					string text4 = string.Format(ScriptLocalization.Get(text), UgcConsts.GetYellowTan(msg.PlayerName));
					return UgcChatHelper.GetColor(msg.MessageType, text4);
				}
				return UgcChatHelper.GetColor(msg.MessageType, ScriptLocalization.Get(text));
			}
		}

		private static string GetColor(UserCompetitionChatMessageType t, string msg)
		{
			switch (t)
			{
			case UserCompetitionChatMessageType.UgcPlayerRegistered:
				return UgcConsts.GetYellowTan(msg);
			case UserCompetitionChatMessageType.UgcPlayerUnregistered:
			case UserCompetitionChatMessageType.UgcPlayerRemovedByHost:
			case UserCompetitionChatMessageType.UgcPlayerUnapproved:
				return string.Format("<color=#AC4836FF>{0}</color>", msg);
			case UserCompetitionChatMessageType.UgcPlayerApproved:
				return UgcConsts.GetWinner(msg);
			default:
				return msg;
			}
		}

		private static readonly Dictionary<UserCompetitionChatMessageType, string> ChatMessageTypeLoc = new Dictionary<UserCompetitionChatMessageType, string>
		{
			{
				UserCompetitionChatMessageType.UgcPlayerRegistered,
				"UGC_Chat_PlayerRegistered"
			},
			{
				UserCompetitionChatMessageType.UgcPlayerUnregistered,
				"UGC_Chat_PlayerUnregistered"
			},
			{
				UserCompetitionChatMessageType.UgcPlayerApproved,
				"UGC_Chat_PlayerApproved"
			},
			{
				UserCompetitionChatMessageType.UgcPlayerUnapproved,
				"UGC_Chat_PlayerUnapproved"
			},
			{
				UserCompetitionChatMessageType.UgcPlayerRemovedByHost,
				"UGC_Chat_PlayerRemovedByHost"
			},
			{
				UserCompetitionChatMessageType.UgcPlayerCaughtFish,
				"UGC_Chat_PlayerCaughtFish"
			}
		};
	}
}
