using System;
using UnityEngine;

namespace DeltaDNA
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance
		{
			get
			{
				if (Singleton<T>.applicationIsQuitting)
				{
					Logger.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
					return (T)((object)null);
				}
				object @lock = Singleton<T>._lock;
				T instance;
				lock (@lock)
				{
					if (Singleton<T>._instance == null)
					{
						Singleton<T>._instance = (T)((object)Object.FindObjectOfType(typeof(T)));
						if (Object.FindObjectsOfType(typeof(T)).Length > 1)
						{
							Logger.LogWarning("[Singleton] Something went really wrong  - there should never be more than 1 singleton! Reopening the scene might fix it.");
							return Singleton<T>._instance;
						}
						if (Singleton<T>._instance == null)
						{
							GameObject gameObject = new GameObject();
							Singleton<T>._instance = gameObject.AddComponent<T>();
							gameObject.name = typeof(T).ToString();
							Object.DontDestroyOnLoad(gameObject);
						}
					}
					instance = Singleton<T>._instance;
				}
				return instance;
			}
		}

		public virtual void OnDestroy()
		{
			Singleton<T>.applicationIsQuitting = true;
		}

		private static T _instance;

		private static object _lock = new object();

		private static bool applicationIsQuitting = false;
	}
}
