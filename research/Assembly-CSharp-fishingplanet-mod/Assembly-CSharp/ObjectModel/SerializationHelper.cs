using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Photon.Interfaces;
using UnityEngine;

namespace ObjectModel
{
	public static class SerializationHelper
	{
		public static void MakeEqualTo(this object destination, object source)
		{
			foreach (PropertyInfo propertyInfo in destination.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				PropertyInfo property = source.GetType().GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
				if (property != null && propertyInfo.CanWrite && property.CanRead && !propertyInfo.PropertyType.IsArray && !property.PropertyType.IsArray && (propertyInfo.PropertyType == property.PropertyType || Nullable.GetUnderlyingType(propertyInfo.PropertyType) == property.PropertyType || Nullable.GetUnderlyingType(property.PropertyType) == propertyInfo.PropertyType))
				{
					propertyInfo.SetValue(destination, property.GetValue(source, null), null);
				}
			}
		}

		public static void MakeCloneOf(this object destination, object source, bool fullClone = false)
		{
			PropertyInfo[] properties = destination.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			int i = 0;
			while (i < properties.Length)
			{
				PropertyInfo propertyInfo = properties[i];
				if (fullClone)
				{
					goto IL_57;
				}
				if (!propertyInfo.GetCustomAttributes(true).OfType<NoCloneAttribute>().Any((NoCloneAttribute a) => a.NoClone))
				{
					goto IL_57;
				}
				IL_D4:
				i++;
				continue;
				IL_57:
				PropertyInfo property = source.GetType().GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
				if (property != null && propertyInfo.CanWrite && property.CanRead && (propertyInfo.PropertyType == property.PropertyType || Nullable.GetUnderlyingType(propertyInfo.PropertyType) == property.PropertyType || Nullable.GetUnderlyingType(property.PropertyType) == propertyInfo.PropertyType))
				{
					propertyInfo.SetValue(destination, property.GetValue(source, null), null);
					goto IL_D4;
				}
				goto IL_D4;
			}
		}

		public static string CheckEqual(this object obj1, object obj2, string[] props2Compare = null)
		{
			if (obj1.GetType() != obj2.GetType())
			{
				return string.Format("Object types differs: {0} and {1}", obj1.GetType(), obj2.GetType());
			}
			string text = string.Empty;
			foreach (PropertyInfo propertyInfo in obj1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				bool flag = false;
				if (props2Compare != null)
				{
					foreach (string text2 in props2Compare)
					{
						if (text2 == propertyInfo.Name)
						{
							flag = true;
							break;
						}
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					if (propertyInfo.GetIndexParameters().Length <= 0 && !propertyInfo.PropertyType.IsClass)
					{
						object value = propertyInfo.GetValue(obj1, null);
						object value2 = propertyInfo.GetValue(obj2, null);
						if (value != null || value2 != null)
						{
							if ((value == null && value2 != null) || (value != null && value2 == null) || !SerializationHelper.ValuesAreEqual(propertyInfo.PropertyType, value, value2))
							{
								text += string.Format("Difference: property-{0}, val.required-{1}, val.actual-{2}\r\n", propertyInfo.Name, value, value2);
							}
						}
					}
				}
			}
			return text;
		}

		private static bool ValuesAreEqual(Type type, object value1, object value2)
		{
			if (type == typeof(DateTime) || Nullable.GetUnderlyingType(type) == typeof(DateTime))
			{
				DateTime dateTime = (DateTime)value1;
				DateTime dateTime2 = (DateTime)value2;
				return dateTime.Year == dateTime2.Year && dateTime.Month == dateTime2.Month && dateTime.Day == dateTime2.Day && dateTime.Hour == dateTime2.Hour && dateTime.Minute == dateTime2.Minute && dateTime.Second == dateTime2.Second;
			}
			return value1.Equals(value2);
		}

		public static void ReadFromDictionary(this object obj, Dictionary<byte, object> parameters, Type enumType)
		{
			Type type = obj.GetType();
			foreach (byte b in parameters.Keys)
			{
				string name = Enum.GetName(enumType, b);
				if (string.IsNullOrEmpty(name))
				{
					break;
				}
				object obj2 = parameters[b];
				PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
				if (property != null && property.CanWrite)
				{
					Type type2 = obj2.GetType();
					if (type2 == typeof(string) && property.PropertyType == typeof(Guid))
					{
						property.SetValue(obj, new Guid((string)obj2), null);
					}
					else if (type2 == typeof(string) && (property.PropertyType == typeof(DateTime) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(DateTime)))
					{
						string text = (string)obj2;
						if (!string.IsNullOrEmpty(text))
						{
							if (text.Length == 8)
							{
								property.SetValue(obj, DateSerializer.ToDate(text), null);
							}
							else
							{
								property.SetValue(obj, DateSerializer.ToDateTime(text), null);
							}
						}
					}
					else
					{
						if (property.PropertyType != type2 && Nullable.GetUnderlyingType(property.PropertyType) != type2)
						{
							throw new InvalidOperationException(string.Format("Can't assign property {0} from Dictionary to Object. Reader type is {1}, Dto type is {2}", name, obj2.GetType(), property.PropertyType));
						}
						property.SetValue(obj, obj2, null);
					}
				}
			}
		}

		public static void WriteToDictionary(this object obj, Dictionary<byte, object> parameters, Type enumType)
		{
			Type type = obj.GetType();
			string[] names = Enum.GetNames(enumType);
			foreach (string text in names)
			{
				byte b = (byte)Enum.Parse(enumType, text);
				PropertyInfo property = type.GetProperty(text, BindingFlags.Instance | BindingFlags.Public);
				if (property != null && property.CanRead)
				{
					if (property.PropertyType == typeof(Guid))
					{
						parameters[b] = property.GetGetMethod().Invoke(obj, null).ToString();
					}
					else if (property.PropertyType == typeof(DateTime) || Nullable.GetUnderlyingType(property.PropertyType) == typeof(DateTime))
					{
						object obj2 = property.GetGetMethod().Invoke(obj, null);
						if (obj2 != null)
						{
							DateTime dateTime = (DateTime)obj2;
							if (dateTime.TimeOfDay.TotalSeconds > 0.0)
							{
								parameters[b] = DateSerializer.FromDateTime(new DateTime?(dateTime));
							}
							else
							{
								parameters[b] = DateSerializer.FromDate(new DateTime?(dateTime));
							}
						}
					}
					else
					{
						parameters[b] = property.GetGetMethod().Invoke(obj, null);
					}
				}
			}
		}

		public static JsonSerializerSettings JsonSerializerSettings
		{
			get
			{
				JsonSerializerSettings jsonSerializerSettings;
				if ((jsonSerializerSettings = SerializationHelper.jsonSerializerSettings) == null)
				{
					jsonSerializerSettings = (SerializationHelper.jsonSerializerSettings = SerializationHelper.JsonSerializerSettingsClone());
				}
				return jsonSerializerSettings;
			}
		}

		public static JsonSerializerSettings JsonSerializerSettingsClone()
		{
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.NullValueHandling = 1;
			jsonSerializerSettings.TypeNameHandling = 4;
			jsonSerializerSettings.Culture = CultureInfo.InvariantCulture;
			jsonSerializerSettings.Error = delegate(object o, ErrorEventArgs a)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"JSON error ",
					a.ErrorContext.Error.Message,
					" of type ",
					a.ErrorContext.Error.GetType().Name,
					" at ",
					a.ErrorContext.Error.StackTrace
				}));
			};
			JsonSerializerSettings jsonSerializerSettings2 = jsonSerializerSettings;
			if (SerializationHelper.jsonSerializerSettings != null && SerializationHelper.jsonSerializerSettings.Binder != null)
			{
				jsonSerializerSettings2.Binder = SerializationHelper.jsonSerializerSettings.Binder;
			}
			return jsonSerializerSettings2;
		}

		public static JsonSerializerSettings JsonConfigSerializerSettings
		{
			get
			{
				if (SerializationHelper.jsonConfigSerializerSettings == null)
				{
					SerializationHelper.jsonConfigSerializerSettings = SerializationHelper.JsonSerializerSettingsClone();
					SerializationHelper.jsonConfigSerializerSettings.NullValueHandling = 1;
					SerializationHelper.jsonConfigSerializerSettings.ContractResolver = new JsonConfigContractResolver();
				}
				return SerializationHelper.jsonConfigSerializerSettings;
			}
		}

		public static JsonSerializerSettings MissionHintSerializerSettings
		{
			get
			{
				if (SerializationHelper.missionHintSerializerSettings == null)
				{
					SerializationHelper.missionHintSerializerSettings = SerializationHelper.JsonSerializerSettingsClone();
					SerializationHelper.missionHintSerializerSettings.NullValueHandling = 1;
					SerializationHelper.missionHintSerializerSettings.ContractResolver = new HintMessageContractResolver();
				}
				return SerializationHelper.missionHintSerializerSettings;
			}
		}

		public static string FloatToString(float value)
		{
			return value.ToString("#0.0#").Replace(',', '.');
		}

		public static string SerializeGift(char request, string userId, string userName, string itemJson)
		{
			return string.Format("{0}{1},{2},{3}", new object[]
			{
				request,
				userId,
				userName.Replace(",", string.Empty),
				itemJson
			});
		}

		public static bool DeserializeGift(string data, out string userId, out string userName, out string itemJson)
		{
			bool flag;
			try
			{
				int num = data.IndexOf(',');
				userId = data.Substring(1, num - 1);
				int num2 = data.IndexOf(',', num + 1);
				userName = data.Substring(num + 1, num2 - num - 1);
				itemJson = data.Substring(num2 + 1);
				flag = true;
			}
			catch
			{
				userId = null;
				userName = null;
				itemJson = null;
				flag = false;
			}
			return flag;
		}

		public static string SerializeFriend(char addToFriendsRequest, Player friend)
		{
			string text = "{0}{1},{2},{3},{4},{5},{6},{7},{8},{9}";
			object[] array = new object[10];
			array[0] = addToFriendsRequest;
			array[1] = friend.UserName;
			int num = 2;
			int? avatarBID = friend.AvatarBID;
			array[num] = ((avatarBID == null) ? 0 : avatarBID.Value);
			array[3] = friend.Gender;
			array[4] = ((!friend.HasPremium) ? 0 : 1);
			array[5] = ((!friend.IsOnline) ? 0 : 1);
			array[6] = friend.Level;
			array[7] = friend.Rank;
			int num2 = 8;
			int? pondId = friend.PondId;
			array[num2] = ((pondId == null) ? 0 : pondId.Value);
			array[9] = friend.RoomId ?? "-";
			return string.Format(text, array);
		}

		public static void DeserializeFriend(Player player, string data)
		{
			string[] array = data.Substring(1).Split(new char[] { ',' });
			if (array.Length != 9)
			{
				return;
			}
			player.UserName = array[0];
			player.AvatarBID = new int?(int.Parse(array[1]));
			player.Gender = array[2];
			player.HasPremium = array[3] == "1";
			player.IsOnline = array[4] == "1";
			player.Level = int.Parse(array[5]);
			player.Rank = int.Parse(array[6]);
			player.PondId = ((!(array[7] == "0")) ? new int?(int.Parse(array[7])) : null);
			player.RoomId = ((!(array[8] == "-")) ? array[8] : null);
		}

		public static string SerializeRodSetup(char request, string userId, string userName, string itemJson)
		{
			return string.Format("{0}{1},{2},{3}", new object[]
			{
				request,
				userId,
				userName.Replace(",", string.Empty),
				itemJson
			});
		}

		public static bool DeserializeRodSetup(string data, out string userId, out string userName, out string itemJson)
		{
			bool flag;
			try
			{
				int num = data.IndexOf(',');
				userId = data.Substring(1, num - 1);
				int num2 = data.IndexOf(',', num + 1);
				userName = data.Substring(num + 1, num2 - num - 1);
				itemJson = data.Substring(num2 + 1);
				flag = true;
			}
			catch
			{
				userId = null;
				userName = null;
				itemJson = null;
				flag = false;
			}
			return flag;
		}

		public static string SerializePoint(Point3 p)
		{
			return string.Format("{0}~{1}~{2}", p.X, p.Y, p.Z);
		}

		public static Point3 DeserializePoint(string s)
		{
			string[] array = s.Split(new char[] { '~' });
			if (array.Length != 3)
			{
				throw new FormatException("Can't deserialize Point3 from " + s);
			}
			return new Point3(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture));
		}

		public static byte[] SerializeStream(Stream stream)
		{
			byte[] array;
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				array = CompressHelper.CompressArray(binaryReader.ReadBytes((int)stream.Length));
			}
			return array;
		}

		public static byte[] SerializeFeedings(IEnumerable<Feeding> feedings)
		{
			List<byte> list = new List<byte>();
			foreach (Feeding feeding in feedings)
			{
				list.AddRange(feeding.ItemId.ToByteArray());
				list.AddRange(BitConverter.GetBytes(feeding.Position.X));
				list.AddRange(BitConverter.GetBytes(feeding.Position.Y));
				list.AddRange(BitConverter.GetBytes(feeding.Position.Z));
				byte b = (byte)(((!feeding.IsNew) ? 0 : 1) | ((!feeding.IsDestroyed) ? 0 : 2));
				list.Add(b);
			}
			return list.ToArray();
		}

		public static byte[] SerializePlayerDetractor(PlayerDetractorData data)
		{
			byte[] array = new byte[PlayerDetractorData.Size];
			Array.Copy(BitConverter.GetBytes(data.Position.x), array, 4);
			Array.Copy(BitConverter.GetBytes(data.Position.z), 0, array, 4, 4);
			Array.Copy(BitConverter.GetBytes(data.Radius), 0, array, 8, 4);
			Array.Copy(BitConverter.GetBytes(data.Duration), 0, array, 12, 4);
			return array;
		}

		public static PlayerDetractorData DeserializePlayerDetractor(byte[] data)
		{
			float num = BitConverter.ToSingle(data, 0);
			float num2 = BitConverter.ToSingle(data, 4);
			float num3 = BitConverter.ToSingle(data, 8);
			float num4 = BitConverter.ToSingle(data, 12);
			return new PlayerDetractorData(new Vector3f(num, 0f, num2), num3, num4);
		}

		public static IEnumerable<Feeding> DeserializeFeedings(byte[] feedingArray)
		{
			if (feedingArray == null || feedingArray.Length == 0)
			{
				yield break;
			}
			if (feedingArray.Length % 29 != 0)
			{
				throw new InvalidOperationException(string.Concat(new object[] { "Unable to deserialize feeding array. Ivalid array size (x", 29, ") = ", feedingArray.Length }));
			}
			for (int i = 0; i < feedingArray.Length; i += 29)
			{
				byte[] guid = new byte[16];
				Array.Copy(feedingArray, i, guid, 0, 16);
				yield return new Feeding
				{
					ItemId = new Guid(guid),
					Position = new Point3
					{
						X = BitConverter.ToSingle(feedingArray, i + 16),
						Y = BitConverter.ToSingle(feedingArray, i + 20),
						Z = BitConverter.ToSingle(feedingArray, i + 24)
					},
					IsNew = ((feedingArray[i + 28] & 1) != 0),
					IsDestroyed = ((feedingArray[i + 28] & 2) != 0)
				};
			}
			yield break;
		}

		public static float[] DeserializeSounderInfo(byte[] info)
		{
			if (info == null)
			{
				return null;
			}
			float[] array = new float[info.Length / 4];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = BitConverter.ToSingle(info, i * 4);
			}
			return array;
		}

		public static int? DeserializeProfileFlag(string request)
		{
			if (request.Length == 1)
			{
				return null;
			}
			return new int?(int.Parse(request.Substring(1)));
		}

		private static JsonSerializerSettings jsonSerializerSettings;

		private static JsonSerializerSettings jsonConfigSerializerSettings;

		private static JsonSerializerSettings missionHintSerializerSettings;

		private const int FeedingBinaryLength = 29;
	}
}
