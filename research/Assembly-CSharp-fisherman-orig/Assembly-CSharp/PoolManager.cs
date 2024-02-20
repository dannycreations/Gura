using System;
using System.Collections.Generic;

public class PoolManager<K, V> where V : IPoolObject<K>
{
	public PoolManager(int maxInstance)
	{
		this.MaxInstances = maxInstance;
		this.objects = new Dictionary<K, List<V>>();
		this.cache = new Dictionary<Type, List<V>>();
	}

	public virtual int MaxInstances { get; protected set; }

	public virtual int InctanceCount
	{
		get
		{
			return this.objects.Count;
		}
	}

	public virtual int CacheCount
	{
		get
		{
			return this.cache.Count;
		}
	}

	public virtual bool CanPush()
	{
		return this.InctanceCount + 1 < this.MaxInstances;
	}

	public virtual bool Push(K groupKey, V value)
	{
		bool flag = false;
		if (this.CanPush())
		{
			value.OnPush();
			if (!this.objects.ContainsKey(groupKey))
			{
				this.objects.Add(groupKey, new List<V>());
			}
			this.objects[groupKey].Add(value);
			Type type = value.GetType();
			if (!this.cache.ContainsKey(type))
			{
				this.cache.Add(type, new List<V>());
			}
			this.cache[type].Add(value);
		}
		else
		{
			value.FailedPush();
		}
		return flag;
	}

	public virtual T Pop<T>(K groupKey) where T : V
	{
		T t = default(T);
		if (this.Contains(groupKey) && this.objects[groupKey].Count > 0)
		{
			for (int i = 0; i < this.objects[groupKey].Count; i++)
			{
				if (this.objects[groupKey][i] is T)
				{
					t = (T)((object)this.objects[groupKey][i]);
					Type type = t.GetType();
					this.RemoveObject(groupKey, i);
					this.RemoveFromCache((V)((object)t), type);
					t.Create();
					break;
				}
			}
		}
		return t;
	}

	public virtual T Pop<T>() where T : V
	{
		T t = default(T);
		Type typeFromHandle = typeof(T);
		if (this.ValidateForPop(typeFromHandle))
		{
			for (int i = 0; i < this.cache[typeFromHandle].Count; i++)
			{
				t = (T)((object)this.cache[typeFromHandle][i]);
				if (t != null && this.objects.ContainsKey(t.Group))
				{
					this.objects[t.Group].Remove((V)((object)t));
					this.RemoveFromCache((V)((object)t), typeFromHandle);
					t.Create();
					break;
				}
			}
		}
		return t;
	}

	public virtual T Pop<T>(PoolManager<K, V>.Compare<T> comparer) where T : V
	{
		T t = default(T);
		Type typeFromHandle = typeof(T);
		if (this.ValidateForPop(typeFromHandle))
		{
			for (int i = 0; i < this.cache[typeFromHandle].Count; i++)
			{
				T t2 = (T)((object)this.cache[typeFromHandle][i]);
				if (comparer(t2))
				{
					this.objects[t2.Group].Remove((V)((object)t2));
					this.RemoveFromCache((V)((object)t), typeFromHandle);
					t = t2;
					t.Create();
					break;
				}
			}
		}
		return t;
	}

	public virtual bool Contains(K groupKey)
	{
		return this.objects.ContainsKey(groupKey);
	}

	public virtual void Clear()
	{
		this.objects.Clear();
	}

	protected virtual bool ValidateForPop(Type type)
	{
		return this.cache.ContainsKey(type) && this.cache[type].Count > 0;
	}

	protected virtual void RemoveObject(K groupKey, int idx)
	{
		if (idx >= 0 && idx < this.objects[groupKey].Count)
		{
			this.objects[groupKey].RemoveAt(idx);
			if (this.objects[groupKey].Count == 0)
			{
				this.objects.Remove(groupKey);
			}
		}
	}

	protected void RemoveFromCache(V value, Type type)
	{
		if (this.cache.ContainsKey(type))
		{
			this.cache[type].Remove(value);
			if (this.cache[type].Count == 0)
			{
				this.cache.Remove(type);
			}
		}
	}

	protected Dictionary<K, List<V>> objects;

	protected Dictionary<Type, List<V>> cache;

	public delegate bool Compare<T>(T value) where T : V;
}
