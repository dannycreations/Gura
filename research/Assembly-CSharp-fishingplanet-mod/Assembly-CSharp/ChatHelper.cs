using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using UnityEngine;

public class ChatHelper
{
	public static string GetText(IEnumerable<ChatMessage> messages, bool showTimestamp = false)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null)
		{
			Debug.LogError("GetText - Profile is not available!");
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = profile.UserId.ToString();
		foreach (ChatMessage chatMessage in messages.OrderBy((ChatMessage x) => (x.Timestamp == null) ? x.Timestamp : new DateTime?(x.Timestamp.Value.ToLocalTime())))
		{
			if (showTimestamp && chatMessage.Timestamp != null)
			{
				stringBuilder.AppendFormat("<color=grey>[<i>{0}</i>]</color> ", MeasuringSystemManager.LongTimeString(chatMessage.Timestamp.Value.ToLocalTime()));
			}
			if (chatMessage.Sender.UserId == text)
			{
				stringBuilder.AppendFormat("<color=green><b>{0}</b>:</color> {1}\n", ChatHelper.GetSelfName(chatMessage, "ff"), ChatHelper.TransparentMessage(chatMessage.Message, 255));
			}
			else if (chatMessage.Sender.UserId == "-1")
			{
				stringBuilder.AppendFormat("{0}\n", ChatHelper.TransparentMessage(chatMessage.Message, 255));
			}
			else
			{
				stringBuilder.AppendFormat("<color=cyan><b>{0}</b></color> {1}\n", ChatHelper.GetName(chatMessage, "ff"), ChatHelper.TransparentMessage(chatMessage.Message, 255));
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetShortText(IEnumerable<ChatMessage> messages, bool showTimestamp = false)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null)
		{
			Debug.LogError("GetShortText - Profile is not available!");
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = profile.UserId.ToString();
		foreach (ChatMessage chatMessage in messages.OrderBy((ChatMessage x) => (x.Timestamp == null) ? x.Timestamp : new DateTime?(x.Timestamp.Value.ToLocalTime())))
		{
			int num = 255;
			if ((DateTime.Now - chatMessage.AddedDateTime).TotalMilliseconds >= (double)(ChatHelper._messageLifetime * 1000f))
			{
				num = (int)Mathf.Lerp(0f, 255f, (float)((double)(ChatHelper._messageLifetime + ChatHelper._messageHideFadeTime) - (DateTime.Now - chatMessage.AddedDateTime).TotalMilliseconds / 1000.0));
			}
			string text2 = num.ToString("X2");
			if (num < -1)
			{
				break;
			}
			if (num != 0)
			{
				if (showTimestamp && chatMessage.Timestamp != null)
				{
					stringBuilder.AppendFormat("\n<color=#808080{1}>[<i>{0}</i>]</color> ", MeasuringSystemManager.LongTimeString(chatMessage.Timestamp.Value.ToLocalTime()), text2);
				}
				else
				{
					stringBuilder.AppendFormat("\n", new object[0]);
				}
				if (chatMessage.Sender.UserId == text)
				{
					stringBuilder.AppendFormat("<color=#008000{2}><b>{0}</b>:</color> <color=#dcdcdc{2}>{1}</color>", ChatHelper.GetSelfName(chatMessage, text2), ChatHelper.TransparentMessage(chatMessage.Message, num), text2);
				}
				else if (chatMessage.Sender.UserId == "-1")
				{
					stringBuilder.AppendFormat("<color=#dcdcdc{1}>{0}</color>", ChatHelper.TransparentMessage(chatMessage.Message, num), text2);
				}
				else
				{
					stringBuilder.AppendFormat("<color=#00ffff{2}><b>{0}</b></color> <color=#dcdcdc{2}>{1}</color>", ChatHelper.GetName(chatMessage, text2), ChatHelper.TransparentMessage(chatMessage.Message, num), text2);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetName(ChatMessage message, string hexAlpha)
	{
		string channel = message.Channel;
		if (channel != null)
		{
			if (channel == "Private")
			{
				return string.Format("{0} {3} <color=#808080{2}><i>-->{1}</i> {4}:</color>", new object[]
				{
					message.Sender.UserName,
					PhotonConnectionFactory.Instance.Profile.Name,
					hexAlpha,
					PlayerProfileHelper.GetPlayerLevelRank(message.Sender),
					PlayerProfileHelper.GetPlayerLevelRank(PhotonConnectionFactory.Instance.Profile)
				});
			}
			if (channel == "Local" || channel == "Club" || channel == "Friends" || channel == "Global" || channel == "Tournament")
			{
				return string.Format("{0}:", PlayerProfileHelper.GetPlayerNameLevelRank(message.Sender));
			}
		}
		return message.Sender.UserName;
	}

	public static string GetSelfName(ChatMessage message, string hexAlpha)
	{
		string channel = message.Channel;
		if (channel != null)
		{
			if (channel == "Private")
			{
				return string.Format("{0} {4} <color=#808080{2}><i>-->{1}</i> {3}</color>", new object[]
				{
					PhotonConnectionFactory.Instance.Profile.Name,
					message.Recepient.UserName,
					hexAlpha,
					PlayerProfileHelper.GetPlayerLevelRank(message.Recepient),
					PlayerProfileHelper.GetPlayerLevelRank(PhotonConnectionFactory.Instance.Profile)
				});
			}
			if (channel == "Local" || channel == "Club" || channel == "Friends" || channel == "Global" || channel == "Tournament")
			{
				return PlayerProfileHelper.GetPlayerNameLevelRank(PhotonConnectionFactory.Instance.Profile);
			}
		}
		return PhotonConnectionFactory.Instance.Profile.Name;
	}

	private static string TransparentMessage(string original, int value)
	{
		return original.Replace("##", value.ToString("X2"));
	}

	private static float _messageLifetime = 25f;

	private static float _messageHideFadeTime = 5f;
}
