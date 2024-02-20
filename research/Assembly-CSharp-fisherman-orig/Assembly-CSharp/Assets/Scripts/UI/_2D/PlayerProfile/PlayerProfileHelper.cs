using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Assets.Scripts.Steamwork.NET;
using I2.Loc;
using ObjectModel;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.Scripts.UI._2D.PlayerProfile
{
	public class PlayerProfileHelper
	{
		public static void Clear()
		{
			PlayerProfileHelper.UserPics.Clear();
			PlayerProfileHelper.UserPicsLoading.Clear();
		}

		public static Player ProfileToPlayer(Profile p)
		{
			return new Player
			{
				UserName = p.Name,
				UserId = p.UserId.ToString(),
				Level = p.Level,
				Rank = p.Rank
			};
		}

		public static Player ProfileToPlayerExt(Profile profile)
		{
			return new Player
			{
				AvatarBID = new int?(profile.AvatarBID),
				AvatarUrl = profile.AvatarUrl,
				ExternalId = profile.ExternalId,
				Gender = profile.Gender,
				HasPremium = profile.HasPremium,
				Level = profile.Level,
				Rank = profile.Rank,
				PondId = profile.PondId,
				Source = profile.Source,
				UserId = profile.UserId.ToString()
			};
		}

		public static string GetPlayerLevelRank(ILevelRank player)
		{
			return (player.Rank <= 0) ? string.Format("\ue62d {0}", player.Level) : string.Format("\ue62d {0} \ue727 {1}", player.Level, player.Rank);
		}

		public static string GetPlayerLevelColored(ILevelRank player)
		{
			return string.Format("<color=#F3D462FF>\ue62d</color> {0}", player.Level);
		}

		public static string GetPlayerRankColored(ILevelRank player)
		{
			return (player.Rank <= 0) ? string.Empty : string.Format("<color=#F3D462FF>\ue727</color> {0}", player.Rank);
		}

		public static string GetPlayerLevelRank(Player player)
		{
			return (player.Rank <= 0) ? string.Format("\ue62d {0}", player.Level) : string.Format("\ue62d {0} \ue727 {1}", player.Level, player.Rank);
		}

		public static string GetPlayerNameLevelRank(IPlayer player)
		{
			return string.Format("{0} {1}", player.UserName, PlayerProfileHelper.GetPlayerLevelRank(player));
		}

		public static string GetPlayerLevelRank(Profile player)
		{
			return (player.Rank <= 0) ? string.Format("\ue62d {0}", player.Level) : string.Format("\ue62d {0} \ue727 {1}", player.Level, player.Rank);
		}

		public static string GetPlayerNameLevelRank(Profile player)
		{
			return string.Format("{0} {1}", player.Name, PlayerProfileHelper.GetPlayerLevelRank(player));
		}

		public static string PaymentCurrency
		{
			get
			{
				Profile profile = PhotonConnectionFactory.Instance.Profile;
				return (profile == null || string.IsNullOrEmpty(profile.PaymentCurrency)) ? "USD" : profile.PaymentCurrency;
			}
		}

		public static void SetAvatarIco(string userId, string externalId, string avatarUrl, Image avatar)
		{
			PlayerProfileHelper.SetAvatarIco(new Profile
			{
				UserId = new Guid(userId),
				ExternalId = externalId,
				AvatarUrl = avatarUrl
			}, avatar, SteamAvatarLoader.AvatarTypes.Large);
		}

		public static void SetAvatarIco(Player player, Image avatar)
		{
			PlayerProfileHelper.SetAvatarIco(player.UserId, player.ExternalId, player.AvatarUrl, avatar);
		}

		private static IEnumerator LoadGamerPic(WWW download, Image pic, SteamAvatarLoader.AvatarTypes type, CSteamID steamId)
		{
			yield return download;
			Texture2D remoteGamerPic = new Texture2D(pic.mainTexture.width, pic.mainTexture.height, 10, false);
			download.LoadImageIntoTexture(remoteGamerPic);
			download.Dispose();
			if (pic == null)
			{
				yield break;
			}
			SteamAvatarLoader.Push(steamId, type, remoteGamerPic);
			pic.overrideSprite = Sprite.Create(remoteGamerPic, new Rect(0f, 0f, (float)remoteGamerPic.width, (float)remoteGamerPic.height), new Vector2(0.5f, 0.5f));
			yield break;
		}

		private static IEnumerator GetUserAvatarUri(string url, Action<string> onFinish, SteamAvatarLoader.AvatarTypes type)
		{
			using (UnityWebRequest www = UnityWebRequest.Get(url))
			{
				yield return www.SendWebRequest();
				if (www.isNetworkError || www.isHttpError)
				{
					Debug.LogError(www.error);
				}
				else
				{
					byte[] data = www.downloadHandler.data;
					if (data.Length > 0)
					{
						using (MemoryStream memoryStream = new MemoryStream(data))
						{
							XmlDocument xmlDocument = new XmlDocument();
							try
							{
								xmlDocument.Load(memoryStream);
							}
							catch (Exception)
							{
								onFinish(null);
							}
							if (xmlDocument.DocumentElement != null)
							{
								string text = "avatarFull";
								if (type == SteamAvatarLoader.AvatarTypes.Medium)
								{
									text = "avatarMedium";
								}
								else if (type == SteamAvatarLoader.AvatarTypes.Small)
								{
									text = "avatarIcon";
								}
								XmlNodeList elementsByTagName = xmlDocument.DocumentElement.GetElementsByTagName(text);
								if (elementsByTagName.Count > 0)
								{
									onFinish(elementsByTagName[0].InnerText);
								}
							}
							else
							{
								onFinish(null);
							}
						}
					}
				}
			}
			yield break;
		}

		public static void SetAvatarIco(Profile profile, Image avatar, SteamAvatarLoader.AvatarTypes type = SteamAvatarLoader.AvatarTypes.Large)
		{
			Profile profile2 = PhotonConnectionFactory.Instance.Profile;
			if (profile2 == null)
			{
				return;
			}
			if (!SteamManager.Initialized)
			{
				return;
			}
			CSteamID steamId;
			steamId.m_SteamID = 0UL;
			ulong num;
			if (profile2.UserId == profile.UserId)
			{
				steamId = SteamUser.GetSteamID();
			}
			else if (ulong.TryParse(profile.ExternalId, out num))
			{
				steamId = new CSteamID(num);
			}
			if (steamId.m_SteamID != 0UL)
			{
				SteamAvatarLoader.GetUserAvatar(steamId, delegate(Texture2D texture)
				{
					if (texture != null)
					{
						if (avatar != null)
						{
							avatar.overrideSprite = Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), new Vector2(0.5f, 0.5f));
						}
					}
					else if (avatar != null && avatar.gameObject.activeSelf)
					{
						avatar.StartCoroutine(PlayerProfileHelper.GetUserAvatarUri(string.Format("https://steamcommunity.com/profiles/{0}/?xml=1", steamId.m_SteamID), delegate(string uri)
						{
							if (!string.IsNullOrEmpty(uri))
							{
								avatar.StartCoroutine(PlayerProfileHelper.LoadGamerPic(new WWW(uri), avatar, type, steamId));
							}
						}, type));
					}
				}, type);
			}
		}

		public static string UsernameCaption
		{
			get
			{
				return ScriptLocalization.Get("NameCaption");
			}
		}

		public const float timeToBatchProfileRequsts = 0.3f;

		public const string LevelIco = "\ue62d";

		public const string RankIco = "\ue727";

		public const string LevelIcoColoredFormat = "<color=#F3D462FF>\ue62d</color> {0}";

		public const string RankIcoColoredFormat = "<color=#F3D462FF>\ue727</color> {0}";

		public const string LevelFormat = "\ue62d {0}";

		public const string RankFormat = "\ue727 {0}";

		public const string LevelRankFormat = "\ue62d {0} \ue727 {1}";

		public const string NameLevelRankFormat = "{0} {1}";

		private static readonly List<PlayerProfileHelper.UserPic> UserPics = new List<PlayerProfileHelper.UserPic>();

		private static readonly List<string> UserPicsLoading = new List<string>();

		public class UserPic
		{
			public string ExternalId { get; set; }

			public Image Pic { get; set; }
		}
	}
}
