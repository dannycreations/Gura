using System;
using System.Collections.Generic;
using System.Linq;

public class TimerCore<T> where T : struct
{
	public TimerCore<T>.Timer AddTimer(T name, float duration, Action action = null, bool repeatedly = false)
	{
		TimerCore<T>.Timer timer = this._timers.FirstOrDefault(delegate(TimerCore<T>.Timer _)
		{
			T name2 = _.Name;
			return name2.Equals(name);
		});
		if (timer != null)
		{
			timer.Change(duration, action, repeatedly);
		}
		else
		{
			timer = new TimerCore<T>.Timer(name, duration, action, repeatedly);
			this._timers.AddFirst(timer);
		}
		return timer;
	}

	public void Clear()
	{
		this._timers.Clear();
	}

	public void RemoveTimer(T name)
	{
		TimerCore<T>.Timer timer = this._timers.FirstOrDefault(delegate(TimerCore<T>.Timer _)
		{
			T name2 = _.Name;
			return name2.Equals(name);
		});
		if (timer != null)
		{
			this._timers.Remove(timer);
		}
	}

	public bool IsTimerActive(T name)
	{
		return this._timers.Any(delegate(TimerCore<T>.Timer _)
		{
			T name2 = _.Name;
			return name2.Equals(name);
		});
	}

	public float? GetTimerLeftTime(T name)
	{
		TimerCore<T>.Timer timer = this._timers.FirstOrDefault(delegate(TimerCore<T>.Timer _)
		{
			T name2 = _.Name;
			return name2.Equals(name);
		});
		return (timer == null) ? null : new float?(timer.TimeLeft);
	}

	public void Update(float dt)
	{
		LinkedListNode<TimerCore<T>.Timer> linkedListNode = this._timers.First;
		while (linkedListNode != null)
		{
			linkedListNode.Value.Update(dt);
			if (linkedListNode.Value.IsExpired)
			{
				LinkedListNode<TimerCore<T>.Timer> next = linkedListNode.Next;
				if (!linkedListNode.Value.Repeatedly)
				{
					this._timers.Remove(linkedListNode);
				}
				else
				{
					linkedListNode.Value.Refresh();
				}
				if (linkedListNode.Value.Action != null)
				{
					linkedListNode.Value.Action();
				}
				linkedListNode = next;
			}
			else
			{
				linkedListNode = linkedListNode.Next;
			}
		}
	}

	private LinkedList<TimerCore<T>.Timer> _timers = new LinkedList<TimerCore<T>.Timer>();

	public class Timer
	{
		public Timer(T name, float duration, Action action, bool repeatedly)
		{
			this.Name = name;
			this._duration = duration;
			this._timeLeft = duration;
			this._action = action;
			this._repeatedly = repeatedly;
		}

		public bool IsExpired
		{
			get
			{
				return this._timeLeft < 0f;
			}
		}

		public float Duration
		{
			get
			{
				return this._duration;
			}
		}

		public float Prc
		{
			get
			{
				return 1f - this._timeLeft / this._duration;
			}
		}

		public float TimeLeft
		{
			get
			{
				return this._timeLeft;
			}
		}

		public bool Repeatedly
		{
			get
			{
				return this._repeatedly;
			}
		}

		public Action Action
		{
			get
			{
				return this._action;
			}
		}

		public void Change(float duration, Action action, bool repeatedly)
		{
			this._timeLeft = duration;
			this._action = action;
			this._repeatedly = repeatedly;
		}

		public void Refresh()
		{
			this._timeLeft = this._duration;
		}

		public void Update(float dt)
		{
			this._timeLeft -= dt;
		}

		public readonly T Name;

		private float _duration;

		private float _timeLeft;

		private bool _repeatedly;

		private Action _action;
	}
}
