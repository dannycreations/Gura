using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Malee
{
	[Serializable]
	public abstract class ReorderableArray<T> : ICloneable, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		public ReorderableArray()
			: this(0)
		{
		}

		public ReorderableArray(int length)
		{
			this.array = new List<T>(length);
		}

		public T this[int index]
		{
			get
			{
				return this.array[index];
			}
			set
			{
				this.array[index] = value;
			}
		}

		public int Length
		{
			get
			{
				return this.array.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public int Count
		{
			get
			{
				return this.array.Count;
			}
		}

		public object Clone()
		{
			return new List<T>(this.array);
		}

		public void CopyFrom(IEnumerable<T> value)
		{
			this.array.Clear();
			this.array.AddRange(value);
		}

		public bool Contains(T value)
		{
			return this.array.Contains(value);
		}

		public int IndexOf(T value)
		{
			return this.array.IndexOf(value);
		}

		public void Insert(int index, T item)
		{
			this.array.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			this.array.RemoveAt(index);
		}

		public void Add(T item)
		{
			this.array.Add(item);
		}

		public void Clear()
		{
			this.array.Clear();
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this.array.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return this.array.Remove(item);
		}

		public T[] ToArray()
		{
			return this.array.ToArray();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.array.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.array.GetEnumerator();
		}

		[SerializeField]
		private List<T> array = new List<T>();
	}
}
