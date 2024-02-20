using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Photon.Interfaces.Profile;

namespace ObjectModel
{
	public static class ProfileSerializationHelper
	{
		public static Dictionary<byte, object> Serialize(Profile profile)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			FishCageContents fishCage = profile.FishCage;
			InventoryRodSetups inventoryRodSetups = profile.InventoryRodSetups;
			List<Player> friends = profile.Friends;
			List<PlayerLicense> licenses = profile.Licenses;
			List<LevelLockRemoval> levelLockRemovals = profile.LevelLockRemovals;
			Inventory inventory = profile.Inventory;
			string text;
			try
			{
				profile.FishCage = null;
				profile.InventoryRodSetups = null;
				profile.Friends = null;
				profile.Licenses = null;
				profile.LevelLockRemovals = null;
				profile.Inventory = null;
				text = JsonConvert.SerializeObject(profile, 0, SerializationHelper.JsonSerializerSettings);
				text = ProfileSerializationHelper.SerializeRodsStripped(text, inventory);
			}
			finally
			{
				profile.FishCage = fishCage;
				profile.InventoryRodSetups = inventoryRodSetups;
				profile.Friends = friends;
				profile.Licenses = licenses;
				profile.LevelLockRemovals = levelLockRemovals;
				profile.Inventory = inventory;
			}
			dictionary[10] = CompressHelper.CompressString(text);
			if (profile.FishCage != null && profile.FishCage.Count > 0)
			{
				text = JsonConvert.SerializeObject(profile.FishCage, 0, SerializationHelper.JsonSerializerSettings);
				dictionary[178] = CompressHelper.CompressString(text);
			}
			profile.WriteToDictionary(dictionary, typeof(ProfileParameterCode));
			return dictionary;
		}

		private static string SerializeRodsStripped(string profileJson, Inventory inventory)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(profileJson.Substring(0, profileJson.Length - 1));
			stringBuilder.Append(",\"Inventory\":{\"Items\":[");
			bool flag = true;
			foreach (Rod rod in inventory.Items.OfType<Rod>())
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append("{\"$type\":\"ObjectModel.Rod\",\"InstanceId\":\"");
				stringBuilder.Append(rod.InstanceId);
				stringBuilder.Append("\",\"Slot\":");
				stringBuilder.Append(rod.Slot);
				stringBuilder.Append("}");
			}
			stringBuilder.Append("]}}");
			return stringBuilder.ToString();
		}

		public static Profile Deserialize(Dictionary<byte, object> parameters)
		{
			object obj;
			Profile profile;
			if (parameters.TryGetValue(10, out obj))
			{
				profile = ProfileSerializationHelper.Deserialize((byte[])obj);
			}
			else
			{
				profile = new Profile();
			}
			profile.Init();
			profile.ReadFromDictionary(parameters, typeof(ProfileParameterCode));
			if (parameters.TryGetValue(180, out obj))
			{
				profile.Stats = ProfileSerializationHelper.DeserializeStats((byte[])obj);
			}
			if (parameters.TryGetValue(178, out obj))
			{
				profile.FishCage = ProfileSerializationHelper.DeserializeFishCage((byte[])obj);
			}
			return profile;
		}

		public static Profile Deserialize(byte[] encodedJson)
		{
			string text = CompressHelper.DecompressString(encodedJson);
			return JsonConvert.DeserializeObject<Profile>(text, SerializationHelper.JsonSerializerSettings);
		}

		public static PlayerStats DeserializeStats(byte[] encodedJson)
		{
			string text = CompressHelper.DecompressString(encodedJson);
			return JsonConvert.DeserializeObject<PlayerStats>(text, SerializationHelper.JsonSerializerSettings);
		}

		public static FishCageContents DeserializeFishCage(byte[] encodedJson)
		{
			string text = CompressHelper.DecompressString(encodedJson);
			return JsonConvert.DeserializeObject<FishCageContents>(text, SerializationHelper.JsonSerializerSettings);
		}

		public static Profile Deserialize(string json)
		{
			return JsonConvert.DeserializeObject<Profile>(json, SerializationHelper.JsonSerializerSettings);
		}
	}
}
