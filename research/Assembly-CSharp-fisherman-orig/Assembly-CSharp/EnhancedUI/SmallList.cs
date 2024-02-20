using System;
using UnityEngine;

namespace EnhancedUI
{
	public class SmallList<T>
	{
		public T this[int i]
		{
			get
			{
				return this.data[i];
			}
			set
			{
				this.data[i] = value;
			}
		}

		private void ResizeArray()
		{
			T[] array;
			if (this.data != null)
			{
				array = new T[Mathf.Max(this.data.Length << 1, 64)];
			}
			else
			{
				array = new T[64];
			}
			if (this.data != null && this.Count > 0)
			{
				this.data.CopyTo(array, 0);
			}
			this.data = array;
		}

		public void Clear()
		{
			this.Count = 0;
		}

		public T First()
		{
			if (this.data == null || this.Count == 0)
			{
				return default(T);
			}
			return this.data[0];
		}

		public T Last()
		{
			if (this.data == null || this.Count == 0)
			{
				return default(T);
			}
			return this.data[this.Count - 1];
		}

		public void Add(T item)
		{
			if (this.data == null || this.Count == this.data.Length)
			{
				this.ResizeArray();
			}
			this.data[this.Count] = item;
			this.Count++;
		}

		public void AddStart(T item)
		{
			this.Insert(item, 0);
		}

		public void Insert(T item, int index)
		{
			if (this.data == null || this.Count == this.data.Length)
			{
				this.ResizeArray();
			}
			for (int i = this.Count; i > index; i--)
			{
				this.data[i] = this.data[i - 1];
			}
			this.data[index] = item;
			this.Count++;
		}

		public T RemoveStart()
		{
			return this.RemoveAt(0);
		}

		public T RemoveAt(int index)
		{
			if (this.data != null && this.Count != 0)
			{
				T t = this.data[index];
				for (int i = index; i < this.Count - 1; i++)
				{
					this.data[i] = this.data[i + 1];
				}
				this.Count--;
				this.data[this.Count] = default(T);
				return t;
			}
			return default(T);
		}

		public T Remove(T item)
		{
			if (this.data != null && this.Count != 0)
			{
				for (int i = 0; i < this.Count; i++)
				{
					if (this.data[i].Equals(item))
					{
						return this.RemoveAt(i);
					}
				}
			}
			return default(T);
		}

		public T RemoveEnd()
		{
			if (this.data != null && this.Count != 0)
			{
				this.Count--;
				T t = this.data[this.Count];
				this.data[this.Count] = default(T);
				return t;
			}
			return default(T);
		}

		public bool Contains(T item)
		{
			if (this.data == null)
			{
				return false;
			}
			for (int i = 0; i < this.Count; i++)
			{
				if (this.data[i].Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		public T[] data;

		public int Count;
	}
}
