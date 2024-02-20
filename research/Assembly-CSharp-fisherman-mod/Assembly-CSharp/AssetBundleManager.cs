using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class AssetBundleManager
{
	public static IEnumerator LoadSkyFromBundle(string assetBundleName, string assetSkyName, string assetLightName, Action<Object, Object, AssetBundle> result)
	{
		string path = string.Empty;
		path = AssetBundleManager.GetPathToAssetBundle();
		AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(path, assetBundleName));
		yield return bundleLoadRequest;
		AssetBundle myLoadedAssetBundle = bundleLoadRequest.assetBundle;
		if (myLoadedAssetBundle == null)
		{
			Debug.Log("Failed to load AssetBundle!");
			yield break;
		}
		AssetBundleRequest assetSkyLoadRequest = myLoadedAssetBundle.LoadAssetAsync<GameObject>(assetSkyName);
		yield return assetSkyLoadRequest;
		AssetBundleRequest assetLightLoadRequest = myLoadedAssetBundle.LoadAssetAsync<GameObject>(assetLightName);
		yield return assetLightLoadRequest;
		result(assetSkyLoadRequest.asset, assetLightLoadRequest.asset, myLoadedAssetBundle);
		Debug.Log(string.Format("AssetBundle {0}  was downloaded", assetBundleName));
		yield break;
	}

	public static IEnumerator LoadAssetBundleFromDisk(string assetBundleName, Action<AssetBundle> result)
	{
		string path = string.Empty;
		path = AssetBundleManager.GetPathToAssetBundle();
		AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(path, assetBundleName));
		yield return bundleLoadRequest;
		AssetBundle myLoadedAssetBundle = bundleLoadRequest.assetBundle;
		if (myLoadedAssetBundle == null)
		{
			Debug.Log("Failed to load AssetBundle!");
			yield break;
		}
		result(myLoadedAssetBundle);
		Debug.Log(string.Format("AssetBundle {0}  was downloaded", assetBundleName));
		yield break;
	}

	public static IEnumerator LoadFromExisting(AssetBundle myLoadedAssetBundle, string assetName, Action<Object> result)
	{
		AssetBundleRequest assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<Sprite>(assetName);
		yield return assetLoadRequest;
		result(assetLoadRequest.asset);
		yield break;
	}

	public static IEnumerator LoadPrefabFromExisting(AssetBundle myLoadedAssetBundle, string assetName, Action<Object> result)
	{
		AssetBundleRequest assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<GameObject>(assetName);
		yield return assetLoadRequest;
		result(assetLoadRequest.asset);
		yield break;
	}

	public static IEnumerator LoadAssetFromBundle(string assetBundleName, string assetName, Action<Object, AssetBundle> result)
	{
		string path = string.Empty;
		path = AssetBundleManager.GetPathToAssetBundle();
		WWW wwwManifest = new WWW(path + "/" + AssetBundleManager.GetManifestName());
		yield return wwwManifest;
		AssetBundle manifestBundle = wwwManifest.assetBundle;
		AssetBundleManifest manifest = manifestBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
		manifestBundle.Unload(false);
		string[] dependentAssetBundles = manifest.GetAllDependencies(assetBundleName);
		foreach (string text in dependentAssetBundles)
		{
			Debug.Log(text);
		}
		AssetBundle[] assetBundles = new AssetBundle[dependentAssetBundles.Length];
		for (int i = 0; i < dependentAssetBundles.Length; i++)
		{
			string dependentAssetBundle = dependentAssetBundles[i];
			string assetBundlePath = path + "/" + dependentAssetBundle;
			Hash128 hash = manifest.GetAssetBundleHash(dependentAssetBundle);
			WWW www = WWW.LoadFromCacheOrDownload(assetBundlePath, hash);
			yield return www;
			assetBundles[i] = www.assetBundle;
		}
		WWW wwwSkyNight = WWW.LoadFromCacheOrDownload(path + "/" + assetBundleName, manifest.GetAssetBundleHash(assetBundleName));
		yield return wwwSkyNight;
		AssetBundle assetBundle = wwwSkyNight.assetBundle;
		Object sky = assetBundle.LoadAsset(assetName);
		yield return sky;
		result(sky, assetBundle);
		Debug.Log(string.Format("AssetBundle {0} asset {1} was downloaded", assetBundleName, assetName));
		yield break;
	}

	public static IEnumerator LoadAssetBundle(string assetBundleName, Action<AssetBundle> result)
	{
		string path = string.Empty;
		path = AssetBundleManager.GetPathToAssetBundle();
		WWW wwwManifest = new WWW(path + "/" + AssetBundleManager.GetManifestName());
		yield return wwwManifest;
		AssetBundle manifestBundle = wwwManifest.assetBundle;
		AssetBundleManifest manifest = manifestBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
		manifestBundle.Unload(false);
		string[] dependentAssetBundles = manifest.GetAllDependencies(assetBundleName);
		foreach (string text in dependentAssetBundles)
		{
			Debug.Log(text);
		}
		AssetBundle[] assetBundles = new AssetBundle[dependentAssetBundles.Length];
		for (int i = 0; i < dependentAssetBundles.Length; i++)
		{
			string dependentAssetBundle = dependentAssetBundles[i];
			string assetBundlePath = path + "/" + dependentAssetBundle;
			Hash128 hash = manifest.GetAssetBundleHash(dependentAssetBundle);
			WWW www = WWW.LoadFromCacheOrDownload(assetBundlePath, hash);
			yield return www;
			assetBundles[i] = www.assetBundle;
		}
		WWW wwwSkyNight = WWW.LoadFromCacheOrDownload(path + "/" + assetBundleName, manifest.GetAssetBundleHash(assetBundleName));
		yield return wwwSkyNight;
		AssetBundle assetBundle = wwwSkyNight.assetBundle;
		result(assetBundle);
		Debug.Log(string.Format("AssetBundle {0} was downloaded", assetBundleName));
		yield break;
	}

	private static string GetPathToAssetBundle()
	{
		string empty = string.Empty;
		string text = string.Empty;
		RuntimePlatform platform = Application.platform;
		switch (platform)
		{
		case 0:
		case 1:
			text = "OsX";
			break;
		case 2:
		case 7:
			text = "x32";
			break;
		default:
			if (platform != 18 && platform != 19)
			{
				switch (platform)
				{
				case 25:
					text = "PS4";
					break;
				default:
					if (platform == 13)
					{
						text = "Linux";
					}
					break;
				case 27:
					text = "XboxOne";
					break;
				}
			}
			else
			{
				text = "WSA";
			}
			break;
		}
		return Path.Combine(Application.streamingAssetsPath, Path.Combine("AssetBundles", text));
	}

	private static string GetManifestName()
	{
		string text = string.Empty;
		RuntimePlatform platform = Application.platform;
		if (platform != 1)
		{
			if (platform != 2)
			{
				if (platform == 18 || platform == 19)
				{
					return "WSA";
				}
				switch (platform)
				{
				case 25:
					return "PS4";
				default:
					if (platform != 7)
					{
						if (platform != 13)
						{
							return text;
						}
						return "Linux";
					}
					break;
				case 27:
					return "XboxOne";
				}
			}
			text = "x32";
		}
		else
		{
			text = "OsX";
		}
		return text;
	}

	private static void CopyDirectory(string sourcePath, string targetPath)
	{
		if (!Directory.Exists(targetPath))
		{
			Directory.CreateDirectory(targetPath);
		}
		if (Directory.Exists(sourcePath))
		{
			string[] files = Directory.GetFiles(sourcePath);
			foreach (string text in files)
			{
				string fileName = Path.GetFileName(text);
				string text2 = Path.Combine(targetPath, fileName);
				File.Copy(text, text2, true);
			}
		}
	}

	private static void DeleteDirectory(string directoryPath)
	{
		if (Directory.Exists(directoryPath))
		{
			try
			{
				Directory.Delete(directoryPath, true);
			}
			catch (IOException ex)
			{
			}
		}
	}
}
