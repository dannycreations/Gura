using System;
using System.Collections.Generic;

public class CleverPool<T> where T : class, CleverPool<T>.IItem, new()
{
	public CleverPool(int size, int maxId = 32000)
	{
		this._maxUniqueId = maxId;
		this._pool = new T[size];
		for (int i = 0; i < size; i++)
		{
			this._pool[i] = new T();
		}
	}

	public int Count
	{
		get
		{
			return this._curCount;
		}
	}

	protected T GetObjectToAdd()
	{
		if (this._curCount == this._pool.Length)
		{
			throw new Exception("pool size exceeded");
		}
		do
		{
			this._lastUniqueId = ((this._lastUniqueId != this._maxUniqueId) ? (this._lastUniqueId + 1) : 1);
		}
		while (this._usedIds.Contains(this._lastUniqueId));
		T t = this._pool[this._curCount++];
		this._usedIds.Add(this._lastUniqueId);
		t.SetId(this._lastUniqueId);
		return t;
	}

	public void Remove(int id)
	{
		for (int i = 0; i < this._curCount; i++)
		{
			if (this._pool[i].Id == id)
			{
				if (i < this._curCount - 1)
				{
					this._pool[i].Clone(this._pool[this._curCount - 1]);
				}
				this._curCount--;
				this._usedIds.Remove(id);
				break;
			}
		}
	}

	public void Clear()
	{
		this._curCount = 0;
		this._usedIds.Clear();
	}

	public void Sync(CleverPool<T> src)
	{
		this._usedIds.Clear();
		for (int i = 0; i < src._curCount; i++)
		{
			this._usedIds.Add(src._pool[i].Id);
			this._pool[i].Clone(src._pool[i]);
		}
		this._curCount = src._curCount;
		this._lastUniqueId = src._lastUniqueId;
	}

	public T this[int i]
	{
		get
		{
			return this._pool[i];
		}
	}

	public int FindIndex(Func<T, bool> f)
	{
		for (int i = 0; i < this._curCount; i++)
		{
			if (f(this._pool[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public bool Any(Func<T, bool> f)
	{
		return this.FindIndex(f) != -1;
	}

	private int _maxUniqueId;

	private int _lastUniqueId;

	private HashSet<int> _usedIds = new HashSet<int>();

	protected T[] _pool;

	protected int _curCount;

	public interface IItem
	{
		void Clone(T target);

		void SetId(int id);

		int Id { get; }
	}
}
