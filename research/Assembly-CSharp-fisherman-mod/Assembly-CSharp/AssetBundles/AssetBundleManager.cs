using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleManager : MonoBehaviour
	{
		public static AssetBundleManager.LogMode logMode
		{
			get
			{
				return AssetBundleManager.m_LogMode;
			}
			set
			{
				AssetBundleManager.m_LogMode = value;
			}
		}

		public static string BaseDownloadingURL
		{
			get
			{
				return AssetBundleManager.m_BaseDownloadingURL;
			}
			set
			{
				AssetBundleManager.m_BaseDownloadingURL = value;
			}
		}

		public static string[] ActiveVariants
		{
			get
			{
				return AssetBundleManager.m_ActiveVariants;
			}
			set
			{
				AssetBundleManager.m_ActiveVariants = value;
			}
		}

		public static AssetBundleManifest AssetBundleManifestObject
		{
			set
			{
				AssetBundleManager.m_AssetBundleManifest = value;
			}
		}

		private static void Log(AssetBundleManager.LogType logType, string text)
		{
			if (logType == AssetBundleManager.LogType.Error)
			{
				Debug.LogError("[AssetBundleManager] " + text);
			}
			else if (AssetBundleManager.m_LogMode == AssetBundleManager.LogMode.All)
			{
				Debug.Log("[AssetBundleManager] " + text);
			}
		}

		private static string GetStreamingAssetsPath()
		{
			if (Application.platform == 2 || Application.platform == 7)
			{
				return "file:///" + Application.dataPath + "/../";
			}
			if (Application.platform == 1)
			{
				return "file:///" + Application.dataPath;
			}
			if (Application.platform == 13)
			{
				return "file:///" + Application.dataPath;
			}
			return "file:///" + Application.dataPath + "/../";
		}

		public static void SetSourceAssetBundleDirectory(string relativePath)
		{
			AssetBundleManager.BaseDownloadingURL = AssetBundleManager.GetStreamingAssetsPath() + relativePath + Utility.GetPlatformName() + "/";
		}

		public static void SetSourceAssetBundleURL(string absolutePath)
		{
			AssetBundleManager.BaseDownloadingURL = absolutePath + Utility.GetPlatformName() + "/";
		}

		public static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
		{
			if (AssetBundleManager.m_DownloadingErrors.TryGetValue(assetBundleName, out error))
			{
				return null;
			}
			LoadedAssetBundle loadedAssetBundle = null;
			AssetBundleManager.m_LoadedAssetBundles.TryGetValue(assetBundleName, out loadedAssetBundle);
			if (loadedAssetBundle == null)
			{
				return null;
			}
			string[] array = null;
			if (!AssetBundleManager.m_Dependencies.TryGetValue(assetBundleName, out array))
			{
				return loadedAssetBundle;
			}
			foreach (string text in array)
			{
				if (AssetBundleManager.m_DownloadingErrors.TryGetValue(assetBundleName, out error))
				{
					return loadedAssetBundle;
				}
				LoadedAssetBundle loadedAssetBundle2;
				AssetBundleManager.m_LoadedAssetBundles.TryGetValue(text, out loadedAssetBundle2);
				if (loadedAssetBundle2 == null)
				{
					return null;
				}
			}
			return loadedAssetBundle;
		}

		public static AssetBundleLoadManifestOperation Initialize()
		{
			return AssetBundleManager.Initialize(Utility.GetPlatformName());
		}

		public static AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
		{
			GameObject gameObject = new GameObject("AssetBundleManager", new Type[] { typeof(AssetBundleManager) });
			Object.DontDestroyOnLoad(gameObject);
			AssetBundleManager.LoadAssetBundle(manifestAssetBundleName, true);
			AssetBundleLoadManifestOperation assetBundleLoadManifestOperation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
			AssetBundleManager.m_InProgressOperations.Add(assetBundleLoadManifestOperation);
			return assetBundleLoadManifestOperation;
		}

		protected static void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
		{
			AssetBundleManager.Log(AssetBundleManager.LogType.Info, "Loading Asset Bundle " + ((!isLoadingAssetBundleManifest) ? ": " : "Manifest: ") + assetBundleName);
			if (!isLoadingAssetBundleManifest && AssetBundleManager.m_AssetBundleManifest == null)
			{
				Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
				return;
			}
			if (!AssetBundleManager.LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest) && !isLoadingAssetBundleManifest)
			{
				AssetBundleManager.LoadDependencies(assetBundleName);
			}
		}

		protected static string RemapVariantName(string assetBundleName)
		{
			string[] allAssetBundlesWithVariant = AssetBundleManager.m_AssetBundleManifest.GetAllAssetBundlesWithVariant();
			string[] array = assetBundleName.Split(new char[] { '.' });
			int num = int.MaxValue;
			int num2 = -1;
			for (int i = 0; i < allAssetBundlesWithVariant.Length; i++)
			{
				string[] array2 = allAssetBundlesWithVariant[i].Split(new char[] { '.' });
				if (!(array2[0] != array[0]))
				{
					int num3 = Array.IndexOf<string>(AssetBundleManager.m_ActiveVariants, array2[1]);
					if (num3 == -1)
					{
						num3 = 2147483646;
					}
					if (num3 < num)
					{
						num = num3;
						num2 = i;
					}
				}
			}
			if (num == 2147483646)
			{
				Debug.LogWarning("Ambigious asset bundle variant chosen because there was no matching active variant: " + allAssetBundlesWithVariant[num2]);
			}
			if (num2 != -1)
			{
				return allAssetBundlesWithVariant[num2];
			}
			return assetBundleName;
		}

		protected static bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
		{
			LoadedAssetBundle loadedAssetBundle = null;
			AssetBundleManager.m_LoadedAssetBundles.TryGetValue(assetBundleName, out loadedAssetBundle);
			if (loadedAssetBundle != null)
			{
				loadedAssetBundle.m_ReferencedCount++;
				return true;
			}
			if (AssetBundleManager.m_DownloadingWWWs.ContainsKey(assetBundleName))
			{
				return true;
			}
			string text = AssetBundleManager.m_BaseDownloadingURL + assetBundleName;
			WWW www;
			if (isLoadingAssetBundleManifest)
			{
				www = new WWW(text);
			}
			else
			{
				www = WWW.LoadFromCacheOrDownload(text, AssetBundleManager.m_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0U);
			}
			AssetBundleManager.m_DownloadingWWWs.Add(assetBundleName, www);
			return false;
		}

		protected static void LoadDependencies(string assetBundleName)
		{
			if (AssetBundleManager.m_AssetBundleManifest == null)
			{
				Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
				return;
			}
			string[] allDependencies = AssetBundleManager.m_AssetBundleManifest.GetAllDependencies(assetBundleName);
			if (allDependencies.Length == 0)
			{
				return;
			}
			for (int i = 0; i < allDependencies.Length; i++)
			{
				allDependencies[i] = AssetBundleManager.RemapVariantName(allDependencies[i]);
			}
			AssetBundleManager.m_Dependencies.Add(assetBundleName, allDependencies);
			for (int j = 0; j < allDependencies.Length; j++)
			{
				AssetBundleManager.LoadAssetBundleInternal(allDependencies[j], false);
			}
		}

		public static void UnloadAssetBundle(string assetBundleName)
		{
			AssetBundleManager.UnloadAssetBundleInternal(assetBundleName);
			AssetBundleManager.UnloadDependencies(assetBundleName);
		}

		protected static void UnloadDependencies(string assetBundleName)
		{
			string[] array = null;
			if (!AssetBundleManager.m_Dependencies.TryGetValue(assetBundleName, out array))
			{
				return;
			}
			foreach (string text in array)
			{
				AssetBundleManager.UnloadAssetBundleInternal(text);
			}
			AssetBundleManager.m_Dependencies.Remove(assetBundleName);
		}

		protected static void UnloadAssetBundleInternal(string assetBundleName)
		{
			string text;
			LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleName, out text);
			if (loadedAssetBundle == null)
			{
				return;
			}
			if (--loadedAssetBundle.m_ReferencedCount == 0)
			{
				loadedAssetBundle.m_AssetBundle.Unload(false);
				AssetBundleManager.m_LoadedAssetBundles.Remove(assetBundleName);
				AssetBundleManager.Log(AssetBundleManager.LogType.Info, assetBundleName + " has been unloaded successfully");
			}
		}

		private void Update()
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, WWW> keyValuePair in AssetBundleManager.m_DownloadingWWWs)
			{
				WWW value = keyValuePair.Value;
				if (value.error != null)
				{
					AssetBundleManager.m_DownloadingErrors.Add(keyValuePair.Key, string.Format("Failed downloading bundle {0} from {1}: {2}", keyValuePair.Key, value.url, value.error));
					list.Add(keyValuePair.Key);
				}
				else if (value.isDone)
				{
					AssetBundle assetBundle = value.assetBundle;
					if (assetBundle == null)
					{
						AssetBundleManager.m_DownloadingErrors.Add(keyValuePair.Key, string.Format("{0} is not a valid asset bundle.", keyValuePair.Key));
						list.Add(keyValuePair.Key);
					}
					else
					{
						AssetBundleManager.m_LoadedAssetBundles.Add(keyValuePair.Key, new LoadedAssetBundle(value.assetBundle));
						list.Add(keyValuePair.Key);
					}
				}
			}
			foreach (string text in list)
			{
				WWW www = AssetBundleManager.m_DownloadingWWWs[text];
				AssetBundleManager.m_DownloadingWWWs.Remove(text);
				www.Dispose();
			}
			int i = 0;
			while (i < AssetBundleManager.m_InProgressOperations.Count)
			{
				if (!AssetBundleManager.m_InProgressOperations[i].Update())
				{
					AssetBundleManager.m_InProgressOperations.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}

		public static AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, Type type)
		{
			AssetBundleManager.Log(AssetBundleManager.LogType.Info, string.Concat(new string[] { "Loading ", assetName, " from ", assetBundleName, " bundle" }));
			assetBundleName = AssetBundleManager.RemapVariantName(assetBundleName);
			AssetBundleManager.LoadAssetBundle(assetBundleName, false);
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);
			AssetBundleManager.m_InProgressOperations.Add(assetBundleLoadAssetOperation);
			return assetBundleLoadAssetOperation;
		}

		public static AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
		{
			AssetBundleManager.Log(AssetBundleManager.LogType.Info, string.Concat(new string[] { "Loading ", levelName, " from ", assetBundleName, " bundle" }));
			assetBundleName = AssetBundleManager.RemapVariantName(assetBundleName);
			AssetBundleManager.LoadAssetBundle(assetBundleName, false);
			AssetBundleLoadOperation assetBundleLoadOperation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);
			AssetBundleManager.m_InProgressOperations.Add(assetBundleLoadOperation);
			return assetBundleLoadOperation;
		}

		private static AssetBundleManager.LogMode m_LogMode = AssetBundleManager.LogMode.All;

		private static string m_BaseDownloadingURL = string.Empty;

		private static string[] m_ActiveVariants = new string[0];

		private static AssetBundleManifest m_AssetBundleManifest = null;

		private static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

		private static Dictionary<string, WWW> m_DownloadingWWWs = new Dictionary<string, WWW>();

		private static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();

		private static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation>();

		private static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

		public enum LogMode
		{
			All,
			JustErrors
		}

		public enum LogType
		{
			Info,
			Warning,
			Error
		}
	}
}
