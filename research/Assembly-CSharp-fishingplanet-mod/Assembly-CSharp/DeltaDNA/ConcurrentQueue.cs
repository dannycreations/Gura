using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	internal class ConcurrentQueue<T>
	{
		public int Count
		{
			get
			{
				object obj = this.queueLock;
				int count;
				lock (obj)
				{
					count = this.queue.Count;
				}
				return count;
			}
		}

		public T Peek()
		{
			object obj = this.queueLock;
			T t;
			lock (obj)
			{
				t = this.queue.Peek();
			}
			return t;
		}

		public void Enqueue(T obj)
		{
			object obj2 = this.queueLock;
			lock (obj2)
			{
				this.queue.Enqueue(obj);
			}
		}

		public T Dequeue()
		{
			object obj = this.queueLock;
			T t;
			lock (obj)
			{
				t = this.queue.Dequeue();
			}
			return t;
		}

		public void Clear()
		{
			object obj = this.queueLock;
			lock (obj)
			{
				this.queue.Clear();
			}
		}

		private readonly object queueLock = new object();

		private Queue<T> queue = new Queue<T>();
	}
}
