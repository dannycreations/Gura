using System;
using System.Collections;
using System.Collections.Generic;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class LazyList<T>
	{
		public LazyList(Func<int, T> newValueCreator, int initialCount)
		{
			initialCount = ((initialCount <= 0) ? 0 : initialCount);
			this._NewValueCreator = newValueCreator;
			this.InitWithNewCount(initialCount);
		}

		public T this[int key]
		{
			get
			{
				T t = this._BackingList[key];
				if (t == null)
				{
					t = (this._BackingList[key] = this._NewValueCreator(key));
				}
				return t;
			}
			set
			{
				this._BackingList[key] = value;
			}
		}

		public int Count
		{
			get
			{
				return this._BackingList.Count;
			}
		}

		public LazyList<T>.EnumerableLazyList AsEnumerableForExistingItems
		{
			get
			{
				return new LazyList<T>.EnumerableLazyList(this);
			}
		}

		public void InitWithNewCount(int newCount)
		{
			this._BackingList = new List<T>(Array.CreateInstance(typeof(T), newCount) as T[]);
		}

		public void Add(int count)
		{
			this._BackingList.AddRange(Array.CreateInstance(typeof(T), count) as T[]);
		}

		public void InsertAtStart(int count)
		{
			this._BackingList.InsertRange(0, Array.CreateInstance(typeof(T), count) as T[]);
		}

		public void Insert(int index, int count)
		{
			this._BackingList.InsertRange(index, Array.CreateInstance(typeof(T), count) as T[]);
		}

		public void Clear()
		{
			this._BackingList.Clear();
		}

		public void Remove(T value)
		{
			this._BackingList.Remove(value);
		}

		public void RemoveAt(int index)
		{
			this._BackingList.RemoveAt(index);
		}

		private IEnumerator<T> GetEnumeratorForExistingItems()
		{
			for (int i = 0; i < this.Count; i++)
			{
				T v = this._BackingList[i];
				if (v != null)
				{
					yield return v;
				}
			}
			yield break;
		}

		private List<T> _BackingList = new List<T>();

		private Func<int, T> _NewValueCreator;

		public class EnumerableLazyList : IEnumerable<T>, IEnumerable
		{
			public EnumerableLazyList(LazyList<T> lazyList)
			{
				this._LazyList = lazyList;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return this._LazyList.GetEnumeratorForExistingItems();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			private LazyList<T> _LazyList;
		}
	}
}
