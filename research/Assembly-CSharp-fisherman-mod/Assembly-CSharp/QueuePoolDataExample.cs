using System;

public class QueuePoolDataExample : QueuePool<QueuePoolDataExample>.IItem
{
	public QueuePoolDataExample()
	{
		this._value = 0;
	}

	public void Replace()
	{
		this._value = ++QueuePoolDataExample._counter;
	}

	public void Clone(QueuePoolDataExample obj)
	{
		this._value = obj._value;
	}

	public override string ToString()
	{
		return string.Format("_value = {0}, _count = {1}", this._value, QueuePoolDataExample._counter);
	}

	private static int _counter;

	private int _value;
}
