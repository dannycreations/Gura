using System;

public class QueuePool<T> where T : class, QueuePool<T>.IItem, new()
{
	public QueuePool(int size)
	{
		this._pool = new T[size];
		for (int i = 0; i < size; i++)
		{
			this._pool[i] = new T();
		}
	}

	public void Enqueue(T obj)
	{
		if (this._start == -1)
		{
			this._start = 0;
			this._end = 0;
			this._pool[this._start].Clone(obj);
		}
		else
		{
			if (++this._end == this._pool.Length)
			{
				this._end = 0;
			}
			this._pool[this._end].Clone(obj);
			if (this._start == this._end && ++this._start == this._pool.Length)
			{
				this._start = 0;
			}
		}
	}

	public T Peek()
	{
		return (this._start != -1) ? this._pool[this._start] : ((T)((object)null));
	}

	public T Dequeue()
	{
		if (this._start == -1)
		{
			return (T)((object)null);
		}
		T t = this._pool[this._start];
		if (this._start == this._end)
		{
			this._start = -1;
		}
		else
		{
			this._start++;
			if (this._start == this._pool.Length)
			{
				this._start = 0;
			}
		}
		return t;
	}

	public bool IsEmpty
	{
		get
		{
			return this._start == -1;
		}
	}

	public int Count
	{
		get
		{
			if (this._start == -1)
			{
				return 0;
			}
			if (this._end >= this._start)
			{
				return this._end - this._start + 1;
			}
			return this._pool.Length - this._start + this._end + 1;
		}
	}

	public void Clear()
	{
		this._start = -1;
	}

	private T[] _pool;

	private int _start = -1;

	private int _end;

	public interface IItem
	{
		void Clone(T obj);
	}
}
