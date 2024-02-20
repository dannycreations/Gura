using System;
using System.Collections;
using UnityEngine;

public class CacheLibrary : MonoBehaviour
{
	private void Awake()
	{
		if (CacheLibrary._instance != null)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public static bool AllChachesInited
	{
		get
		{
			return CacheLibrary.AssetsCache != null && CacheLibrary.MapCache != null && CacheLibrary.MapCache.AllMapChachesInited && CacheLibrary.AssetsCache.AllAssetsChachesInited;
		}
	}

	public static void ResetAllCaches(bool canReuseCaches)
	{
		if (CacheLibrary._instance.GetComponent<GlobalShopCache>() != null)
		{
			GlobalShopCache component = CacheLibrary._instance.GetComponent<GlobalShopCache>();
			if (canReuseCaches)
			{
				component.UnsubscribeEvents();
			}
			else
			{
				Object.Destroy(component);
			}
		}
		if (CacheLibrary._instance.GetComponent<LocalShopCache>() != null)
		{
			LocalShopCache component2 = CacheLibrary._instance.GetComponent<LocalShopCache>();
			if (canReuseCaches)
			{
				component2.UnsubscribeEvents();
			}
			else
			{
				Object.Destroy(component2);
			}
		}
		if (CacheLibrary._instance.GetComponent<ProductsCache>() != null)
		{
			ProductsCache component3 = CacheLibrary._instance.GetComponent<ProductsCache>();
			if (canReuseCaches)
			{
				component3.UnsubscribeEvents();
			}
			else
			{
				Object.Destroy(component3);
			}
		}
		if (CacheLibrary._instance.GetComponent<GlobalMapCache>() != null)
		{
			GlobalMapCache component4 = CacheLibrary._instance.GetComponent<GlobalMapCache>();
			if (canReuseCaches)
			{
				component4.UnsubscribeEvents();
			}
			else
			{
				Object.Destroy(component4);
			}
		}
		if (CacheLibrary._instance.GetComponent<AssetsCache>() != null)
		{
			AssetsCache component5 = CacheLibrary._instance.GetComponent<AssetsCache>();
			if (canReuseCaches)
			{
				component5.UnsubscribeEvents();
			}
			else
			{
				Object.Destroy(component5);
			}
		}
		if (CacheLibrary._instance.GetComponent<ItemsCache>() != null)
		{
			ItemsCache component6 = CacheLibrary._instance.GetComponent<ItemsCache>();
			if (canReuseCaches)
			{
				component6.UnsubscribeEvents();
			}
			else
			{
				Object.Destroy(component6);
			}
		}
		CacheLibrary._instance.StartCoroutine(CacheLibrary._instance.InitAllCaches());
	}

	public static void SubscribeAllCaches()
	{
		if (CacheLibrary._instance.GetComponent<GlobalShopCache>() != null)
		{
			GlobalShopCache component = CacheLibrary._instance.GetComponent<GlobalShopCache>();
			component.SubscribeEvents();
		}
		if (CacheLibrary._instance.GetComponent<LocalShopCache>() != null)
		{
			LocalShopCache component2 = CacheLibrary._instance.GetComponent<LocalShopCache>();
			component2.SubscribeEvents();
		}
		if (CacheLibrary._instance.GetComponent<ProductsCache>() != null)
		{
			ProductsCache component3 = CacheLibrary._instance.GetComponent<ProductsCache>();
			component3.SubscribeEvents();
		}
		if (CacheLibrary._instance.GetComponent<GlobalMapCache>() != null)
		{
			GlobalMapCache component4 = CacheLibrary._instance.GetComponent<GlobalMapCache>();
			component4.SubscribeEvents();
		}
		if (CacheLibrary._instance.GetComponent<AssetsCache>() != null)
		{
			AssetsCache component5 = CacheLibrary._instance.GetComponent<AssetsCache>();
			component5.SubscribeEvents();
		}
		if (CacheLibrary._instance.GetComponent<ItemsCache>() != null)
		{
			ItemsCache component6 = CacheLibrary._instance.GetComponent<ItemsCache>();
			component6.SubscribeEvents();
		}
	}

	private void Start()
	{
		CacheLibrary._instance = this;
		Object.DontDestroyOnLoad(this);
		base.StartCoroutine(this.InitAllCaches());
	}

	private IEnumerator InitAllCaches()
	{
		yield return new WaitForSeconds(0.5f);
		CacheLibrary.GlobalShopCacheInstance = ((!(base.GetComponent<GlobalShopCache>() == null)) ? base.gameObject.GetComponent<GlobalShopCache>() : base.gameObject.AddComponent<GlobalShopCache>());
		CacheLibrary.LocalCacheInstance = ((!(base.GetComponent<LocalShopCache>() == null)) ? base.gameObject.GetComponent<LocalShopCache>() : base.gameObject.AddComponent<LocalShopCache>());
		CacheLibrary.ProductCache = ((!(base.GetComponent<ProductsCache>() == null)) ? base.gameObject.GetComponent<ProductsCache>() : base.gameObject.AddComponent<ProductsCache>());
		CacheLibrary.MapCache = ((!(base.GetComponent<GlobalMapCache>() == null)) ? base.gameObject.GetComponent<GlobalMapCache>() : base.gameObject.AddComponent<GlobalMapCache>());
		CacheLibrary.AssetsCache = ((!(base.GetComponent<AssetsCache>() == null)) ? base.gameObject.GetComponent<AssetsCache>() : base.gameObject.AddComponent<AssetsCache>());
		CacheLibrary.ItemsCache = ((!(base.GetComponent<ItemsCache>() == null)) ? base.gameObject.GetComponent<ItemsCache>() : base.gameObject.AddComponent<ItemsCache>());
		yield break;
	}

	private static CacheLibrary _instance;

	[HideInInspector]
	public static GlobalShopCache GlobalShopCacheInstance;

	[HideInInspector]
	public static LocalShopCache LocalCacheInstance;

	[HideInInspector]
	public static ProductsCache ProductCache;

	[HideInInspector]
	public static GlobalMapCache MapCache;

	[HideInInspector]
	public static AssetsCache AssetsCache;

	[HideInInspector]
	public static ItemsCache ItemsCache;
}
