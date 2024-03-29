﻿using System;
using System.Collections.Generic;

public class ListQueue<T> : List<T>
{
	public new void Add(T item)
	{
		throw new NotSupportedException();
	}

	public new void AddRange(IEnumerable<T> collection)
	{
		throw new NotSupportedException();
	}

	public new void Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	public new void InsertRange(int index, IEnumerable<T> collection)
	{
		throw new NotSupportedException();
	}

	public new void Reverse()
	{
		throw new NotSupportedException();
	}

	public new void Reverse(int index, int count)
	{
		throw new NotSupportedException();
	}

	public new void Sort()
	{
		throw new NotSupportedException();
	}

	public new void Sort(Comparison<T> comparison)
	{
		throw new NotSupportedException();
	}

	public new void Sort(IComparer<T> comparer)
	{
		throw new NotSupportedException();
	}

	public new void Sort(int index, int count, IComparer<T> comparer)
	{
		throw new NotSupportedException();
	}

	public void Enqueue(T item)
	{
		base.Add(item);
	}

	public T Dequeue()
	{
		T t = base[0];
		base.RemoveAt(0);
		return t;
	}

	public T Peek()
	{
		return base[0];
	}
}
