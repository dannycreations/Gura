using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace I2.Loc
{
	public class ResourceManager : MonoBehaviour
	{
		public static ResourceManager pInstance
		{
			get
			{
				bool flag = ResourceManager.mInstance == null;
				if (ResourceManager.mInstance == null)
				{
					ResourceManager.mInstance = (ResourceManager)Object.FindObjectOfType(typeof(ResourceManager));
				}
				if (ResourceManager.mInstance == null)
				{
					GameObject gameObject = new GameObject("I2ResourceManager", new Type[] { typeof(ResourceManager) });
					gameObject.hideFlags |= 61;
					ResourceManager.mInstance = gameObject.GetComponent<ResourceManager>();
					SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(ResourceManager.MyOnLevelWasLoaded);
				}
				if (flag && Application.isPlaying)
				{
					Object.DontDestroyOnLoad(ResourceManager.mInstance.gameObject);
				}
				return ResourceManager.mInstance;
			}
		}

		public static void MyOnLevelWasLoaded(Scene scene, LoadSceneMode mode)
		{
			LocalizationManager.UpdateSources();
		}

		public T GetAsset<T>(string Name) where T : Object
		{
			T t = this.FindAsset(Name) as T;
			if (t != null)
			{
				return t;
			}
			return this.LoadFromResources<T>(Name);
		}

		private Object FindAsset(string Name)
		{
			if (this.Assets != null)
			{
				int i = 0;
				int num = this.Assets.Length;
				while (i < num)
				{
					if (this.Assets[i] != null && this.Assets[i].name == Name)
					{
						return this.Assets[i];
					}
					i++;
				}
			}
			return null;
		}

		public bool HasAsset(Object Obj)
		{
			return this.Assets != null && Array.IndexOf<Object>(this.Assets, Obj) >= 0;
		}

		public T LoadFromResources<T>(string Path) where T : Object
		{
			if (string.IsNullOrEmpty(Path))
			{
				return (T)((object)null);
			}
			Object @object;
			if (this.mResourcesCache.TryGetValue(Path, out @object) && @object != null)
			{
				return @object as T;
			}
			T t = (T)((object)null);
			if (Path.EndsWith("]", StringComparison.OrdinalIgnoreCase))
			{
				int num = Path.LastIndexOf("[", StringComparison.OrdinalIgnoreCase);
				int num2 = Path.Length - num - 2;
				string text = Path.Substring(num + 1, num2);
				Path = Path.Substring(0, num);
				T[] array = Resources.LoadAll<T>(Path);
				int i = 0;
				int num3 = array.Length;
				while (i < num3)
				{
					if (array[i].name.Equals(text))
					{
						t = array[i];
						break;
					}
					i++;
				}
			}
			else
			{
				t = Resources.Load<T>(Path);
			}
			if (t == null)
			{
				t = this.LoadFromBundle<T>(Path);
			}
			if (t != null)
			{
				this.mResourcesCache[Path] = t;
			}
			if (!this.mCleaningScheduled)
			{
				base.Invoke("CleanResourceCache", 0.1f);
				this.mCleaningScheduled = true;
			}
			return t;
		}

		public T LoadFromBundle<T>(string path) where T : Object
		{
			int i = 0;
			int count = this.mBundleManagers.Count;
			while (i < count)
			{
				if (this.mBundleManagers[i] != null)
				{
					T t = this.mBundleManagers[i].LoadFromBundle<T>(path);
					if (t != null)
					{
						return t;
					}
				}
				i++;
			}
			return (T)((object)null);
		}

		public void CleanResourceCache()
		{
			this.mResourcesCache.Clear();
			Resources.UnloadUnusedAssets();
			base.CancelInvoke();
			this.mCleaningScheduled = false;
		}

		private static ResourceManager mInstance;

		public List<IResourceManager_Bundles> mBundleManagers = new List<IResourceManager_Bundles>();

		public Object[] Assets;

		private Dictionary<string, Object> mResourcesCache = new Dictionary<string, Object>();

		private bool mCleaningScheduled;
	}
}
