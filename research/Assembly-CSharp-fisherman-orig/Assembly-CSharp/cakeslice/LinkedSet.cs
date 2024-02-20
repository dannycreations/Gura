using System;
using System.Collections;
using System.Collections.Generic;

namespace cakeslice
{
	public class LinkedSet<T> : IEnumerable<T>, IEnumerable
	{
		public LinkedSet()
		{
			this.list = new LinkedList<T>();
			this.dictionary = new Dictionary<T, LinkedListNode<T>>();
		}

		public LinkedSet(IEqualityComparer<T> comparer)
		{
			this.list = new LinkedList<T>();
			this.dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
		}

		public bool Contains(T t)
		{
			return this.dictionary.ContainsKey(t);
		}

		public bool Add(T t)
		{
			if (this.dictionary.ContainsKey(t))
			{
				return false;
			}
			LinkedListNode<T> linkedListNode = this.list.AddLast(t);
			this.dictionary.Add(t, linkedListNode);
			return true;
		}

		public void Clear()
		{
			this.list.Clear();
			this.dictionary.Clear();
		}

		public LinkedSet<T>.AddType AddOrMoveToEnd(T t)
		{
			if (this.dictionary.Comparer.Equals(t, this.list.Last.Value))
			{
				return LinkedSet<T>.AddType.NO_CHANGE;
			}
			LinkedListNode<T> linkedListNode;
			if (this.dictionary.TryGetValue(t, out linkedListNode))
			{
				this.list.Remove(linkedListNode);
				linkedListNode = this.list.AddLast(t);
				this.dictionary[t] = linkedListNode;
				return LinkedSet<T>.AddType.MOVED;
			}
			linkedListNode = this.list.AddLast(t);
			this.dictionary[t] = linkedListNode;
			return LinkedSet<T>.AddType.ADDED;
		}

		public bool Remove(T t)
		{
			LinkedListNode<T> linkedListNode;
			if (this.dictionary.TryGetValue(t, out linkedListNode) && this.dictionary.Remove(t))
			{
				this.list.Remove(linkedListNode);
				return true;
			}
			return false;
		}

		public void ExceptWith(IEnumerable<T> enumerable)
		{
			foreach (T t in enumerable)
			{
				this.Remove(t);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		private LinkedList<T> list;

		private Dictionary<T, LinkedListNode<T>> dictionary;

		public enum AddType
		{
			NO_CHANGE,
			ADDED,
			MOVED
		}
	}
}
