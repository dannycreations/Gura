using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace DeltaDNA
{
	internal static class Utils
	{
		public static Dictionary<K, V> HashtableToDictionary<K, V>(Hashtable table)
		{
			Dictionary<K, V> dictionary = new Dictionary<K, V>();
			IDictionaryEnumerator enumerator = table.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
					dictionary.Add((K)((object)dictionaryEntry.Key), (V)((object)dictionaryEntry.Value));
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			return dictionary;
		}

		public static Dictionary<K, V> HashtableToDictionary<K, V>(Dictionary<K, V> dictionary)
		{
			return dictionary;
		}

		public static byte[] ComputeMD5Hash(byte[] buffer)
		{
			MD5CryptoServiceProvider md5CryptoServiceProvider = new MD5CryptoServiceProvider();
			return md5CryptoServiceProvider.ComputeHash(buffer);
		}

		public static bool FileExists(string path)
		{
			return Singleton<DDNA>.Instance.Settings.UseEventStore && File.Exists(path);
		}

		public static bool DirectoryExists(string path)
		{
			return Singleton<DDNA>.Instance.Settings.UseEventStore && Directory.Exists(path);
		}

		public static void CreateDirectory(string path)
		{
			if (Singleton<DDNA>.Instance.Settings.UseEventStore)
			{
				Directory.CreateDirectory(path);
			}
		}

		public static Stream CreateStream(string path)
		{
			if (Singleton<DDNA>.Instance.Settings.UseEventStore)
			{
				Logger.LogDebug("Creating file based stream");
				return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			}
			Logger.LogDebug("Creating memory based stream");
			return new MemoryStream();
		}

		public static Stream OpenStream(string path)
		{
			if (Singleton<DDNA>.Instance.Settings.UseEventStore)
			{
				Logger.LogDebug("Opening file based stream");
				return new FileStream(path, FileMode.Open, FileAccess.Read);
			}
			Logger.LogDebug("Opening memory based stream");
			return new MemoryStream();
		}
	}
}
