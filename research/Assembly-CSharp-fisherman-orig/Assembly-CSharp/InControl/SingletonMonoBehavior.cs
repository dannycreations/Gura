using System;
using UnityEngine;

namespace InControl
{
	public abstract class SingletonMonoBehavior<T, P> : MonoBehaviour where T : MonoBehaviour where P : MonoBehaviour
	{
		public static T Instance
		{
			get
			{
				return SingletonMonoBehavior<T, P>.GetInstance();
			}
		}

		private static void CreateInstance()
		{
			GameObject gameObject;
			if (typeof(P) == typeof(MonoBehaviour))
			{
				gameObject = new GameObject();
				gameObject.name = typeof(T).Name;
			}
			else
			{
				P p = Object.FindObjectOfType<P>();
				if (!p)
				{
					Debug.LogError("Could not find object with required component " + typeof(P).Name);
					return;
				}
				gameObject = p.gameObject;
			}
			Debug.Log("Creating instance of singleton component " + typeof(T).Name);
			SingletonMonoBehavior<T, P>.instance = gameObject.AddComponent<T>();
			SingletonMonoBehavior<T, P>.hasInstance = true;
		}

		private static T GetInstance()
		{
			object obj = SingletonMonoBehavior<T, P>.lockObject;
			T t;
			lock (obj)
			{
				if (SingletonMonoBehavior<T, P>.hasInstance)
				{
					t = SingletonMonoBehavior<T, P>.instance;
				}
				else
				{
					Type typeFromHandle = typeof(T);
					T[] array = Object.FindObjectsOfType<T>();
					if (array.Length > 0)
					{
						SingletonMonoBehavior<T, P>.instance = array[0];
						SingletonMonoBehavior<T, P>.hasInstance = true;
						if (array.Length > 1)
						{
							Debug.LogWarning("Multiple instances of singleton " + typeFromHandle + " found; destroying all but the first.");
							for (int i = 1; i < array.Length; i++)
							{
								Object.DestroyImmediate(array[i].gameObject);
							}
						}
						t = SingletonMonoBehavior<T, P>.instance;
					}
					else
					{
						SingletonPrefabAttribute singletonPrefabAttribute = Attribute.GetCustomAttribute(typeFromHandle, typeof(SingletonPrefabAttribute)) as SingletonPrefabAttribute;
						if (singletonPrefabAttribute == null)
						{
							SingletonMonoBehavior<T, P>.CreateInstance();
						}
						else
						{
							string name = singletonPrefabAttribute.Name;
							GameObject gameObject = Object.Instantiate<GameObject>(Resources.Load<GameObject>(name));
							if (gameObject == null)
							{
								Debug.LogError(string.Concat(new object[] { "Could not find prefab ", name, " for singleton of type ", typeFromHandle, "." }));
								SingletonMonoBehavior<T, P>.CreateInstance();
							}
							else
							{
								gameObject.name = name;
								SingletonMonoBehavior<T, P>.instance = gameObject.GetComponent<T>();
								if (SingletonMonoBehavior<T, P>.instance == null)
								{
									Debug.LogWarning(string.Concat(new object[] { "There wasn't a component of type \"", typeFromHandle, "\" inside prefab \"", name, "\"; creating one now." }));
									SingletonMonoBehavior<T, P>.instance = gameObject.AddComponent<T>();
									SingletonMonoBehavior<T, P>.hasInstance = true;
								}
							}
						}
						t = SingletonMonoBehavior<T, P>.instance;
					}
				}
			}
			return t;
		}

		protected bool EnforceSingleton()
		{
			object obj = SingletonMonoBehavior<T, P>.lockObject;
			lock (obj)
			{
				if (SingletonMonoBehavior<T, P>.hasInstance)
				{
					T[] array = Object.FindObjectsOfType<T>();
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].GetInstanceID() != SingletonMonoBehavior<T, P>.instance.GetInstanceID())
						{
							Object.DestroyImmediate(array[i].gameObject);
						}
					}
				}
			}
			int instanceID = base.GetInstanceID();
			T t = SingletonMonoBehavior<T, P>.Instance;
			return instanceID == t.GetInstanceID();
		}

		protected bool EnforceSingletonComponent()
		{
			object obj = SingletonMonoBehavior<T, P>.lockObject;
			lock (obj)
			{
				if (SingletonMonoBehavior<T, P>.hasInstance && base.GetInstanceID() != SingletonMonoBehavior<T, P>.instance.GetInstanceID())
				{
					Object.DestroyImmediate(this);
					return false;
				}
			}
			return true;
		}

		private void OnDestroy()
		{
			SingletonMonoBehavior<T, P>.hasInstance = false;
		}

		private static T instance;

		private static bool hasInstance;

		private static object lockObject = new object();
	}
}
