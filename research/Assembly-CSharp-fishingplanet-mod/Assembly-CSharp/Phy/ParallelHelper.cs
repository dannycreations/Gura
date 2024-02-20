using System;
using System.Threading;
using UnityEngine;

namespace Phy
{
	public class ParallelHelper<T>
	{
		public ParallelHelper()
		{
			this.threadsCount = SystemInfo.processorCount;
			this.workerThreads = new Thread[this.threadsCount];
			this.startEvents = new AutoResetEvent[this.threadsCount];
			this.finishEvents = new AutoResetEvent[this.threadsCount];
			for (int i = 0; i < this.threadsCount; i++)
			{
				this.startEvents[i] = new AutoResetEvent(false);
				this.finishEvents[i] = new AutoResetEvent(false);
				ParameterizedThreadStart parameterizedThreadStart = new ParameterizedThreadStart(this.DoWork);
				this.workerThreads[i] = new Thread(parameterizedThreadStart);
				this.workerThreads[i].Start(i);
			}
		}

		protected override void Finalize()
		{
			try
			{
				for (int i = 0; i < this.threadsCount; i++)
				{
					this.workerThreads[i].Abort();
				}
			}
			finally
			{
				base.Finalize();
			}
		}

		public void ParallelForEach(T[] array, Action<T> action)
		{
			this.arrayToIterate = array;
			this.actionToCall = action;
			foreach (AutoResetEvent autoResetEvent in this.startEvents)
			{
				autoResetEvent.Set();
			}
			for (int j = 0; j < this.threadsCount; j++)
			{
				if (!this.finishEvents[j].WaitOne(10000))
				{
					throw new Exception("Thread #" + j + " has not returned in 10s");
				}
			}
		}

		private void DoWork(object o)
		{
			int num = (int)o;
			for (;;)
			{
				bool flag = false;
				try
				{
					if (this.startEvents[num].WaitOne(1000))
					{
						flag = true;
						for (int i = num; i < this.arrayToIterate.Length; i += this.threadsCount)
						{
							this.actionToCall(this.arrayToIterate[i]);
						}
					}
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception)
				{
				}
				finally
				{
					if (flag)
					{
						this.finishEvents[num].Set();
					}
				}
			}
		}

		private readonly int threadsCount;

		private readonly Thread[] workerThreads;

		private readonly AutoResetEvent[] startEvents;

		private readonly AutoResetEvent[] finishEvents;

		private T[] arrayToIterate;

		private Action<T> actionToCall;
	}
}
