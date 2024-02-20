using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectModel
{
	public static class ReelSettingsPersister
	{
		public static ReelSettingsPersister.ReelSetting GetReelSettings(Dictionary<string, string> settings, string reelInstanceId)
		{
			if (!settings.ContainsKey("ReelsSetting") || string.IsNullOrEmpty(settings["ReelsSetting"]))
			{
				return ReelSettingsPersister.DefaultReelSetting;
			}
			IList<ReelSettingsPersister.ReelSetting> list = ReelSettingsPersister.DeserializeReelSettings(settings["ReelsSetting"]);
			return ReelSettingsPersister.GetReelSetting(list, reelInstanceId);
		}

		public static void SetReelFriction(Dictionary<string, string> settings, string reelInstanceId, int value)
		{
			IList<ReelSettingsPersister.ReelSetting> list;
			if (!settings.ContainsKey("ReelsSetting") || string.IsNullOrEmpty(settings["ReelsSetting"]))
			{
				list = new List<ReelSettingsPersister.ReelSetting>();
			}
			else
			{
				list = ReelSettingsPersister.DeserializeReelSettings(settings["ReelsSetting"]);
			}
			ReelSettingsPersister.SetFrictionSetting(list, reelInstanceId, value);
			settings["ReelsSetting"] = ReelSettingsPersister.SerializeReelSettings(list);
		}

		public static void SetReelSpeed(Dictionary<string, string> settings, string reelInstanceId, int value)
		{
			IList<ReelSettingsPersister.ReelSetting> list;
			if (!settings.ContainsKey("ReelsSetting") || string.IsNullOrEmpty(settings["ReelsSetting"]))
			{
				list = new List<ReelSettingsPersister.ReelSetting>();
			}
			else
			{
				list = ReelSettingsPersister.DeserializeReelSettings(settings["ReelsSetting"]);
			}
			ReelSettingsPersister.SetSpeedSetting(list, reelInstanceId, value);
			settings["ReelsSetting"] = ReelSettingsPersister.SerializeReelSettings(list);
		}

		public static void PackReelSettings(Dictionary<string, string> settings, IList<string> existingReelIds)
		{
			if (!settings.ContainsKey("ReelsSetting") || string.IsNullOrEmpty(settings["ReelsSetting"]))
			{
				return;
			}
			IList<ReelSettingsPersister.ReelSetting> list = ReelSettingsPersister.DeserializeReelSettings(settings["ReelsSetting"]);
			for (int i = 0; i < list.Count; i++)
			{
				ReelSettingsPersister.ReelSetting reelSetting = list[i];
				bool flag = false;
				for (int j = 0; j < existingReelIds.Count; j++)
				{
					if (reelSetting.ReelInstanceId.Equals(existingReelIds[j], StringComparison.InvariantCultureIgnoreCase))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Remove(reelSetting);
				}
			}
			settings["ReelsSetting"] = ReelSettingsPersister.SerializeReelSettings(list);
		}

		private static void SetFrictionSetting(IList<ReelSettingsPersister.ReelSetting> settings, string insanceId, int value)
		{
			bool flag = false;
			ReelSettingsPersister.ReelSetting reelSetting = null;
			for (int i = 0; i < settings.Count; i++)
			{
				if (settings[i].ReelInstanceId == insanceId)
				{
					reelSetting = settings[i];
					reelSetting.Friction = value;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (value == 3 && reelSetting != null && reelSetting.Speed == 1)
				{
					settings.Remove(reelSetting);
				}
			}
			else
			{
				settings.Add(new ReelSettingsPersister.ReelSetting
				{
					ReelInstanceId = insanceId,
					Friction = value,
					Speed = 1
				});
			}
		}

		private static void SetSpeedSetting(IList<ReelSettingsPersister.ReelSetting> settings, string insanceId, int value)
		{
			bool flag = false;
			ReelSettingsPersister.ReelSetting reelSetting = null;
			for (int i = 0; i < settings.Count; i++)
			{
				if (settings[i].ReelInstanceId == insanceId)
				{
					reelSetting = settings[i];
					reelSetting.Speed = value;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (value == 1 && reelSetting != null && reelSetting.Friction == 3)
				{
					settings.Remove(reelSetting);
				}
			}
			else
			{
				settings.Add(new ReelSettingsPersister.ReelSetting
				{
					ReelInstanceId = insanceId,
					Friction = 3,
					Speed = value
				});
			}
		}

		private static ReelSettingsPersister.ReelSetting GetReelSetting(IList<ReelSettingsPersister.ReelSetting> settings, string insanceId)
		{
			for (int i = 0; i < settings.Count; i++)
			{
				if (settings[i].ReelInstanceId == insanceId)
				{
					return settings[i];
				}
			}
			return ReelSettingsPersister.DefaultReelSetting;
		}

		private static string SerializeReelSettings(IList<ReelSettingsPersister.ReelSetting> settings)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < settings.Count; i++)
			{
				ReelSettingsPersister.ReelSetting reelSetting = settings[i];
				if (stringBuilder.Length == 0)
				{
					stringBuilder.AppendFormat("{0},{1},{2}", reelSetting.ReelInstanceId, reelSetting.Friction, reelSetting.Speed);
				}
				else
				{
					stringBuilder.AppendFormat(",{0},{1},{2}", reelSetting.ReelInstanceId, reelSetting.Friction, reelSetting.Speed);
				}
			}
			return stringBuilder.ToString();
		}

		private static IList<ReelSettingsPersister.ReelSetting> DeserializeReelSettings(string settings)
		{
			string[] array = settings.Split(new char[] { ',' });
			if (array.Length % 3 != 0)
			{
				return new List<ReelSettingsPersister.ReelSetting>();
			}
			List<ReelSettingsPersister.ReelSetting> list = new List<ReelSettingsPersister.ReelSetting>();
			for (int i = 0; i < array.Length / 3; i++)
			{
				string text = array[i * 3];
				string text2 = array[i * 3 + 1];
				string text3 = array[i * 3 + 2];
				int num;
				int num2;
				if (text.Length != 36 || !int.TryParse(text2, out num) || !int.TryParse(text3, out num2))
				{
					break;
				}
				list.Add(new ReelSettingsPersister.ReelSetting
				{
					ReelInstanceId = text,
					Friction = num,
					Speed = num2
				});
			}
			return list;
		}

		private const string ReelsSetting = "ReelsSetting";

		public const int DefaultFrictionSection = 3;

		public const int DefaultSpeedSection = 1;

		private static readonly ReelSettingsPersister.ReelSetting DefaultReelSetting = new ReelSettingsPersister.ReelSetting
		{
			Friction = 3,
			Speed = 1
		};

		public class ReelSetting
		{
			public string ReelInstanceId;

			public int Friction;

			public int Speed;
		}
	}
}
