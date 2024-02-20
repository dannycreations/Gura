using System;
using System.IO;
using ExitGames.Client.Photon;
using ObjectModel;

namespace TPM
{
	public class CharacterInfo
	{
		public static Hashtable ToHashtable(string id, CharacterEventType eventType, Package package)
		{
			if (package.Length == 0)
			{
				return null;
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add(1, (byte)eventType);
			hashtable.Add(2, id);
			Hashtable hashtable2 = hashtable;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				ThirdPersonData.WriteHeader(memoryStream);
				for (int i = 0; i < package.Length; i++)
				{
					package[i].SerializeToStream(memoryStream);
				}
				hashtable2[3] = memoryStream.ToArray();
			}
			return hashtable2;
		}

		public static CharacterInfo FromHashtable(Hashtable hashtable)
		{
			byte[] array = (byte[])hashtable[3];
			return new CharacterInfo
			{
				Id = (string)hashtable[2],
				Package = ThirdPersonData.DeserializePackage(array)
			};
		}

		private const int LOG_INTERVAL = 10;

		public string Id;

		public string UserName;

		public Package Package;

		public Player PlayerData;
	}
}
