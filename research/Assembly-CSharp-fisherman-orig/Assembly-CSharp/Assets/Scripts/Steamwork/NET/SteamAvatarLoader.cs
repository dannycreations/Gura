using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Assets.Scripts.Steamwork.NET
{
	public static class SteamAvatarLoader
	{
		public static void GetUserAvatar(CSteamID steamId, Action<Texture2D> callback, SteamAvatarLoader.AvatarTypes type)
		{
			if (SteamAvatarLoader._cache.ContainsKey(steamId))
			{
				Texture2D texture = SteamAvatarLoader._cache[steamId].GetTexture(type);
				if (texture != null)
				{
					callback(texture);
					return;
				}
			}
			int num;
			if (type != SteamAvatarLoader.AvatarTypes.Large)
			{
				if (type != SteamAvatarLoader.AvatarTypes.Medium)
				{
					num = SteamFriends.GetSmallFriendAvatar(steamId);
				}
				else
				{
					num = SteamFriends.GetMediumFriendAvatar(steamId);
				}
			}
			else
			{
				num = SteamFriends.GetLargeFriendAvatar(steamId);
			}
			uint num2;
			uint num3;
			bool flag = SteamUtils.GetImageSize(num, ref num2, ref num3);
			if (flag && num2 > 0U && num3 > 0U)
			{
				byte[] array = new byte[num2 * num3 * 4U];
				Texture2D texture2D = new Texture2D((int)num2, (int)num3, 4, false, false);
				flag = SteamUtils.GetImageRGBA(num, array, (int)(num2 * num3 * 4U));
				if (flag)
				{
					texture2D.LoadRawTextureData(array);
					texture2D.Apply();
					Texture2D texture2D2 = SteamAvatarLoader.FlipTexture(texture2D);
					SteamAvatarLoader.Push(steamId, type, texture2D2);
					callback(texture2D2);
					Object.DestroyImmediate(texture2D);
				}
				else
				{
					callback(null);
				}
			}
			else
			{
				callback(null);
			}
		}

		public static Texture2D FlipTexture(Texture2D original)
		{
			Texture2D texture2D = new Texture2D(original.width, original.height);
			int width = original.width;
			int height = original.height;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					texture2D.SetPixel(i, height - j - 1, original.GetPixel(i, j));
				}
			}
			texture2D.Apply();
			return texture2D;
		}

		public static void Push(CSteamID steamId, SteamAvatarLoader.AvatarTypes type, Texture2D t)
		{
			if (!SteamAvatarLoader._cache.ContainsKey(steamId))
			{
				SteamAvatarLoader._cache.Add(steamId, new SteamAvatarLoader.UserAvatarData());
			}
			SteamAvatarLoader._cache[steamId].Add(type, t);
		}

		private static readonly Dictionary<CSteamID, SteamAvatarLoader.UserAvatarData> _cache = new Dictionary<CSteamID, SteamAvatarLoader.UserAvatarData>();

		private static ulong avatarTaskCount;

		private static readonly Dictionary<ulong, Callback<AvatarImageLoaded_t>> AvatarTaskList = new Dictionary<ulong, Callback<AvatarImageLoaded_t>>();

		public enum AvatarTypes : byte
		{
			Large,
			Medium,
			Small
		}

		private class UserAvatarData
		{
			public void Add(SteamAvatarLoader.AvatarTypes type, Texture2D texture)
			{
				this._cache[type] = texture;
			}

			public Texture2D GetTexture(SteamAvatarLoader.AvatarTypes type)
			{
				return (!this._cache.ContainsKey(type)) ? null : this._cache[type];
			}

			private Dictionary<SteamAvatarLoader.AvatarTypes, Texture2D> _cache = new Dictionary<SteamAvatarLoader.AvatarTypes, Texture2D>();
		}
	}
}
