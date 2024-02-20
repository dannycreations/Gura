using System;
using UnityEngine;

public class UnityPoolManager : MonoBehaviour
{
	public static UnityPoolManager Instance { get; protected set; }

	protected virtual void Awake()
	{
		UnityPoolManager.Instance = this;
		this.poolManager = new PoolManager<string, UnityPoolObject>(this.maxInstanceCount);
	}

	public virtual bool CanPush()
	{
		return this.poolManager.CanPush();
	}

	public virtual bool Push(string groupKey, UnityPoolObject poolObject)
	{
		return this.poolManager.Push(groupKey, poolObject);
	}

	public virtual T PopOrCreate<T>(T prefab) where T : UnityPoolObject
	{
		return this.PopOrCreate<T>(prefab, Vector3.zero, Quaternion.identity);
	}

	public virtual T PopOrCreate<T>(T prefab, Vector3 position, Quaternion rotation) where T : UnityPoolObject
	{
		T t = this.poolManager.Pop<T>(prefab.Group);
		if (t == null)
		{
			t = this.CreateObject<T>(prefab, position, rotation);
		}
		else
		{
			t.SetTransform(position, rotation);
		}
		return t;
	}

	public virtual UnityPoolObject Pop(string groupKey)
	{
		return this.poolManager.Pop<UnityPoolObject>(groupKey);
	}

	public virtual T Pop<T>() where T : UnityPoolObject
	{
		return this.poolManager.Pop<T>();
	}

	public virtual T Pop<T>(PoolManager<string, UnityPoolObject>.Compare<T> comparer) where T : UnityPoolObject
	{
		return this.poolManager.Pop<T>(comparer);
	}

	public virtual T Pop<T>(string groupKey) where T : UnityPoolObject
	{
		return this.poolManager.Pop<T>(groupKey);
	}

	public virtual bool Contains(string groupKey)
	{
		return this.poolManager.Contains(groupKey);
	}

	public virtual void Clear()
	{
		this.poolManager.Clear();
	}

	protected virtual T CreateObject<T>(T prefab, Vector3 position, Quaternion rotation) where T : UnityPoolObject
	{
		GameObject gameObject = Object.Instantiate<GameObject>(prefab.gameObject, position, rotation);
		T component = gameObject.GetComponent<T>();
		component.name = prefab.name;
		return component;
	}

	public int maxInstanceCount = 128;

	protected PoolManager<string, UnityPoolObject> poolManager;
}
