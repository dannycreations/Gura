using System;
using System.Collections.Generic;

public class QueueFilter<T> where T : struct
{
	public QueueFilter(int maxSize, QueueFilter<T>.OperationsDelegate addF, QueueFilter<T>.OperationsDelegate decF, QueueFilter<T>.DivDelegate divF)
	{
		this._queue = new Queue<T>(maxSize);
		this._maxSize = maxSize;
		this._addF = addF;
		this._decF = decF;
		this._divF = divF;
	}

	public T MiddleValue
	{
		get
		{
			return (this._queue.Count <= 0) ? default(T) : this._divF(this._sum, this._queue.Count);
		}
	}

	public void Add(T value)
	{
		if (this._queue.Count == this._maxSize)
		{
			T t = this._queue.Dequeue();
			this._sum = this._decF(this._sum, t);
		}
		this._sum = this._addF(this._sum, value);
		this._queue.Enqueue(value);
	}

	public virtual void Clear()
	{
		this._queue.Clear();
		this._sum = default(T);
	}

	protected Queue<T> _queue;

	protected int _maxSize;

	private T _sum;

	private QueueFilter<T>.OperationsDelegate _addF;

	private QueueFilter<T>.OperationsDelegate _decF;

	private QueueFilter<T>.DivDelegate _divF;

	public delegate T OperationsDelegate(T sum, T operand);

	public delegate T DivDelegate(T sum, int count);
}
