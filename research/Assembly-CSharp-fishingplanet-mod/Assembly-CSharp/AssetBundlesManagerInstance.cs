using System;
using System.Collections;
using AssetBundles;
using UnityEngine;

public class AssetBundlesManagerInstance : MonoBehaviour
{
	private IEnumerator Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		global::AssetBundles.AssetBundleManager.SetSourceAssetBundleDirectory("AssetBundles/");
		yield return base.StartCoroutine(this.Initialize());
		yield break;
	}

	protected IEnumerator Initialize()
	{
		AssetBundleLoadManifestOperation request = global::AssetBundles.AssetBundleManager.Initialize();
		if (request != null)
		{
			yield return base.StartCoroutine(request);
		}
		yield break;
	}
}
